
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.OrderDTOs;
using TravelAgencyFrontendAPI.ECPay.Services;
using TravelAgencyFrontendAPI.ECPay.Models;

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly ECPayService _ecpayService;

        public OrderController(AppDbContext context, ILogger<OrderController> logger, ECPayService ecpayService)
        {
            _context = context;
            _logger = logger;
            _ecpayService = ecpayService;
        }

        // POST: api/Order/initiate
        // << ��B�q��إߡv >>
        /// <summary>
        /// �B�J1�G�إߪ�B�q�� (���t�I�ڤ覡�M�o����T)
        /// </summary>
        /// <param name="orderCreateDto">�q��إ߸�T (���t�I�کM�o��)</param>
        /// <returns>��B�q��K�n�A�]�tOrderId, MerchantTradeNo, ExpiresAt</returns>
        [HttpPost("initiate")]
        public async Task<ActionResult<OrderSummaryDto>> InitiateOrderAsync([FromBody] OrderCreateDto orderCreateDto)
        {
            _logger.LogInformation("�������B�q��إ߽ШD: {@OrderCreateDto}", orderCreateDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("��B�q��ҫ����ҥ���: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var memberIdFromDto = orderCreateDto.MemberId;
            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == memberIdFromDto);
            if (member == null)
            {
                _logger.LogWarning("CreateOrder �䤣��|���A�|�� ID (�Ӧ۫e��DTO): {MemberId}", memberIdFromDto);
                ModelState.AddModelError(nameof(orderCreateDto.MemberId), $"���Ѫ��|�� ID {memberIdFromDto} ���s�b�C");
                return BadRequest(ModelState);
            }

            // << �s�W�G���Ͱߤ@���ө�����s�� (MerchantTradeNo) ���ܦ��B >>
            // �T�O���s�����ߤ@�ʡA��ɭn�D20�X���^�Ʀr
            string tempGuidPart = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string timePartForMtn = DateTime.Now.ToString("yyMMddHHmmss"); // ��T���A��֭��ƾ��v
            string prefixForMtn = "TRVORD"; // �z���ө��e��
            string mtnBase = $"{prefixForMtn}{timePartForMtn}{tempGuidPart}";
            string merchantTradeNo = new string(mtnBase.Where(char.IsLetterOrDigit).ToArray());
            if (merchantTradeNo.Length > 20)
            {
                merchantTradeNo = merchantTradeNo.Substring(0, 20);
            }
            _logger.LogInformation("���q�沣�ͪ� MerchantTradeNo: {MerchantTradeNo}", merchantTradeNo);


            // ��l�� Order ����
            var order = new Order
            {
                MemberId = member.MemberId,
                OrdererName = orderCreateDto.OrdererInfo.Name,
                OrdererPhone = NormalizePhoneNumber(orderCreateDto.OrdererInfo.MobilePhone),
                OrdererEmail = orderCreateDto.OrdererInfo.Email,
                OrdererNationality = orderCreateDto.OrdererInfo.Nationality,
                OrdererDocumentNumber = orderCreateDto.OrdererInfo.DocumentNumber,
                OrdererDocumentType = orderCreateDto.OrdererInfo.DocumentType,

                Status = OrderStatus.Awaiting,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(3), // 3�����ᥢ�� (����I���A�ȷ|�B�z)
                MerchantTradeNo = merchantTradeNo, // << �s�W�G�]�w�ө�����s�� >>
                Note = orderCreateDto.OrderNotes,
                OrderDetails = new List<OrderDetail>(),
                OrderParticipants = new List<OrderParticipant>()
            };
            decimal calculatedServerTotalAmount = 0;
            int participantDtoGlobalIndex = 0;
            if (orderCreateDto.CartItems == null || !orderCreateDto.CartItems.Any())
            {
                ModelState.AddModelError("CartItems", "�ʪ������S���ӫ~�C");
                return BadRequest(ModelState);
            }
            // --- �֤߭ק�G�ھګe�ݶǨӪ� CartItems �ͦ� OrderDetails ---
            foreach (var cartItemDto in orderCreateDto.CartItems)
            {
                decimal unitPrice = 0;
                string productName = "���~�W��";
                string optionSpecificDescription = cartItemDto.OptionType; // �ﶵ���y�z
                ProductCategory itemCategory; // �Ψ��x�s�ഫ�᪺ ProductCategory

                DateTime? itemSpecificStartDate = null;
                DateTime? itemSpecificEndDate = null;

                if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var parsedCategory))
                {
                    _logger.LogWarning($"�L�Ī� ProductType: {cartItemDto.ProductType}�C");
                    return BadRequest($"���䴩���ӫ~����: {cartItemDto.ProductType}�C");
                }
                itemCategory = parsedCategory;

                if (itemCategory == ProductCategory.GroupTravel)
                {
                    var groupTravel = await _context.GroupTravels
                                                .Include(gt => gt.OfficialTravelDetail)
                                                    .ThenInclude(otd => otd.OfficialTravel)
                                                .FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "�i���W");

                    if (groupTravel == null || groupTravel.OfficialTravelDetail == null || groupTravel.OfficialTravelDetail.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                    { 
                        return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) ��T������Υ���w����C"); 
                    }

                    productName = groupTravel.OfficialTravelDetail.OfficialTravel.Title;
                    var priceDetail = groupTravel.OfficialTravelDetail; // �����T�q�o�̨�

                    switch (cartItemDto.OptionType.ToUpper())
                    {
                        case "���H": case "ADULT": unitPrice = priceDetail.AdultPrice ?? 0; break;
                        case "�ൣ�[��": unitPrice = priceDetail.AdultPrice ?? 0; optionSpecificDescription = "�ൣ�[��"; break; // ���]�[�ɻ���P���H�ۦP
                        case "�ൣ����": unitPrice = priceDetail.ChildPrice ?? 0; optionSpecificDescription = "�ൣ����"; break;
                        case "�ൣ������": unitPrice = priceDetail.BabyPrice ?? 0; optionSpecificDescription = "�ൣ������"; break; // ���]�����ɦP�����
                        case "�ൣ": unitPrice = priceDetail.ChildPrice ?? 0; break;
                        case "����": case "BABY": unitPrice = priceDetail.BabyPrice ?? 0; break;
                        default:
                            _logger.LogWarning($"�����{ '{productName}' (GT{cartItemDto.ProductId}) ���ﶵ���� '{cartItemDto.OptionType}' �L�k�ѧO�Υ��w���C");
                            return BadRequest($"�ӫ~ '{productName}' ���ﶵ���� '{cartItemDto.OptionType}' �L�k�ѧO�Υ����ѻ���C");
                    }

                    if (unitPrice <= 0 && cartItemDto.Quantity > 0)
                    {
                        _logger.LogWarning($"�����{ '{productName}' �ﶵ '{cartItemDto.OptionType}' �����沧�` ({unitPrice})�C");
                        return BadRequest($"�ӫ~ '{productName}' �ﶵ '{cartItemDto.OptionType}' �������Ʋ��`�C");
                    }
                    itemSpecificStartDate = groupTravel.DepartureDate;
                    itemSpecificEndDate = groupTravel.ReturnDate;
                }
                else if (itemCategory == ProductCategory.CustomTravel)
                {
                    var customTravel = await _context.CustomTravels
                                             .FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Pending); // ���]�O Pending ���A
                    if (customTravel == null)
                    {
                        return BadRequest($"�Ȼs�Ʀ�{���~ (ID: CT{cartItemDto.ProductId}) �L�ĩΥ��o�G�C");
                    }

                    unitPrice = customTravel.TotalAmount; // CustomTravel ������O�� TotalAmount
                    productName = customTravel.Note ?? $"�Ȼs�Ʀ�{ {customTravel.CustomTravelId}"; // �ϥ� Note �@���W�١A�ιw�]�W��
                    //optionSpecificDescription = cartItemDto.OptionType; // �Ҧp "���]���" �Ϋe�ݶǨӪ��ﶵ�y�z

                    if (unitPrice <= 0 && cartItemDto.Quantity > 0)
                    {
                        _logger.LogWarning($"�����{ '{productName}' �ﶵ '{cartItemDto.OptionType}' �����沧�` ({unitPrice})�C");
                        return BadRequest($"�ӫ~ '{productName}' �ﶵ '{cartItemDto.OptionType}' �������Ʋ��`�C");
                    }
                }
                else
                {
                    return BadRequest($"�t�Τ������~�G�L�k�B�z���ӫ~���� {itemCategory}�C");
                }

                var orderDetail = new OrderDetail
                {
                    Order = order,
                    Category = itemCategory,                     
                    ItemId = cartItemDto.ProductId, // ItemId (GroupTravelId �� CustomTravelId)
                    Description = $"{productName} - {optionSpecificDescription}",
                    Quantity = cartItemDto.Quantity,
                    Price = unitPrice,
                    TotalAmount = cartItemDto.Quantity * unitPrice,
                    OrderParticipants = new List<OrderParticipant>(),
                    StartDate = itemSpecificStartDate,
                    EndDate = itemSpecificEndDate
                };
                order.OrderDetails.Add(orderDetail);
                calculatedServerTotalAmount += orderDetail.TotalAmount;
            }

            // �ϥΦ��A���p�⪺�`���B�A�åi��ܩʦa�P�e�ݶǨӪ��`���B�i����
            order.TotalAmount = calculatedServerTotalAmount;
            //if (Math.Abs(calculatedServerTotalAmount - orderCreateDto.TotalAmount) > 0.01m) // ���\0.01���~�t
            //{
            //    _logger.LogWarning("�q���`���B���@�P�C�|��ID {MemberId}�C���A���p��: {CalculatedTotal}, �e�ݴ���: {DtoTotal}.",
            //    member.MemberId, calculatedServerTotalAmount, orderCreateDto.TotalAmount);
            //}

            // 4. �B�z�ȫȦC�� (OrderParticipants)
            foreach (var detail in order.OrderDetails) // �M�����ھ� CartItems �ͦ��� OrderDetail ����
            {
                // �ھڳo�� OrderDetail �� Quantity�A���t�����ƶq���ȫ�
                for (int i = 0; i < detail.Quantity; i++)
                {
                    if (orderCreateDto.Participants != null && participantDtoGlobalIndex < orderCreateDto.Participants.Count)
                    {
                        var participantDto = orderCreateDto.Participants[participantDtoGlobalIndex++]; // �����Ǩ��@�Ӯȫ� DTO

                        // --- �A���Ѫ��{���X���q�q�o�̶}�l ---
                        if (participantDto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(participantDto.DocumentNumber))
                        {
                            ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].DocumentNumber", "����@�ӧ@���ҥ������ɡA�@�Ӹ��X������C");
                        }
                        if (string.IsNullOrEmpty(participantDto.IdNumber) && string.IsNullOrEmpty(participantDto.DocumentNumber))
                        {
                            ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].IdOrDocumentNumber", "�ȫȨ����Ҹ����ҥ󸹽X�ܤֻݶ�g�@���C");
                        }
                        if (!ModelState.IsValid)
                        {
                            _logger.LogWarning("CreateOrder Participant Custom Validation Failed: {@ModelStateErrors}", ModelState);
                            return BadRequest(ModelState);
                        }

                        var participant = new OrderParticipant
                        {
                            Order = order,           // <--- ���p��D�q��
                            OrderDetail = detail,    // <--- ���p���e�� OrderDetail (�Ӧۥ~�h�j�骺 detail)
                            Name = participantDto.Name,
                            BirthDate = participantDto.BirthDate,
                            IdNumber = participantDto.IdNumber,
                            Gender = participantDto.Gender, // �Ӧ� DTO�A���������P OrderParticipant.Gender (�T�|) �ݮe
                            //Phone = participantDto.Phone,
                            //Email = participantDto.Email,
                            DocumentType = participantDto.DocumentType, // �Ӧ� DTO�A���������P OrderParticipant.DocumentType (�T�|) �ݮe
                            DocumentNumber = participantDto.DocumentNumber,
                            PassportSurname = participantDto.PassportSurname,
                            PassportGivenName = participantDto.PassportGivenName,
                            PassportExpireDate = participantDto.PassportExpireDate,
                            Nationality = participantDto.Nationality,
                            Note = participantDto.Note,

                            FavoriteTravelerId = participantDto.FavoriteTravelerId
                        };
                        detail.OrderParticipants.Add(participant); // �N participant �[�J�����ݪ� OrderDetail �����X��
                        //order.OrderParticipants.Add(participant); // �Ϊ̲Τ@�[�J�� Order ���`�ȫȶ��X���AEF Core�|�B�z���Y


                        // �B�J 5: ���ձq�`�ήȫ�Ū�����
                        if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                        {
                            // ���ձq�`�ήȫ�Ū�����
                            var favoriteTraveler = await _context.MemberFavoriteTravelers
                                .AsNoTracking()
                                .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                           ft.MemberId == member.MemberId && // �T�O�O�ӷ|�����`�ήȫ�
                                                           ft.Status == FavoriteStatus.Active); // �T�O�`�ήȫȬO���Ī�

                            if (favoriteTraveler != null)
                            {
                                // �ϥα`�ήȫȸ�ƹw�� OrderParticipant
                                participant.Name = favoriteTraveler.Name;
                                participant.BirthDate = favoriteTraveler.BirthDate ?? default; // �p�G�`�ήȫȥͤ�i��null�A���ѹw�]��
                                participant.IdNumber = favoriteTraveler.IdNumber;
                                participant.Gender = favoriteTraveler.Gender ?? default; // �P�W
                                //participant.Phone = favoriteTraveler.Phone;
                                //participant.Email = favoriteTraveler.Email;
                                participant.DocumentType = favoriteTraveler.DocumentType ?? default; // �P�W
                                participant.DocumentNumber = favoriteTraveler.DocumentNumber;
                                participant.PassportSurname = favoriteTraveler.PassportSurname;
                                participant.PassportGivenName = favoriteTraveler.PassportGivenName;
                                participant.PassportExpireDate = favoriteTraveler.PassportExpireDate;
                                participant.Nationality = favoriteTraveler.Nationality;
                                participant.Note = string.IsNullOrEmpty(participantDto.Note) ? favoriteTraveler.Note : participantDto.Note;
                            }
                            else
                            {
                                _logger.LogWarning($"���ըϥα`�ήȫ�ID: {participantDto.FavoriteTravelerId.Value} ���䤣��θӮȫȤw���ΡC�N�ϥΫe�ݴ��Ѫ��ȫȸ�ơC");
                            }
                        }
                        if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                        {
                            var favoriteTraveler = await _context.MemberFavoriteTravelers
                                .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                          ft.MemberId == member.MemberId &&
                                                          ft.Status == FavoriteStatus.Active);
                            if (favoriteTraveler != null)
                            {
                                // ��s participant ���ݩ�
                                participant.Name = favoriteTraveler.Name;
                                participant.BirthDate = favoriteTraveler.BirthDate ?? default;
                                // ... ��s�Ҧ�������� ...
                                participant.Note = favoriteTraveler.Note; // �� participantDto.Note
                            }
                            else
                            {
                                return BadRequest($"�䤣��`�ήȫ� ID: {participantDto.FavoriteTravelerId.Value} �θӮȫȤw���ΡC");
                            }
                        }
                    }
                    else
                    {
                        // �ȫ� DTO �ƶq�����H�񺡩Ҧ� OrderDetail �� Quantity
                        _logger.LogWarning("���Ѫ��ȫȸ�Ƽƶq�����H�����Ҧ��q����Ӷ����ݨD�C");
                        ModelState.AddModelError("Participants", "�ȫȸ�Ƽƶq�P�q��ӫ~�һݼƶq���šC");
                        return BadRequest(ModelState); // �Ϊ̨�L�B�z�覡
                    }
                
                }
            }

            // �b�����x�s���Ʈw���e�A�A���ˬd ModelState�A�]�A��~�K�[���۩w�q���~
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateOrder �̲� ModelState �L��: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            // ��Ʈw�x�s (�ϥΥ��)
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // �Ĥ@���x�s�H��� OrderId

                    // �B�z�`�ήȫȪ���s�ηs�W
                    if (orderCreateDto.TravelerProfileActions != null && orderCreateDto.TravelerProfileActions.Any())
                    {
                        foreach (var profileAction in orderCreateDto.TravelerProfileActions)
                        {
                            if (profileAction.FavoriteTravelerIdToUpdate.HasValue && profileAction.FavoriteTravelerIdToUpdate.Value > 0)
                            {
                                // ��s�{�����`�ήȫ�
                                var existingFavTraveler = await _context.MemberFavoriteTravelers
                                    .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == profileAction.FavoriteTravelerIdToUpdate.Value &&
                                                                ft.MemberId == member.MemberId); // �w�����ˬd�G�T�O�O�ӷ|����

                                if (existingFavTraveler != null)
                                {
                                    _logger.LogInformation("�ǳƧ�s�`�ήȫȸ�ơA�`�ήȫ�ID: {FavoriteTravelerId}, �|��ID: {MemberId}",
                                        existingFavTraveler.FavoriteTravelerId, member.MemberId);

                                    // ��s��� (���] MemberFavoriteTraveler ���o�����)
                                    existingFavTraveler.Name = profileAction.Name;
                                    existingFavTraveler.BirthDate = profileAction.BirthDate;
                                    existingFavTraveler.IdNumber = profileAction.IdNumber;
                                    existingFavTraveler.Gender = profileAction.Gender;
                                    // existingFavTraveler.Phone = profileAction.Phone; // �p�G�`�ήȫȦ��q��
                                    // existingFavTraveler.Email = profileAction.Email; // �p�G�`�ήȫȦ�Email
                                    existingFavTraveler.DocumentType = profileAction.DocumentType;
                                    existingFavTraveler.DocumentNumber = profileAction.DocumentNumber;
                                    existingFavTraveler.PassportSurname = profileAction.PassportSurname;
                                    existingFavTraveler.PassportGivenName = profileAction.PassportGivenName;
                                    existingFavTraveler.PassportExpireDate = profileAction.PassportExpireDate;
                                    existingFavTraveler.Nationality = profileAction.Nationality;
                                    // existingFavTraveler.Note = profileAction.Note; // �p�G�`�ήȫȦ��Ƶ�
                                    existingFavTraveler.UpdatedAt = DateTime.Now;
                                    // Status �q�`�O�� Active�A���D���S�O�޿�

                                    _context.MemberFavoriteTravelers.Update(existingFavTraveler);
                                }
                                else
                                {
                                    _logger.LogWarning("���է�s���s�b�Τ��ݩ�ӷ|�����`�ήȫȸ�ơA�`�ήȫ�ID: {FavoriteTravelerIdToUpdate}, �|��ID: {MemberId}",
                                        profileAction.FavoriteTravelerIdToUpdate, member.MemberId);
                                    // �i��ܬO�_�n�]�����_����ζȰO��
                                }
                            }
                            else
                            {
                                // �s�W�`�ήȫ�
                                _logger.LogInformation("�ǳƷs�W�`�ήȫȸ�ơA�|��ID: {MemberId}, �ȫȩm�W: {TravelerName}",
                                    member.MemberId, profileAction.Name);

                                var newFavTraveler = new MemberFavoriteTraveler
                                {
                                    MemberId = member.MemberId,
                                    Name = profileAction.Name,
                                    BirthDate = profileAction.BirthDate,
                                    IdNumber = profileAction.IdNumber,
                                    Gender = profileAction.Gender,
                                    // Phone = profileAction.Phone,
                                    // Email = profileAction.Email,
                                    DocumentType = profileAction.DocumentType,
                                    DocumentNumber = profileAction.DocumentNumber,
                                    PassportSurname = profileAction.PassportSurname,
                                    PassportGivenName = profileAction.PassportGivenName,
                                    PassportExpireDate = profileAction.PassportExpireDate,
                                    Nationality = profileAction.Nationality,
                                    // Note = profileAction.Note,
                                    Status = FavoriteStatus.Active, // �]�w�w�]���A
                                    CreatedAt = DateTime.Now,    
                                    UpdatedAt = DateTime.Now
                                };
                                _context.MemberFavoriteTravelers.Add(newFavTraveler);
                            }
                        }
                        await _context.SaveChangesAsync(); // �x�s�`�ήȫȪ��ܧ�
                    }

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "�إ߭q��ɸ�Ʈw��s���ѡCInnerException: {InnerMessage}", ex.InnerException?.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, "�إ߭q�楢�ѡA�еy��A�աC");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "�إ߭q��ɵo�ͥ��w�����~�C");
                    return StatusCode(StatusCodes.Status500InternalServerError, "�إ߭q��ɵo�Ϳ��~�C");
                }
            }

            // �ǳƦ^�����e�ݪ����
            // �^�ǭq��ID�A�H�Τ�I�һݪ�������T (�o�������M��z�P��I�h�D����X�覡)
            var orderSummary = new OrderSummaryDto
            {
                OrderId = order.OrderId,
                MerchantTradeNo = order.MerchantTradeNo, // ���n�A�Ω����I��
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ExpiresAt = order.ExpiresAt // ���n�A�Ω�e����ܭ˼�
            };
            _logger.LogInformation("��B�q��إߦ��\�A�^�� orderSummary: {@OrderSummary}", orderSummary);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId, memberId = order.MemberId }, orderSummary);
        }

        // << �B�z�ĤG���q�I�ڻP�o����T�� Action >>
        /// <summary>
        /// �B�J2�G�̲׽T�{�q��I�ڤ覡�P�o����T�A�è��o ECPay �I�ڰѼ�
        /// </summary>
        /// <param name="orderId">�q�� ID</param>
        /// <param name="paymentDto">�I�ڤεo����T</param>
        /// <returns>ECPay �I�ڪ��һݰѼ�</returns>
        [HttpPut("{orderId}/finalize-payment")]
        public async Task<ActionResult<ECPayService.ECPayPaymentRequestViewModel>> FinalizePaymentAsync(int orderId, [FromBody] OrderPaymentFinalizationDto paymentDto)
        {
            _logger.LogInformation("������q��̲׽T�{�I�ڽШD�AOrderID: {OrderId}, DTO: {@PaymentDto}", orderId, paymentDto); // << �s�W�G��x�O�� >>

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("�q��̲׽T�{�I�ڼҫ����ҥ���: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            if (paymentDto.MemberId <= 0)
            {
                _logger.LogWarning("�q��̲׽T�{�I�ڡGDTO �����Ѫ� MemberId �L��: {ProvidedMemberId}", paymentDto.MemberId);
                return BadRequest(new { message = "�ШD���ʤ֦��Ī��|��ID�C" });
            }
            var memberIdFromDto = paymentDto.MemberId;
            var order = await _context.Orders
                                .Include(o => o.OrderDetails) // ECPayService �i��|�Ψ�
                                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == memberIdFromDto);

            if (order == null)
            {
                _logger.LogWarning("�q��̲׽T�{�I�ڡG�䤣��q�� ID {OrderId} �η|�� {MemberIdFromDto} �L�v���C", orderId, memberIdFromDto);
                return NotFound(new { message = $"�䤣��q�� {orderId} �αz�L�v�s���C" });
            }

            if (order.Status != OrderStatus.Awaiting)
            {
                _logger.LogWarning("�q�� {OrderId} ���A�� {Status}�A�L�k�i��I�ڳ]�w�C", orderId, order.Status);
                return BadRequest(new { message = $"�q��ثe���A�� '{order.Status}'�A�L�k�]�w�I�ڸ�T�C�нT�{�q�檬�A�έ��s�U��C" });
            }

            // ��s�q�檺�I�ڤ覡�M�o����T
            order.PaymentMethod = paymentDto.SelectedPaymentMethod;
            order.InvoiceOption = paymentDto.InvoiceRequestInfo.InvoiceOption;
            order.InvoiceDeliveryEmail = paymentDto.InvoiceRequestInfo.InvoiceDeliveryEmail;
            order.InvoiceUniformNumber = paymentDto.InvoiceRequestInfo.InvoiceUniformNumber;
            order.InvoiceTitle = paymentDto.InvoiceRequestInfo.InvoiceTitle;
            order.InvoiceAddBillingAddr = paymentDto.InvoiceRequestInfo.InvoiceAddBillingAddr;
            order.InvoiceBillingAddress = paymentDto.InvoiceRequestInfo.InvoiceBillingAddress;
            // �p�G OrderPaymentFinalizationDto.InvoiceRequestInfo ����L�o�����A�@�֧�s

            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("�q�� {OrderId} �w��s�I�ڤεo����T�C", order.OrderId);

                // << �s�W�G�ե� ECPayService �Ӳ��ͺ�ɥI�ڪ��Ѽ� >>
                // �`�N�GECPayService.GenerateEcPayPaymentForm �����|�A���ˬd�q�檬�A�M�Ĵ�
                var ecpayViewModel = await _ecpayService.GenerateEcPayPaymentForm(order.OrderId);

                _logger.LogInformation("���\���q�� {OrderId} ���� ECPay �I�ڰѼơA�ǳƦ^�ǫe�ݡC", order.OrderId);
                return Ok(ecpayViewModel);
            }
            catch (InvalidOperationException ex) // �i��� ECPayService �ߥX (�Ҧp�q�檬�A���ũΤw�L��)
            {
                _logger.LogWarning(ex, "�ǳ� ECPay �ѼƮɵo�ͷ~���޿���~ (InvalidOperationException)�C�q�� ID: {OrderId}", orderId);
                return BadRequest(new { message = ex.Message }); // �N ECPayService �����~�T���^�ǵ��e��
            }
            catch (ArgumentException ex) // �i��� ECPayService �ߥX (�Ҧp�䤣��q��)
            {
                _logger.LogWarning(ex, "�ǳ� ECPay �ѼƮɵo�ͰѼƿ��~ (ArgumentException)�C�q�� ID: {OrderId}", orderId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�̲׽T�{�I�ڨò��� ECPay �ѼƮɵo�ͥ��w�����~�C�q�� ID: {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "�B�z�I�ڽШD�ɵo�ͤ������~�A�еy��A�աC" });
            }
        }

        // << �s�W API ���I�G�Ұʭq�檺�u�ɮĴ� (30��S�ݴ�) >>
        // POST: api/Order/{orderId}/activate-short-expiration?memberId=123
        [HttpPost("{orderId}/activate-short-expiration")]
        public async Task<IActionResult> ActivateShortExpiration(int orderId, [FromQuery] int? memberId)
        {
            _logger.LogInformation("[ActivateShortExpiration] API endpoint hit for OrderId: {OrderId}, MemberId from query: {QueryMemberId}", orderId, memberId);

            if (memberId == null || memberId.Value <= 0)
            {
                _logger.LogWarning("[ActivateShortExpiration] Invalid memberId received: {QueryMemberId}", memberId);
                return BadRequest(new { message = "�ݭn���Ѧ��Ī� memberId�C" });
            }

            // << �ק�G�T�{�z�� Order �ҫ��D��O OrderId �٬O Id >>
            var order = await _context.Orders
                                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == memberId.Value);

            if (order == null)
            {
                _logger.LogWarning("[ActivateShortExpiration] Order not found. OrderId: {OrderId}, MemberId: {MemberId}", orderId, memberId.Value);
                return NotFound(new { message = $"�䤣��q�� (ID: {orderId}) �θӭq�椣�ݩ󦹷|���C" });
            }

            // �u�����A�� Awaiting ���q��~��Ұʵu�ɮĴ� (�S�ݴ�)
            // �åB�A�u����ثe�� ExpiresAt ���O�@�ӫD�`�u���ɶ� (�Ҧp�A�p�G�w�g�O�S�ݴ��A�h���A���Ƴ]�w)
            // ��²�ư_���A�u�n�O Awaiting �N���\���]�� 30 ��S�ݴ�
            if (order.Status == OrderStatus.Awaiting)
            {
                var newExpiresAt = DateTime.Now.AddMinutes(3);
                order.ExpiresAt = newExpiresAt; // ��s ExpiresAt ��3������

                // �i��G�O���ާ@��q��Ƶ�
                // string hesitationNote = $"�S�ݴ��Ұʩ� {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC�A�s������ɶ�: {newExpiresAt:yyyy-MM-dd HH:mm:ss} UTC�C";
                // order.Note = string.IsNullOrEmpty(order.Note) ? hesitationNote : $"{order.Note}\n{hesitationNote}";

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[ActivateShortExpiration] Order {OrderId} short expiration activated. New ExpiresAt: {NewExpiresAt} UTC.", order.OrderId, newExpiresAt);
                    // ��^�s������ɶ��A�e�ݥi�H��ܩʨϥ�
                    return Ok(new { message = "�q��S�ݴ��w�Ұ� (3����)�C", expiresAt = newExpiresAt });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ActivateShortExpiration] ��s�q�� {OrderId} ExpiresAt �ɵo�Ϳ��~�C", order.OrderId);
                    return StatusCode(500, "��s�q�����ɶ��ɵo�ͤ������~�C");
                }
            }
            else
            {
                _logger.LogWarning("[ActivateShortExpiration] Order {OrderId} ���A�� {OrderStatus}�A���A�Ω�ҰʵS�ݴ��C", order.OrderId, order.Status);
                return BadRequest(new { message = $"�q�檬�A ({order.Status}) �����\�ҰʵS�ݴ��C" });
            }
        }

        // *** �b�o�̲K�[�q�ܸ��X���W�ƪ��p�� Helper Method ***
        private string NormalizePhoneNumber(string phoneNumberString)
        {
            if (string.IsNullOrWhiteSpace(phoneNumberString)) // �ϥ� IsNullOrWhiteSpace ���
            {
                return phoneNumberString; // �p�G�O null �ΪŦr��A������^
            }

            string normalizedNumber = phoneNumberString.Trim(); // �����e��ť�

            // �ˬd�O�_�H "+" �}�Y�A�B���ר����i���ˬd (�ܤ֥]�t +��X�Ʀr0�Ʀr �� +��X�Ʀr�Ʀr)
            if (normalizedNumber.StartsWith("+"))
            {
                // �����}�Y�� '+'
                string numberWithoutPlus = normalizedNumber.Substring(1);

                // �S�O�B�z�x�W����X "886"
                if (numberWithoutPlus.StartsWith("886"))
                {
                    string countryCodePart = "886";
                    // ���o "886" �᭱�����X����
                    string nationalNumberPart = numberWithoutPlus.Substring(countryCodePart.Length); // �Ҧp "0905088127" �� "905088127"

                    // �p�G���X�����H "0" �}�Y�A�B���פj��1 (�Ҧp "09..." �ӫD�u�� "0")
                    if (nationalNumberPart.StartsWith("0") && nationalNumberPart.Length > 1)
                    {
                        nationalNumberPart = nationalNumberPart.Substring(1); // �����}�Y�� "0"�A�ܦ� "905088127"
                    }
                    // �զX��X�M�B�z�᪺���X����
                    _logger.LogInformation("Normalized phone from {Original} to {Processed}", phoneNumberString, countryCodePart + nationalNumberPart);
                    return countryCodePart + nationalNumberPart; // ���G�G"886905088127"
                }
                else
                {
                    // ĵ�i�G�o���] "+" �᭱��򪺬O���㪺�Ʀr�A�p�G�]�t��L�D�Ʀr�r���A�i��ݭn�B�~�B�z�C
                    if (numberWithoutPlus.All(char.IsDigit))
                    {
                        _logger.LogInformation("Normalized non-TW phone from {Original} to {Processed} (removed '+')", phoneNumberString, numberWithoutPlus);
                        return numberWithoutPlus; // �Ҧp�G��J "+14155552671"�A��^ "14155552671"
                    }
                    else
                    {
                        _logger.LogWarning("Phone number {PhoneNumber} starts with '+' but is not in the expected +886... format or contains non-digits after the initial '+'. Returning without '+'.", phoneNumberString);
                        // ���ղ��� "+" ��A�����Ҧ��D�Ʀr�r�� (�o�O�@�Ӥ���e�P���B�z)
                        // string cleanedNumber = new string(numberWithoutPlus.Where(char.IsDigit).ToArray());
                        // return cleanedNumber;
                        // �Ϊ̡A�p�G�榡���Źw���A�i�H�Ҽ{��^��l���X(���t+)�ΩߥX�榡���~
                        return numberWithoutPlus; // �ثe�Ȳ��� '+'
                    }
                }
            }
            _logger.LogInformation("Phone number {Original} did not match specific normalization rules, returning trimmed version.", phoneNumberString);
            return normalizedNumber;
        }

        // ���U��k�G�����e�n�J�ϥΪ̪� MemberId
        // ��ڹ�@�|�ھڱz���������ҳ]�w�Ӧ��Ҥ��P
        private int? GetCurrentUserId()
        {
            // ... (�z���Τ�ID����޿�) ...
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int userId)) { return userId; }
                _logger.LogWarning("GetCurrentUserId (parameterless): �w�{�ҡA���L�k�q Claims ���ѪR UserId�CClaimValue: '{ClaimValue}'", userIdClaim);
            }
            else { _logger.LogWarning("GetCurrentUserId (parameterless): User ���{�ҩ� User.Identity �� null�C"); }
            return null;
        }

        /// <summary>
        /// �d�߯S�w�q�檺�̷s���A�A�ѫe�ݮֹ�C
        /// </summary>
        /// <param name="orderId">�n�d�ߪ��q�� ID</param>
        /// <returns>PendingOrderStatusDto �� NotFound/Unauthorized</returns>
        [HttpGet("{orderId}/status-check")] // �Ҧp: GET /api/Order/123/status-check
        public async Task<ActionResult<PendingOrderStatusDto>> CheckOrderStatus(int orderId)
        {
            var memberId = GetCurrentUserId();
            if (memberId == null)
            {
                _logger.LogWarning("CheckOrderStatus for OrderId {OrderId}: User ���{�ҩεL�k��� MemberId�C", orderId);
                return Unauthorized(new { message = "�ϥΪ̥��n�J�εL�k�ѧO�����C" });
            }

            var orderStatusInfo = await _context.Orders
                .Where(o => o.OrderId == orderId && o.MemberId == memberId.Value) // �T�O�O�ӷ|�����q��
                .Select(o => new PendingOrderStatusDto
                {
                    OrderId = o.OrderId,
                    MerchantTradeNo = o.MerchantTradeNo,
                    Status = o.Status.ToString(),
                    ExpiresAt = o.ExpiresAt, // �Y�ϹL���]�^�ǡA���e�ݪ��D
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod.ToString()
                })
                .FirstOrDefaultAsync();

            if (orderStatusInfo == null)
            {
                _logger.LogWarning("CheckOrderStatus: MemberId {MemberId} �d�ߤ���q�� {OrderId} �εL�v���C", memberId.Value, orderId);
                return NotFound(new { message = $"�䤣��q�� {orderId} �αz�L�v�s���C" });
            }
            _logger.LogInformation("CheckOrderStatus: �d�ߨ�q�檬�A {@OrderStatusInfo} for OrderId {OrderId}, MemberId {MemberId}", orderStatusInfo, orderId, memberId.Value);
            return Ok(orderStatusInfo);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id, [FromQuery] int? memberId)
        {
            if (memberId == null || memberId.Value <= 0)
            {
                _logger.LogWarning("GetOrderById �ШD�������Ѧ��Ī� memberId�C�q��ID: {OrderId}", id);
                // ��^ BadRequest�A�i���e�ݻݭn���� memberId
                return BadRequest("�d�߭q��Ա��ݭn���Ѧ��Ī� memberId�C");
            }

            var orderData = await _context.Orders
                .Include(o => o.OrderParticipants) // �p�G OrderParticipant �]�ݭn
                .Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.GroupTravel) // �w�����J GroupTravel
                //        .ThenInclude(gt => gt.OfficialTravelDetail) // �p�G�ݭn��`�h��
                //            .ThenInclude(otd => otd.OfficialTravel) // �P�W
                //.Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.CustomTravel) // �w�����J CustomTravel
                .FirstOrDefaultAsync(o => o.OrderId == id && o.MemberId == memberId.Value);

            if (orderData == null)
            {
                return NotFound($"�䤣��q�� ID {id}�A�αz�L�v�s�����q��C");
            }

            // ��ʸ��J���p�� GroupTravel �M CustomTravel (�p�G�ݭn�B���b�W�� Include)
            // �o�ؤ覡�Ĳv�i��y�t�A�������[
            // --- �֤߭ק�G��ʸ��J OrderDetail ���� GroupTravel �� CustomTravel ---
            if (orderData.OrderDetails != null) // �T�O OrderDetails ���O null
            {
                foreach (var detail in orderData.OrderDetails)
                {
                    if (detail.Category == ProductCategory.GroupTravel)
                    {
                        // �ϥ� ItemId �q _context.GroupTravels �d��
                        // �T�O�AGroupTravel ���馳�P detail.ItemId �������D��A�Ҧp GroupTravelId
                        detail.GroupTravel = await _context.GroupTravels
                            .Include(gt => gt.OfficialTravelDetail)
                                .ThenInclude(otd => otd.OfficialTravel)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(gt => gt.GroupTravelId == detail.ItemId);
                    }
                    else if (detail.Category == ProductCategory.CustomTravel)
                    {
                        detail.CustomTravel = await _context.CustomTravels
                            .AsNoTracking()
                            .FirstOrDefaultAsync(ct => ct.CustomTravelId == detail.ItemId);
                    }
                }
            }
            var orderDto = new // �A���ӥΧA�w�q�� OrderDto
            {
                OrderId = orderData.OrderId,
                MemberId = orderData.MemberId,
                TotalAmount = orderData.TotalAmount,
                PaymentMethod = orderData.PaymentMethod.ToString(),
                OrderStatus = orderData.Status.ToString(),
                CreatedAt = orderData.CreatedAt,
                PaymentDate = orderData.PaymentDate,
                Note = orderData.Note,
                InvoiceOption = orderData.InvoiceOption.ToString(), // �ഫ���r��
                InvoiceDeliveryEmail = orderData.InvoiceDeliveryEmail,
                InvoiceUniformNumber = orderData.InvoiceUniformNumber,
                InvoiceTitle = orderData.InvoiceTitle,
                InvoiceAddBillingAddr = orderData.InvoiceAddBillingAddr,
                InvoiceBillingAddress = orderData.InvoiceBillingAddress,
                ExpiresAt = orderData.ExpiresAt, // �^�ǭq��L���ɶ� 
                MerchantTradeNo = orderData.MerchantTradeNo,// �^�ǰө�����s�� 

                OrdererInfo = new OrdererInfoDto
                {
                    Name = orderData.OrdererName,
                    MobilePhone = orderData.OrdererPhone,
                    Email = orderData.OrdererEmail,
                    Nationality = orderData.OrdererNationality,
                    DocumentType = orderData.OrdererDocumentType.ToString(),
                    DocumentNumber = orderData.OrdererDocumentNumber
                },

                Participants = orderData.OrderParticipants?.Select(p => new OrderParticipantDto
                {
                    OrderDetailId = p.OrderDetailId,
                    Name = p.Name,
                    BirthDate = p.BirthDate,
                    IdNumber = p.IdNumber,
                    Gender = p.Gender, // �ഫ���r��
                    DocumentType = p.DocumentType, // �ഫ���r��
                    DocumentNumber = p.DocumentNumber,
                    PassportSurname = p.PassportSurname,
                    PassportGivenName = p.PassportGivenName,
                    PassportExpireDate = p.PassportExpireDate,
                    Nationality = p.Nationality,
                    Note = p.Note,
                    FavoriteTravelerId = p.FavoriteTravelerId
                }).ToList() ?? new List<OrderParticipantDto>(),

                OrderDetails = orderData.OrderDetails?.Select(od =>
                {
                    string productTitle = string.Empty;
                    //DateTime? itemStartDate = null;
                    //DateTime? itemEndDate = null;
                    if (od.Category == ProductCategory.GroupTravel && od.GroupTravel != null)
                    {
                        productTitle = od.GroupTravel.OfficialTravelDetail?.OfficialTravel?.Title ?? string.Empty;
                        // ���] OfficialTravelDetail �� GroupTravel �� StartDate/EndDate
                        //itemStartDate = od.GroupTravel.DepartureDate;
                        //itemEndDate = od.GroupTravel.ReturnDate;
                    }
                    else if (od.Category == ProductCategory.CustomTravel && od.CustomTravel != null)
                    {
                        productTitle = od.CustomTravel.Note ?? $"�Ȼs�Ʀ�{ {od.CustomTravel.CustomTravelId}";
                        // ���] CustomTravel �� StartDate/EndDate
                        //itemStartDate = od.CustomTravel.DepartureDate;
                        //itemEndDate = od.CustomTravel.EndDate;
                    }

                    if (string.IsNullOrEmpty(productTitle) && !string.IsNullOrEmpty(od.Description))
                    {
                        int separatorIndex = od.Description.LastIndexOf(" - ");
                        productTitle = separatorIndex > 0 ? od.Description.Substring(0, separatorIndex) : od.Description;
                    }


                    return new OrderDetailItemDto
                    {
                        OrderDetailId = od.OrderDetailId,
                        Category = od.Category,
                        ItemId = od.ItemId,
                        Description = od.Description,
                        Quantity = od.Quantity,
                        Price = od.Price,
                        TotalAmount = od.TotalAmount,
                        ProductType = od.Category.ToString(),
                        OptionType = ParseOptionTypeFromDescription(od.Description, productTitle),
                        StartDate = od.StartDate,
                        EndDate = od.EndDate,   
                    };
                }).ToList() ?? new List<OrderDetailItemDto>(),
            };

            return Ok(orderDto);
        }
        // ���U��k�G�q OrderDetail.Description ���ѪR OptionType (�o�������tricky�A�ݭn�A�ھ� Description ���榡�ӹ�@)
        private string ParseOptionTypeFromDescription(string description, string productName)
        {
            if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(productName)) return description; // �Ϊ�^�Ŧr��
            // ���] Description �榡�� "{productName} - {optionSpecificDescription}"
            string prefix = $"{productName} - ";
            if (description.StartsWith(prefix))
            {
                return description.Substring(prefix.Length);
            }
            return description; // �p�G�榡���šA��^��l�y�z�ίS�w�w�]��
        }

        [HttpGet("{orderId}/invoice")] // �Ҧp: GET /api/Order/149/invoice
        public async Task<ActionResult<InvoiceDetailsDto>> GetInvoiceDetails(int orderId, [FromQuery] int? memberId)
        {
            // 1. �w�����ˬd (���� GetOrderById�A�T�{ memberId �P�q��ǰt)
            if (memberId == null || memberId.Value <= 0)
            {
                return BadRequest("�d�ߵo���Ա��ݭn���Ѧ��Ī� memberId�C");
            }

            var order = await _context.Orders
                                      .Include(o => o.OrderInvoices) // �T�O���J�w�s�b���o���O��
                                      .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == memberId.Value);

            if (order == null)
            {
                return NotFound($"�䤣��q�� ID {orderId}�A�αz�L�v�s�����q��C");
            }

            // 2. �d��w�}�ߪ��o���O��
            var openedInvoice = order.OrderInvoices
                                         .FirstOrDefault(inv => inv.InvoiceStatus == InvoiceStatus.Opened); // ���] Opened ��ܦ��\

            if (openedInvoice != null)
            {
                return Ok(new InvoiceDetailsDto
                {
                    RtnCode = 1,
                    RtnMsg = "�o���w���\�}�ߡC",
                    InvoiceNo = openedInvoice.InvoiceNumber,
                    InvoiceDate = openedInvoice.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss"), // �ϥΤ@�P������榡
                    RandomNumber = openedInvoice.RandomCode,

                    InvoiceType = openedInvoice.InvoiceType.ToString(), // �N enum �ର�r��
                    BuyerName = openedInvoice.BuyerName,
                    BuyerUniformNumber = openedInvoice.BuyerUniformNumber,
                    TotalAmount = openedInvoice.TotalAmount,
                    InvoiceItemDesc = openedInvoice.InvoiceItemDesc,
                    Note = openedInvoice.Note
                    // InvoiceStatus = openedInvoice.InvoiceStatus.ToString() // �p�G�]�Q�^�Ǫ��A
                });
            }

            // 3. �p�G��Ʈw���S���w�}�ߪ��o���O���A�B�q��w�I�ڡA�i�H�Ҽ{�O�_�n���էY�ɶ}�ߩδ���
            //    �o�̪��޿���M��z���~�Ȭy�{�G
            //    a) �I�ڦ��\�� (ECPay callback) �N���ӤwĲ�o IssueInvoiceAsync�C
            //    b) ���B�Ȭ��d�ߡA�Y���}�߫h���ܡC
            //    c) ���B���ոɶ} (�������A�ݪ`�N���ƶ}�����D)�C

            // ���]�y�{ (a) �w�����A�o�̥D�n���d��
            // �p�G�b ECPay callback ���}�����ѡAOrderInvoice ���i��|�����ѰO��
            var failedInvoiceAttempt = order.OrderInvoices.OrderByDescending(i => i.CreatedAt).FirstOrDefault();
            if (failedInvoiceAttempt != null && failedInvoiceAttempt.InvoiceStatus != InvoiceStatus.Opened)
            {
                _logger.LogWarning("�q�� {OrderId} ���e���ն}�ߵo���������\: {Note}", orderId, failedInvoiceAttempt.Note);
                return Ok(new InvoiceDetailsDto
                {
                    RtnCode = 0, // �۩w�q���ѽX
                    RtnMsg = $"�o�����e���ն}�ߦ������\: {failedInvoiceAttempt.Note ?? "�Ь��ȪA"}",
                });
            }


            _logger.LogInformation("�q�� {OrderId} �|�����w�}�ߪ��o���O���C", orderId);
            return Ok(new InvoiceDetailsDto
            {
                RtnCode = 2, // �۩w�q�X�A��ܩ|���}�ߩάd�ߤ���
                RtnMsg = "���q��ثe�|�L�w�}�ߪ��o����T�C"
            });
        }

        [HttpPut("{orderId}")] // �Ϊ� [HttpPost("{orderId}/update")]
        public async Task<ActionResult<OrderSummaryDto>> UpdateOrderAsync(int orderId, [FromBody] OrderUpdateDto orderUpdateDto)
        {
            _logger.LogInformation("������q���s�ШD�AOrderID: {OrderId}, DTO: {@OrderUpdateDto}", orderId, orderUpdateDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("�q���s�ҫ����ҥ���: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            //var currentMemberId = GetCurrentUserId(); // �����e�n�J�Τ᪺ MemberId
            //if (currentMemberId == null)
            //{
            //    _logger.LogWarning("�q���s�G�Τ᥼�{�ҩεL�k��� MemberId�C");
            //    return Unauthorized(new { message = "�Τ᥼�{�ҡC" });
            //}

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.OrderParticipants) // ���J OrderDetail �U�� Participants
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);
                        //.FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == currentMemberId.Value);

                    if (order == null)
                    {
                        _logger.LogWarning("�q���s�G�䤣��q�� ID {OrderId} �η|�� {CurrentMemberId} �L�v�ק惡�q��C", orderId);
                        //_logger.LogWarning("�q���s�G�䤣��q�� ID {OrderId} �η|�� {CurrentMemberId} �L�v�ק惡�q��C", orderId, currentMemberId.Value);
                        return NotFound(new { message = $"�䤣��q�� {orderId} �αz�L�v�ק惡�q��C" });
                    }

                    if (order.Status != OrderStatus.Awaiting) // ���]�u�� Awaiting ���A���q��i�H�ק�
                    {
                        _logger.LogWarning("�q���s�G�q�� {OrderId} ���A�� {Status}�A�L�k�ק�C", orderId, order.Status);
                        return BadRequest(new { message = $"�q��ثe���A�� '{order.Status}'�A�L�k�ק�C" });
                    }

                    // 1. ��s Order ���򥻸��
                    order.OrdererName = orderUpdateDto.OrdererInfo.Name;
                    order.OrdererPhone = NormalizePhoneNumber(orderUpdateDto.OrdererInfo.MobilePhone); // ���]�A�� NormalizePhoneNumber
                    order.OrdererEmail = orderUpdateDto.OrdererInfo.Email;
                    order.OrdererNationality = orderUpdateDto.OrdererInfo.Nationality;
                    order.OrdererDocumentNumber = orderUpdateDto.OrdererInfo.DocumentNumber;
                    order.OrdererDocumentType = orderUpdateDto.OrdererInfo.DocumentType; // ���] DTO �� DocumentType �O string�A�B OrdererDocumentType �O�T�|
                    order.Note = orderUpdateDto.OrderNotes;
                    order.UpdatedAt = DateTime.Now;
                    // �q�` MerchantTradeNo �b�q��إ߫ᤣ�����
                    order.ExpiresAt = DateTime.Now.AddMinutes(3); // �ھڧA���~���޿�վ�

                    // 2. �R���ª� OrderParticipants �M OrderDetails
                    if (order.OrderDetails.Any())
                    {
                        // ���R���̿� OrderDetail �� OrderParticipant
                        var participantsToRemove = order.OrderDetails.SelectMany(od => od.OrderParticipants).ToList();
                        if (participantsToRemove.Any())
                        {
                            _context.OrderParticipants.RemoveRange(participantsToRemove);
                        }
                        _context.OrderDetails.RemoveRange(order.OrderDetails);
                        // �M�ŰO���餤�����X�O�n�ߺD�A��EF Core�l�ܹ��骬�A�ARemoveRange�q�`����
                        // order.OrderDetails.Clear(); 
                        // order.OrderParticipants.Clear(); // �]�M�Ū������border�U�� (�p�G���e���o�˥[)
                    }
                    // **���n**: �p�G�A�� SaveChanges() ����R���A����K�[��ID���|�Ĭ�C
                    // �����F�b�P�@�ӥ�����A�ڭ̹����� EF Core �B�z�C
                    // �p�G�J����D�A�i�H�Ҽ{�b�o�� SaveChanges �@���C

                    // 2c. �ھ� orderUpdateDto.CartItems ���s�Ы� OrderDetails �M�����p�� OrderParticipants
                    decimal calculatedServerTotalAmount = 0;
                    int participantDtoGlobalIndex = 0;

                    if (orderUpdateDto.CartItems == null || !orderUpdateDto.CartItems.Any())
                    {
                        ModelState.AddModelError("CartItems", "��s�q��ɡA�ʪ������S���ӫ~�C");
                        return BadRequest(ModelState);
                    }

                    var newOrderDetails = new List<OrderDetail>(); // �Ȧs�s�� OrderDetail

                    foreach (var cartItemDto in orderUpdateDto.CartItems)
                    {
                        // --- �o�����޿�P InitiateOrderAsync �����Ы� OrderDetail �M Participant �޿�X�G�@�P ---
                        decimal unitPrice = 0;
                        string productName = ""; // �w�]�αq���~�d��
                        string optionSpecificDescription = cartItemDto.OptionType;
                        ProductCategory itemCategory;

                        // �s�W�G�Ω��x�s�q���~��������
                        DateTime? itemSpecificStartDate = null;
                        DateTime? itemSpecificEndDate = null;

                        if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var parsedCategory))
                        {
                            _logger.LogWarning($"�q���s {orderId}�G�L�Ī� ProductType: {cartItemDto.ProductType}�C");
                            return BadRequest($"���䴩���ӫ~����: {cartItemDto.ProductType}�C");
                        }
                        itemCategory = parsedCategory;

                        // (�ƻs InitiateOrderAsync ���ھ� itemCategory ��� productName �M unitPrice ���޿�)
                        if (itemCategory == ProductCategory.GroupTravel)
                        {
                            var groupTravel = await _context.GroupTravels
                                .Include(gt => gt.OfficialTravelDetail).ThenInclude(otd => otd.OfficialTravel)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "�i���W");

                            if (groupTravel == null || groupTravel.OfficialTravelDetail?.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                            { return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) ��T������Υ���w����C"); }
                            productName = groupTravel.OfficialTravelDetail.OfficialTravel.Title;
                            var priceDetail = groupTravel.OfficialTravelDetail;
                            switch (cartItemDto.OptionType.ToUpper()) // ���] OptionType �O "���H", "�ൣ" ��
                            {
                                case "���H": case "ADULT": unitPrice = priceDetail.AdultPrice ?? 0; break;
                                case "�ൣ�[��": unitPrice = priceDetail.AdultPrice ?? 0; optionSpecificDescription = "�ൣ�[��"; break; // ���]�[�ɻ���P���H�ۦP
                                case "�ൣ����": unitPrice = priceDetail.ChildPrice ?? 0; optionSpecificDescription = "�ൣ����"; break;
                                case "�ൣ������": unitPrice = priceDetail.BabyPrice ?? 0; optionSpecificDescription = "�ൣ������"; break; // ���]�����ɦP�����
                                case "�ൣ": case "CHILD": unitPrice = priceDetail.ChildPrice ?? 0; break; // �վ㬰 CHILD
                                case "����": case "BABY": unitPrice = priceDetail.BabyPrice ?? 0; break;
                                default: return BadRequest($"�ӫ~ '{productName}' ���ﶵ���� '{cartItemDto.OptionType}' �L�k�ѧO�C");
                            }
                            if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"�ӫ~ '{productName}' �ﶵ '{cartItemDto.OptionType}' �������Ʋ��`�C");
                            
                            // �q GroupTravel ������
                            itemSpecificStartDate = groupTravel.DepartureDate;
                            itemSpecificEndDate = groupTravel.ReturnDate;
                        }
                        else if (itemCategory == ProductCategory.CustomTravel)
                        {
                            var customTravel = await _context.CustomTravels.FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Pending);
                            if (customTravel == null) { return BadRequest($"�Ȼs�Ʀ�{���~ (ID: CT{cartItemDto.ProductId}) �L�ġC"); }
                            unitPrice = customTravel.TotalAmount;
                            productName = customTravel.Note ?? $"�Ȼs�Ʀ�{ {customTravel.CustomTravelId}";
                            if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"�ӫ~ '{productName}' �����Ʋ��`�C");
                        }
                        else { return BadRequest($"�������ӫ~���� {itemCategory}�C"); }


                        var orderDetail = new OrderDetail
                        {
                            Order = order, // �۰ʳ]�w OrderId
                            Category = itemCategory,
                            ItemId = cartItemDto.ProductId,
                            Description = $"{productName} - {optionSpecificDescription}",
                            Quantity = cartItemDto.Quantity,
                            Price = unitPrice,
                            TotalAmount = cartItemDto.Quantity * unitPrice,
                            OrderParticipants = new List<OrderParticipant>(), // ��l��
                            StartDate = itemSpecificStartDate,
                            EndDate = itemSpecificEndDate
                        };
                        newOrderDetails.Add(orderDetail);
                        calculatedServerTotalAmount += orderDetail.TotalAmount;

                        for (int i = 0; i < orderDetail.Quantity; i++)
                        {
                            if (orderUpdateDto.Participants != null && participantDtoGlobalIndex < orderUpdateDto.Participants.Count)
                            {
                                var participantDto = orderUpdateDto.Participants[participantDtoGlobalIndex++];
                                if (participantDto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(participantDto.DocumentNumber))
                                { 
                                    ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].DocumentNumber", "����@�ӧ@���ҥ������ɡA�@�Ӹ��X������C"); 
                                }
                                if (string.IsNullOrEmpty(participantDto.IdNumber) && string.IsNullOrEmpty(participantDto.DocumentNumber))
                                {
                                    ModelState.AddModelError($"Participants[{participantDtoGlobalIndex - 1}].IdOrDocumentNumber", "�ȫȨ����Ҹ����ҥ󸹽X�ܤֻݶ�g�@���C"); 
                                }
                                if (!ModelState.IsValid) 
                                {
                                    return BadRequest(ModelState); 
                                }
                                var participant = new OrderParticipant
                                {
                                    Order = order,
                                    OrderDetail = orderDetail, // **���p��s�� OrderDetail**
                                    Name = participantDto.Name,
                                    BirthDate = participantDto.BirthDate,
                                    IdNumber = participantDto.IdNumber,
                                    Gender = participantDto.Gender, // DTO�����T�|�Υi�ഫ���r��/�Ʀr
                                    Phone = participantDto.Phone,
                                    Email = participantDto.Email,
                                    DocumentType = participantDto.DocumentType, // DTO�����T�|�Υi�ഫ���r��/�Ʀr
                                    DocumentNumber = participantDto.DocumentNumber,
                                    PassportSurname = participantDto.PassportSurname,
                                    PassportGivenName = participantDto.PassportGivenName,
                                    PassportExpireDate = participantDto.PassportExpireDate,
                                    Nationality = participantDto.Nationality,
                                    Note = participantDto.Note,
                                    FavoriteTravelerId = participantDto.FavoriteTravelerId
                                };
                                if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                                {
                                    // ���ձq�`�ήȫ�Ū�����
                                    var favoriteTraveler = await _context.MemberFavoriteTravelers
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                                   ft.MemberId == order.MemberId &&
                                                                   ft.Status == FavoriteStatus.Active); // �T�O�`�ήȫȬO���Ī�

                                    if (favoriteTraveler != null)
                                    {
                                        // �ϥα`�ήȫȸ�ƹw�� OrderParticipant
                                        participant.Name = favoriteTraveler.Name;
                                        participant.BirthDate = favoriteTraveler.BirthDate ?? default; // �p�G�`�ήȫȥͤ�i��null�A���ѹw�]��
                                        participant.IdNumber = favoriteTraveler.IdNumber;
                                        participant.Gender = favoriteTraveler.Gender ?? default; // �P�W
                                                                                                 //participant.Phone = favoriteTraveler.Phone;
                                                                                                 //participant.Email = favoriteTraveler.Email;
                                        participant.DocumentType = favoriteTraveler.DocumentType ?? default; // �P�W
                                        participant.DocumentNumber = favoriteTraveler.DocumentNumber;
                                        participant.PassportSurname = favoriteTraveler.PassportSurname;
                                        participant.PassportGivenName = favoriteTraveler.PassportGivenName;
                                        participant.PassportExpireDate = favoriteTraveler.PassportExpireDate;
                                        participant.Nationality = favoriteTraveler.Nationality;
                                        participant.Note = string.IsNullOrEmpty(participantDto.Note) ? favoriteTraveler.Note : participantDto.Note;
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"��s�q��ɡA�`�ήȫ�ID: {participantDto.FavoriteTravelerId.Value} �䤣��B���ݩ�|�� {order.MemberId} �Τw���ΡC");
                                    }
                                }
                                orderDetail.OrderParticipants.Add(participant);
                            }
                            else
                            {
                                _logger.LogWarning("��s�q�� OrderID {OrderId} �ɡA�ӫ~ {ProductName} ���Ѫ��ȫȸ�Ƽƶq�����C", orderId, productName);
                                ModelState.AddModelError("Participants", $"�ӫ~ '{productName} - {optionSpecificDescription}' ���ȫȸ�Ƥ����C");
                                return BadRequest(ModelState);
                            }
                        }
                    }
                    order.OrderDetails = newOrderDetails;
                    order.TotalAmount = calculatedServerTotalAmount;
                    if (Math.Abs(calculatedServerTotalAmount - orderUpdateDto.TotalAmount) > 0.01m)
                    {
                        _logger.LogWarning("�q�� {OrderId} ��s���`���B���@�P�C���A��: {Calculated}, �e��: {Provided}", orderId, calculatedServerTotalAmount, orderUpdateDto.TotalAmount);
                        // �ھڷ~���޿�M�w�O�_�n�]����^���~
                    }

                    // 3. �B�z TravelerProfileActions (�p�G�ݭn�A�޿�P InitiateOrderAsync ����)
                    if (orderUpdateDto.TravelerProfileActions != null && orderUpdateDto.TravelerProfileActions.Any())
                    {
                        // ... (�A����s/�s�W�`�ήȫ��޿�) ...
                    }

                    if (!ModelState.IsValid)
                    {
                        _logger.LogWarning("UpdateOrder �̲� ModelState �L��: {@ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                        return BadRequest(ModelState);
                    }

                    await _context.SaveChangesAsync(); // �@�����x�s�Ҧ��ܧ�
                    await transaction.CommitAsync();

                    var orderSummary = new OrderSummaryDto
                    {
                        OrderId = order.OrderId,
                        MerchantTradeNo = order.MerchantTradeNo,
                        OrderStatus = order.Status.ToString(),
                        TotalAmount = order.TotalAmount,
                        ExpiresAt = order.ExpiresAt
                    };
                    _logger.LogInformation("�q�� {OrderId} ��s���\�A�æ^�ǺK�n�C", order.OrderId);
                    return Ok(orderSummary);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "��s�q�� {OrderId} �ɵo�Ϳ��~�C", orderId);
                    return StatusCode(StatusCodes.Status500InternalServerError, "��s�q�楢�ѡA�еy��A�աC");
                }
            }
        }
    }

}