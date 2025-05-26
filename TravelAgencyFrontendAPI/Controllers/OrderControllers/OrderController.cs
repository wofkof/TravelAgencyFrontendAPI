
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.OrderDTOs;
using Microsoft.Extensions.Logging; // �[�J ILogger
using System; // �[�J System
using System.Collections.Generic; // �[�J List
using System.Linq; // �[�J Linq
using System.Threading.Tasks; // �[�J Task
using System.ComponentModel.DataAnnotations; // �p�G DTO �ϥ� DataAnnotations

namespace TravelAgencyFrontendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // �[�W�o�ӡA�T�O�u���n�J�|���~��U��
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(AppDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Order
        // �إ߷s���q�� (��l���A�A�ݥI��)
        [HttpPost]
        public async Task<ActionResult<OrderSummaryDto>> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var memberIdFromDto = orderCreateDto.MemberId;
            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == memberIdFromDto);
            if (member == null)
            {
                _logger.LogWarning("CreateOrder �䤣��|���A�|�� ID (�Ӧ۫e��DTO): {MemberId}", memberIdFromDto);
                // ��^���T�����~�A�i���e�ݴ��Ѫ� MemberId �䤣��������|��
                ModelState.AddModelError(nameof(orderCreateDto.MemberId), $"���Ѫ��|�� ID {memberIdFromDto} ���s�b�C");
                return BadRequest(ModelState);
            }

            // ��l�� Order ����
            var order = new Order
            {
                MemberId = member.MemberId,

                OrdererName = orderCreateDto.OrdererInfo.Name, // �q�ʤH�m�W
                OrdererPhone = NormalizePhoneNumber(orderCreateDto.OrdererInfo.MobilePhone), // �q�ʤH�q��
                OrdererEmail = orderCreateDto.OrdererInfo.Email, // �q�ʤHEmail

                PaymentMethod = orderCreateDto.SelectedPaymentMethod, // �ϥΪ̿�ܪ��I�ڤ覡
                Status = OrderStatus.Awaiting, // ��l���A�]��Awaiting(�ݥI��)
                CreatedAt = DateTime.UtcNow, // �ϥ� UTC �ɶ�
                Note = orderCreateDto.OrderNotes,
                // �B�z�o���ШD��T
                InvoiceOption = orderCreateDto.InvoiceRequestInfo.InvoiceOption,
                InvoiceDeliveryEmail = orderCreateDto.InvoiceRequestInfo.InvoiceDeliveryEmail,
                InvoiceUniformNumber = orderCreateDto.InvoiceRequestInfo.InvoiceUniformNumber,
                InvoiceTitle = orderCreateDto.InvoiceRequestInfo.InvoiceTitle,
                InvoiceAddBillingAddr = orderCreateDto.InvoiceRequestInfo.InvoiceAddBillingAddr,
                InvoiceBillingAddress = orderCreateDto.InvoiceRequestInfo.InvoiceBillingAddress,
                OrderDetails = new List<OrderDetail>(), // ��l�ƭq����Ӷ��X
                OrderParticipants = new List<OrderParticipant>() // ��l�Ʈȫȶ��X
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

                if (Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var parsedCategory)) // true ��ܩ����j�p�g
                {
                    itemCategory = parsedCategory;
                }
                else
                {
                    _logger.LogWarning($"�L�Ī� ProductType: {cartItemDto.ProductType}�C");
                    return BadRequest($"���䴩���ӫ~����: {cartItemDto.ProductType}�C");
                }
                if (itemCategory == ProductCategory.GroupTravel)
                {
                    var groupTravel = await _context.GroupTravels
                                                .Include(gt => gt.OfficialTravelDetail)
                                                    .ThenInclude(otd => otd.OfficialTravel)
                                                .FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "�}��");

                    if (groupTravel == null)
                    {
                        return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) �L�ġB���}����W�A�Χ䤣���������{��ơC");
                    }
                    if (groupTravel.OfficialTravelDetail == null)
                    {
                        return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) �ʤ֥��n��������ӳ]�w (OfficialTravelDetail)�C");
                    }
                    if (groupTravel.OfficialTravelDetail.OfficialTravel == null)
                    {
                        return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) ���p���x���{�D�ɸ�ƿ򥢡C");
                    }
                    if (groupTravel.OfficialTravelDetail.State != DetailState.Locked) // ���]���楲���O��w���A
                    {
                        return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) �������T�ثe����w�A�L�k�U��C");
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
                    //OrderId = order.OrderId,
                    Category = itemCategory,                     // << ��� ProductCategory
                    ItemId = cartItemDto.ProductId,              // << ��� ItemId (GroupTravelId �� CustomTravelId)
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
                    // �b�Ĥ@�� SaveChangesAsync ���e��� MerchantTradeNo (���̿� OrderId ������)
                    // �Ϊ̡A�p�G MerchantTradeNo �����]�t OrderId�A�h�ݭn�b�Ĥ@�� SaveChangesAsync ����A��s�@���C
                    // �o�̱ĥΥ����ͤ@�Ӱ��ɶ��W�MGUID���q���ߤ@�s���A���̿� OrderId�C
                    // �p�G�z�� ECPayService.GenerateEcPayPaymentForm ���� MerchantTradeNo �ͦ��޿�󧹵��A�i�H�Ҽ{�N�䴣�����@�Τ�k�C
                    string tempGuidPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // �W�[�ߤ@��
                    string timePartForMtn = DateTime.UtcNow.ToString("yyMMddHHmmss"); // �ϥ� UTC �ɶ����
                    string prefixForMtn = "TRV";
                    string mtnBase = $"{prefixForMtn}{timePartForMtn}{tempGuidPart}";
                    order.MerchantTradeNo = new string(mtnBase.Where(char.IsLetterOrDigit).ToArray());
                    if (order.MerchantTradeNo.Length > 20)
                    {
                        order.MerchantTradeNo = order.MerchantTradeNo.Substring(0, 20);
                    }
                    _logger.LogInformation("���s�q�沣�ͪ� MerchantTradeNo: {MerchantTradeNo}", order.MerchantTradeNo);

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // (�i��) �p�G MerchantTradeNo �j�P��ĳ�]�t OrderId�A�i�H�b���B��s�æA���x�s
                    bool requiresMtnUpdateWithOrderId = false; // �ھڱz���ݨD�]�w���лx
                    if (requiresMtnUpdateWithOrderId)
                    {
                        string orderIdPartForMtn = order.OrderId.ToString("D5"); // �Ҧp�ɹs��5��
                        string finalMtnBase = $"{prefixForMtn}{orderIdPartForMtn}{timePartForMtn}"; // ���] timePartForMtn �w�b�W���w�q
                        order.MerchantTradeNo = new string(finalMtnBase.Where(char.IsLetterOrDigit).ToArray());
                        if (order.MerchantTradeNo.Length > 20)
                        {
                            order.MerchantTradeNo = order.MerchantTradeNo.Substring(0, 20);
                        }
                        _context.Orders.Update(order); // �аO����s
                        await _context.SaveChangesAsync(); // �ĤG���x�s�H��s MerchantTradeNo
                        _logger.LogInformation("�w�ϥ� OrderId ��s�q�� MerchantTradeNo: {MerchantTradeNo}", order.MerchantTradeNo);
                    }
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
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                SelectedPaymentMethod = order.PaymentMethod?.ToString(),

            };
            return Created($"/api/Order/{order.OrderId}", orderSummary); // ���]����|��GET api/Order/{id}
        }

        // *** �b�o�̲K�[�q�ܸ��X���W�ƪ��p�� Helper Method ***
        private string NormalizePhoneNumber(string phoneNumberString)
        {
            if (string.IsNullOrEmpty(phoneNumberString))
            {
                return phoneNumberString; // �p�G�O null �ΪŦr��A������^
            }

            string normalizedNumber = phoneNumberString.Trim(); // �����e��ť�

            // �ˬd�O�_�H "+" �}�Y�A�B���ר����i���ˬd (�ܤ֥]�t +��X�Ʀr0�Ʀr �� +��X�Ʀr�Ʀr)
            if (normalizedNumber.StartsWith("+") && normalizedNumber.Length >= 3) // ���]�̵u��X�Ʀr�ܤ�2��A�p +886
            {
                // ����X�᭱���Ʀr����
                int firstDigitAfterPlus = 1; // �q '+' ���U�@�Ӧr���}�l
                int firstNonDigitIndexAfterPlus = firstDigitAfterPlus;

                // ����X�Ʀr������������m (�Ĥ@�ӫD�Ʀr�r��)
                while (firstNonDigitIndexAfterPlus < normalizedNumber.Length && char.IsDigit(normalizedNumber[firstNonDigitIndexAfterPlus]))
                {
                    firstNonDigitIndexAfterPlus++;
                }

                // �p�G��X�Ʀr�����s�b�A�B�᭱�٦����X
                if (firstNonDigitIndexAfterPlus > firstDigitAfterPlus && firstNonDigitIndexAfterPlus < normalizedNumber.Length)
                {
                    string countryCodeWithPlus = normalizedNumber.Substring(0, firstNonDigitIndexAfterPlus); // �]�t "+" �M��X�Ʀr
                    string restOfNumber = normalizedNumber.Substring(firstNonDigitIndexAfterPlus); // ��X�᭱�����X����

                    // �ˬd���X����l�����O�_�H "0" �}�Y�A�åB����פj�� 1
                    if (restOfNumber.StartsWith("0") && restOfNumber.Length > 1)
                    {
                        // �����}�Y�� "0"
                        restOfNumber = restOfNumber.Substring(1);
                    }

                    // ���s�զX��X�M�B�z�᪺���X
                    normalizedNumber = countryCodeWithPlus + restOfNumber;
                }
                // else: �p�G�榡���ŦX�w�� (�Ҧp�u�� + �� +��X�S�����򸹽X)�A�O�����
            }
            // else: �p�G���H "+" �}�Y�A�ھڱz���ݨD�M�w�p��B�z�A�o�̫O�����

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
                //.Include(o => o.OrderDetails)
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
                PaymentMethod = orderData.PaymentMethod?.ToString(),
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
                Participants = orderData.OrderParticipants?.Select(p => new OrderParticipantDto
                {
                    Name = p.Name,
                    BirthDate = p.BirthDate,
                    IdNumber = p.IdNumber,
                    Gender = p.Gender, // �ഫ���r��
                    DocumentType = p.DocumentType // �ഫ���r��
                }).ToList() ?? new List<OrderParticipantDto>(),
                OrderDetails = orderData.OrderDetails?.Select(od => new OrderDetailItemDto 
                {
                    //Category = od.Category, // DTO ���i��O string
                    Category = od.Category, // Enum �i�H������ȡA�p�G DTO �]�O Enum
                    ItemId = od.ItemId,
                    Description = od.Category == ProductCategory.GroupTravel ?
                                  od.GroupTravel?.OfficialTravelDetail?.OfficialTravel?.Title :
                                 (od.Category == ProductCategory.CustomTravel ? od.CustomTravel?.Note : od.Description), // �վ�~�W���
                    Quantity = od.Quantity,
                    Price = od.Price,
                    TotalAmount = od.TotalAmount
                    // ... ��L OrderDetailItemDto ��� ...
                }).ToList() ?? new List<OrderDetailItemDto>(),
            };

            return Ok(orderDto);
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