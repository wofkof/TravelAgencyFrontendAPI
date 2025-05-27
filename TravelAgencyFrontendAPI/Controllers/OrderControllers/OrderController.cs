
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
        public async Task<ActionResult<OrderSummaryDto>> InitiateOrderAsync([FromBody] OrderCreateDto orderCreateDto) // << �ק�G��k�W�M DTO >>
        {
            _logger.LogInformation("�������B�q��إ߽ШD: {@OrderCreateDto}", orderCreateDto); // << �s�W�G��x�O�� >>

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("��B�q��ҫ����ҥ���: {@ModelState}", ModelState); // << �s�W�G��x�O�� >>
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
            string timePartForMtn = DateTime.UtcNow.ToString("yyMMddHHmmss"); // ��T���A��֭��ƾ��v
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

                Status = OrderStatus.Awaiting,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(30), // 30��ᥢ�� (����I���A�ȷ|�B�z)
                MerchantTradeNo = merchantTradeNo, // << �s�W�G�]�w�ө�����s�� >>
                Note = orderCreateDto.OrderNotes,
                OrderDetails = new List<OrderDetail>(),
                OrderParticipants = new List<OrderParticipant>()
            };

            decimal calculatedServerTotalAmount = 0;
            if (orderCreateDto.CartItems == null || !orderCreateDto.CartItems.Any())
            {
                ModelState.AddModelError("CartItems", "�ʪ������S���ӫ~�C");
                return BadRequest(ModelState);
            }
            // --- �֤߭ק�G�ھګe�ݶǨӪ� CartItems �ͦ� OrderDetails ---
            foreach (var cartItemDto in orderCreateDto.CartItems)
            {
                decimal unitPrice = 0;
                string productName = "";
                string optionSpecificDescription = cartItemDto.OptionType; // �ﶵ���y�z
                ProductCategory itemCategory; // �Ψ��x�s�ഫ�᪺ ProductCategory

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
                                                .FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "�}��");

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
                    optionSpecificDescription = cartItemDto.OptionType; // �Ҧp "���]���" �Ϋe�ݶǨӪ��ﶵ�y�z

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
                    Category = itemCategory,                     
                    ItemId = cartItemDto.ProductId,              // ItemId (GroupTravelId �� CustomTravelId)
                    Description = $"{productName} - {optionSpecificDescription}",
                    Quantity = cartItemDto.Quantity,
                    Price = unitPrice,
                    TotalAmount = cartItemDto.Quantity * unitPrice,
                };
                order.OrderDetails.Add(orderDetail);
                calculatedServerTotalAmount += orderDetail.TotalAmount;
            }

            // �ϥΦ��A���p�⪺�`���B�A�åi��ܩʦa�P�e�ݶǨӪ��`���B�i����
            order.TotalAmount = calculatedServerTotalAmount;
            if (Math.Abs(calculatedServerTotalAmount - orderCreateDto.TotalAmount) > 0.01m) // ���\0.01���~�t
            {
                _logger.LogWarning("�q���`���B���@�P�C�|��ID {MemberId}�C���A���p��: {CalculatedTotal}, �e�ݴ���: {DtoTotal}.",
                member.MemberId, calculatedServerTotalAmount, orderCreateDto.TotalAmount);
            }

            // 4. �B�z�ȫȦC�� (OrderParticipants)
            if (orderCreateDto.Participants != null)
            {
                foreach (var participantDto in orderCreateDto.Participants)
                {
                    if (participantDto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(participantDto.DocumentNumber))
                    {
                        // �K�[�@�� Model ���~�Ϊ�����^ BadRequest
                        ModelState.AddModelError($"Participants[{order.OrderParticipants.Count}].DocumentNumber", "����@�ӧ@���ҥ������ɡA�@�Ӹ��X������C");
                    }

                    // �ˬd�������O�_���� (���M DTO �w�� [Required]�A���o�̥i�H�A���T�{�ΰO��)
                    if (string.IsNullOrEmpty(participantDto.Name)) { /* ... ���~�B�z ... */ }
                    // ... ��L��������ˬd ...

                    // �Ϊ̨�L�զX�W�h�A�Ҧp�GIdNumber �� DocumentNumber �ܤ֭n���@�ӫD�šH
                    if (string.IsNullOrEmpty(participantDto.IdNumber) && string.IsNullOrEmpty(participantDto.DocumentNumber))
                    {
                        ModelState.AddModelError($"Participants[{order.OrderParticipants.Count}].IdOrDocumentNumber", "�ȫȨ����Ҹ����ҥ󸹽X�ܤֻݶ�g�@���C");
                    }

                    // �p�G���۩w�q���ҿ��~�A��ĳ�b�o���ˬd�ô��e��^
                    if (!ModelState.IsValid)
                    {
                        // �O�����~�ê�^
                        _logger.LogWarning("CreateOrder Participant Custom Validation Failed: {@ModelStateErrors}", ModelState);
                        return BadRequest(ModelState);
                    }

                    var participant = new OrderParticipant
                    {
                        Name = participantDto.Name,
                        BirthDate = participantDto.BirthDate,
                        IdNumber = participantDto.IdNumber,
                        Gender = participantDto.Gender,

                        Phone = participantDto.Phone,
                        Email = participantDto.Email,

                        DocumentType = participantDto.DocumentType,
                        DocumentNumber = participantDto.DocumentNumber,
                        PassportSurname = participantDto.PassportSurname,
                        PassportGivenName = participantDto.PassportGivenName,
                        PassportExpireDate = participantDto.PassportExpireDate,
                        Nationality = participantDto.Nationality,
                        Note = participantDto.Note
                    };
                    order.OrderParticipants.Add(participant);
                    // �B�J 5: ���ձq�`�ήȫ�Ū�����
                    if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                    {
                        // ���ձq�`�ήȫ�Ū�����
                        var favoriteTraveler = await _context.MemberFavoriteTravelers
                            .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                       ft.MemberId == member.MemberId && // �T�O�O�ӷ|�����`�ήȫ�
                                                       ft.Status == FavoriteStatus.Active); // �T�O�`�ήȫȬO���Ī�

                        if (favoriteTraveler == null)
                        {
                            return BadRequest($"�䤣��`�ήȫ� ID: {participantDto.FavoriteTravelerId.Value} �θӮȫȤw���ΡC");
                        }

                        // �ϥα`�ήȫȸ�ƹw�� OrderParticipant
                        participant.Name = favoriteTraveler.Name;
                        participant.BirthDate = favoriteTraveler.BirthDate ?? default; // �p�G�`�ήȫȥͤ�i��null�A���ѹw�]��
                        participant.IdNumber = favoriteTraveler.IdNumber;
                        participant.Gender = favoriteTraveler.Gender ?? default; // �P�W
                        participant.Phone = favoriteTraveler.Phone;
                        participant.Email = favoriteTraveler.Email;
                        participant.DocumentType = favoriteTraveler.DocumentType ?? default; // �P�W
                        participant.DocumentNumber = favoriteTraveler.DocumentNumber;
                        participant.PassportSurname = favoriteTraveler.PassportSurname;
                        participant.PassportGivenName = favoriteTraveler.PassportGivenName;
                        participant.PassportExpireDate = favoriteTraveler.PassportExpireDate;
                        participant.Nationality = favoriteTraveler.Nationality;
                        participant.Note = favoriteTraveler.Note; // �i�H�Ҽ{�O�_�n�X�� DTO �� Note

                    }
                    order.OrderParticipants.Add(participant);
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
                                    existingFavTraveler.UpdatedAt = DateTime.UtcNow;
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
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
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

            if (order.ExpiresAt.HasValue && order.ExpiresAt.Value <= DateTime.UtcNow)
            {
                _logger.LogWarning("�q�� {OrderId} �w�� {ExpiresAt} �L���C", orderId, order.ExpiresAt.Value);
                order.Status = OrderStatus.Expired; // �D�ʧ�s���A
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "���q�檺�I�ڮɶ��w�L���A�Э��s�U��C" });
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
        /// �d�߷�e�ϥΪ̬O�_�����D���B���L�����ݥI�ڭq��C
        /// </summary>
        /// <returns>PendingOrderStatusDto �� NotFound</returns>
        [HttpGet("active-pending")] // �Ҧp: GET /api/Order/active-pending
        public async Task<ActionResult<PendingOrderStatusDto>> GetActivePendingOrderForCurrentUser([FromQuery] int? memberId)
        {
            _logger.LogInformation("[GetActivePendingOrder] API endpoint hit. Received memberId from query: {QueryMemberId}", memberId);
            if (memberId == null || memberId.Value <= 0) // << �ק��I�G�ˬd�ǤJ�� memberId
            {
                _logger.LogWarning("[GetActivePendingOrder] Invalid memberId received from query: {QueryMemberId}", memberId);
                return BadRequest(new { message = "�ݭn���Ѧ��Ī� memberId�C" });
            }

            var activePendingOrder = await _context.Orders
                .Where(o => o.MemberId == memberId.Value &&
                             o.Status == OrderStatus.Awaiting && // �u�䪬�A�� Awaiting ��
                             o.ExpiresAt.HasValue && o.ExpiresAt.Value > DateTime.UtcNow) // �B�|���L��
                .OrderByDescending(o => o.CreatedAt) // �p�G���h���A���̷s�� (�z�פW���ӥu���@�����D��)
                .Select(o => new PendingOrderStatusDto
                {
                    OrderId = o.OrderId,
                    MerchantTradeNo = o.MerchantTradeNo,
                    Status = o.Status.ToString(), // �N enum �ର string
                    ExpiresAt = o.ExpiresAt,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod.ToString() // �N enum �ର string
                })
                .FirstOrDefaultAsync();

            if (activePendingOrder == null)
            {
                _logger.LogInformation("GetActivePendingOrderForCurrentUser: MemberId {MemberId} �S����쬡�D���ݥI�ڭq��C", memberId.Value);
                return NotFound(new { message = "�ثe�S���ݥI�ڪ��q��C" });
            }

            _logger.LogInformation("GetActivePendingOrderForCurrentUser: MemberId {MemberId} ���ݥI�ڭq�� {@ActiveOrder}", memberId.Value, activePendingOrder);
            return Ok(activePendingOrder);
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
                .Include(o => o.OrderParticipants)
                .Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.GroupTravel) // �p�G�ݭn�b�q��Ա������ GroupTravel ����T
                //    .ThenInclude(od => od.CustomTravel)   // �p�G�ݭn�b�q��Ա������ CustomTravel ����T
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
                            .FirstOrDefaultAsync(gt => gt.GroupTravelId == detail.ItemId);
                    }
                    else if (detail.Category == ProductCategory.CustomTravel)
                    {
                        // �ϥ� ItemId �q _context.CustomTravels �d��
                        // �T�O CustomTravel ���馳�P detail.ItemId �������D��A�Ҧp CustomTravelId
                        detail.CustomTravel = await _context.CustomTravels
                            .FirstOrDefaultAsync(ct => ct.CustomTravelId == detail.ItemId);
                    }
                }
            }
            // �Ȯɦ^�� order ����A���إߨèϥΤ@�ӸԲӪ� OrderDto
            var orderDto = new // �A���ӥΧA�w�q�� OrderDto
            {
                OrderId = orderData.OrderId,
                MemberId = orderData.MemberId,
                OrdererName = orderData.OrdererName,
                OrdererPhone = orderData.OrdererPhone,
                OrdererEmail = orderData.OrdererEmail,
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

                Participants = orderData.OrderParticipants?.Select(p => new OrderParticipantDto
                {
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
                }).ToList() ?? new List<OrderParticipantDto>(),
                OrderDetails = orderData.OrderDetails?.Select(od =>
                {
                    string productTitle = string.Empty;
                    DateTime? itemStartDate = null;
                    DateTime? itemEndDate = null;
                    if (od.Category == ProductCategory.GroupTravel && od.GroupTravel != null)
                    {
                        productTitle = od.GroupTravel.OfficialTravelDetail?.OfficialTravel?.Title ?? string.Empty;
                        // ���] OfficialTravelDetail �� GroupTravel �� StartDate/EndDate
                        // itemStartDate = od.GroupTravel.OfficialTravelDetail?.StartDate;
                        // itemEndDate = od.GroupTravel.OfficialTravelDetail?.EndDate;
                    }
                    else if (od.Category == ProductCategory.CustomTravel && od.CustomTravel != null)
                    {
                        productTitle = od.CustomTravel.Note ?? $"�Ȼs�Ʀ�{ {od.CustomTravel.CustomTravelId}";
                        // ���] CustomTravel �� StartDate/EndDate
                        // itemStartDate = od.CustomTravel.StartDate;
                        // itemEndDate = od.CustomTravel.EndDate;
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
                        StartDate = itemStartDate,
                        EndDate = itemEndDate,   
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

    }

}