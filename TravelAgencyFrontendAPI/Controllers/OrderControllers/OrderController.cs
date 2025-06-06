
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

            string tempGuidPart = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string timePartForMtn = DateTime.Now.ToString("yyMMddHHmmss");
            string prefixForMtn = "TRVORD";
            string mtnBase = $"{prefixForMtn}{timePartForMtn}{tempGuidPart}";
            string merchantTradeNo = new string(mtnBase.Where(char.IsLetterOrDigit).ToArray());
            if (merchantTradeNo.Length > 20)
            {
                merchantTradeNo = merchantTradeNo.Substring(0, 20);
            }
            _logger.LogInformation("���q�沣�ͪ� MerchantTradeNo: {MerchantTradeNo}", merchantTradeNo);

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
                ExpiresAt = DateTime.Now.AddMinutes(3),
                MerchantTradeNo = merchantTradeNo,
                Note = orderCreateDto.OrderNotes,
                OrderDetails = new List<OrderDetail>(),
                OrderParticipants = new List<OrderParticipant>()
            };

            if (orderCreateDto.CartItems == null || !orderCreateDto.CartItems.Any())
            {
                ModelState.AddModelError("CartItems", "�ʪ������S���ӫ~�C");
                return BadRequest(ModelState);
            }

            int participantDtoGlobalIndex = 0;

            // --- �i�֤߳B�z�j��G�إ߭q����ӻP�ȫȡj ---
            foreach (var cartItemDto in orderCreateDto.CartItems)
            {
                if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var itemCategory))
                {
                    return BadRequest($"���䴩���ӫ~����: {cartItemDto.ProductType}�C");
                }

                if (itemCategory == ProductCategory.GroupTravel)
                {
                    var groupTravel = await _context.GroupTravels.Include(gt => gt.OfficialTravelDetail).ThenInclude(otd => otd.OfficialTravel)
                        .AsNoTracking().FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "�i���W");

                    if (groupTravel?.OfficialTravelDetail?.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                        return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) ��T������Υ���w����C");

                    var priceDetail = groupTravel.OfficialTravelDetail;
                    decimal unitPrice = cartItemDto.OptionType.ToUpper() switch
                    {
                        "���H" or "ADULT" => priceDetail.AdultPrice ?? 0,
                        "�ൣ�[��" => priceDetail.AdultPrice ?? 0, // ���]����
                        "�ൣ����" => priceDetail.ChildPrice ?? 0,
                        "�ൣ������" => priceDetail.BabyPrice ?? 0, // ���]����
                        "�ൣ" or "CHILD" => priceDetail.ChildPrice ?? 0,
                        "����" or "BABY" => priceDetail.BabyPrice ?? 0,
                        _ => 0
                    };
                    if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"�ӫ~ '{groupTravel.OfficialTravelDetail.OfficialTravel.Title}' ���ﶵ '{cartItemDto.OptionType}' ���沧�`�C");

                    var groupOrderDetail = new OrderDetail
                    {
                        Order = order,
                        Category = itemCategory,
                        ItemId = cartItemDto.ProductId,
                        Description = $"{groupTravel.OfficialTravelDetail.OfficialTravel.Title} - {cartItemDto.OptionType}",
                        Quantity = cartItemDto.Quantity,
                        Price = unitPrice,
                        TotalAmount = cartItemDto.Quantity * unitPrice,
                        StartDate = groupTravel.DepartureDate,
                        EndDate = groupTravel.ReturnDate,
                        OrderParticipants = new List<OrderParticipant>()
                    };

                    for (int i = 0; i < groupOrderDetail.Quantity; i++)
                    {
                        if (participantDtoGlobalIndex >= orderCreateDto.Participants.Count) return BadRequest("�ȫȸ�Ƽƶq���� (����ȹC)�C");
                        var participantDto = orderCreateDto.Participants[participantDtoGlobalIndex++];
                        var participant = MapParticipantFromDto(participantDto, order, groupOrderDetail); // �ϥλ��U��k
                                                                                                          // �i��G�b�o�̥[�J�`�ήȫȸ�ƶ�R�޿�
                        groupOrderDetail.OrderParticipants.Add(participant);
                        order.OrderParticipants.Add(participant);
                    }
                    order.OrderDetails.Add(groupOrderDetail);
                }
                else if (itemCategory == ProductCategory.CustomTravel)
                {
                    var customTravel = await _context.CustomTravels.AsNoTracking().FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Approved);
                    if (customTravel == null) return BadRequest($"�Ȼs�Ʀ�{ (ID: CT{cartItemDto.ProductId}) �L�ĩΪ��A���šC");
                    if (customTravel.People <= 0) return BadRequest("�Ȼs�Ʀ�{�H�ƥ����j�� 0�C");

                    decimal perPersonPrice = customTravel.TotalAmount / customTravel.People;

                    for (int i = 0; i < customTravel.People; i++)
                    {
                        if (participantDtoGlobalIndex >= orderCreateDto.Participants.Count) return BadRequest("�ȫȸ�Ƽƶq�P�Ȼs�Ʀ�{�H�Ƥ��šC");

                        var customOrderDetail = new OrderDetail
                        {
                            Order = order,
                            Category = itemCategory,
                            ItemId = cartItemDto.ProductId,
                            Description = $"{customTravel.Note ?? "�Ȼs�Ʀ�{"} - {cartItemDto.OptionType}",
                            Quantity = 1,
                            Price = perPersonPrice,
                            TotalAmount = perPersonPrice,
                            StartDate = customTravel.DepartureDate,
                            EndDate = customTravel.EndDate,
                            OrderParticipants = new List<OrderParticipant>()
                        };

                        var participantDto = orderCreateDto.Participants[participantDtoGlobalIndex++];
                        var participant = MapParticipantFromDto(participantDto, order, customOrderDetail); // �ϥλ��U��k
                                                                                                           // �i��G�b�o�̥[�J�`�ήȫȸ�ƶ�R�޿�

                        customOrderDetail.OrderParticipants.Add(participant);
                        order.OrderDetails.Add(customOrderDetail);
                        order.OrderParticipants.Add(participant);
                    }
                }
            }

            //�i���I�j�ª��B�h�l���ȫȤ��t�j��w�Q�����A�Ҧ��޿賣�b�W�觹���C

            order.TotalAmount = order.OrderDetails.Sum(d => d.TotalAmount);

            // �b�x�s�e�̫�@������ ModelState
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
                    await _context.SaveChangesAsync();

                    // �B�z�`�ήȫȪ���s�ηs�W
                    if (orderCreateDto.TravelerProfileActions != null && orderCreateDto.TravelerProfileActions.Any())
                    {
                        // ... ���B�O�z�쥻�� TravelerProfileActions �޿�A�O������ ...
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "�إ߭q��ɵo�Ϳ��~�C");
                    return StatusCode(StatusCodes.Status500InternalServerError, "�إ߭q��ɵo�Ϳ��~�C");
                }
            }

            var orderSummary = new OrderSummaryDto
            {
                OrderId = order.OrderId,
                MerchantTradeNo = order.MerchantTradeNo,
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ExpiresAt = order.ExpiresAt
            };
            _logger.LogInformation("��B�q��إߦ��\�A�^�� orderSummary: {@OrderSummary}", orderSummary);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId, memberId = order.MemberId }, orderSummary);
        }

        // ��ĳ�s�W�@�ӻ��U��k�ӬM�g Participant�A���D��k�󰮲b
        private OrderParticipant MapParticipantFromDto(OrderParticipantDto dto, Order order, OrderDetail detail)
        {
            // �b�o�̥i�H�[�J�z�� participantDto �������޿�
            if (dto.DocumentType == DocumentType.PASSPORT && string.IsNullOrEmpty(dto.DocumentNumber))
            {
                ModelState.AddModelError($"Participants[{dto.Name}].DocumentNumber", "����@�ӧ@���ҥ������ɡA�@�Ӹ��X������C");
            }
            if (string.IsNullOrEmpty(dto.IdNumber) && string.IsNullOrEmpty(dto.DocumentNumber))
            {
                ModelState.AddModelError($"Participants[{dto.Name}].IdOrDocumentNumber", "�ȫȨ����Ҹ����ҥ󸹽X�ܤֻݶ�g�@���C");
            }

            return new OrderParticipant
            {
                Order = order,
                OrderDetail = detail,
                Name = dto.Name,
                BirthDate = dto.BirthDate,
                IdNumber = dto.IdNumber,
                Gender = dto.Gender,
                DocumentType = dto.DocumentType,
                DocumentNumber = dto.DocumentNumber,
                PassportSurname = dto.PassportSurname,
                PassportGivenName = dto.PassportGivenName,
                PassportExpireDate = dto.PassportExpireDate,
                Nationality = dto.Nationality,
                Note = dto.Note,
                FavoriteTravelerId = dto.FavoriteTravelerId
            };
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

        [HttpPut("{orderId}")]
        public async Task<ActionResult<OrderSummaryDto>> UpdateOrderAsync(int orderId, [FromBody] OrderUpdateDto orderUpdateDto)
        {
            _logger.LogInformation("������q���s�ШD�AOrderID: {OrderId}, DTO: {@OrderUpdateDto}", orderId, orderUpdateDto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.OrderParticipants)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

                    if (order == null)
                    {
                        return NotFound(new { message = $"�䤣��q�� {orderId} �αz�L�v�ק惡�q��C" });
                    }

                    if (order.Status != OrderStatus.Awaiting)
                    {
                        return BadRequest(new { message = $"�q��ثe���A�� '{order.Status}'�A�L�k�ק�C" });
                    }

                    // 1. ��s Order ���򥻸��
                    order.OrdererName = orderUpdateDto.OrdererInfo.Name;
                    order.OrdererPhone = NormalizePhoneNumber(orderUpdateDto.OrdererInfo.MobilePhone);
                    order.OrdererEmail = orderUpdateDto.OrdererInfo.Email;
                    order.OrdererNationality = orderUpdateDto.OrdererInfo.Nationality;
                    order.OrdererDocumentNumber = orderUpdateDto.OrdererInfo.DocumentNumber;
                    order.OrdererDocumentType = orderUpdateDto.OrdererInfo.DocumentType;
                    order.Note = orderUpdateDto.OrderNotes;
                    order.UpdatedAt = DateTime.Now;
                    order.ExpiresAt = DateTime.Now.AddMinutes(3);

                    // 2. �R���Ҧ��ª� OrderDetails �M���p�� OrderParticipants
                    if (order.OrderDetails.Any())
                    {
                        var participantsToRemove = order.OrderDetails.SelectMany(od => od.OrderParticipants).ToList();
                        if (participantsToRemove.Any())
                        {
                            _context.OrderParticipants.RemoveRange(participantsToRemove);
                        }
                        _context.OrderDetails.RemoveRange(order.OrderDetails);

                        // �j�� EF Core ����R���ާ@�A�קK�b�P�@�� transaction ���]�D��Ĭ�ӥX��
                        await _context.SaveChangesAsync();
                    }

                    // 3. �ھ� orderUpdateDto ���s�ЫةҦ� OrderDetails �M OrderParticipants
                    int participantDtoGlobalIndex = 0;

                    if (orderUpdateDto.CartItems == null || !orderUpdateDto.CartItems.Any())
                    {
                        return BadRequest(new { message = "��s�q��ɡA�ʪ������S���ӫ~�C" });
                    }

                    var distinctCartItems = orderUpdateDto.CartItems
                        .GroupBy(item => new { item.ProductId, item.ProductType })
                        .Select(group => group.First()) // ���ۦP�����~�A�u���Ĥ@��
                        .ToList();

                    // --- �i�֤߭��c�ϰ�G�P InitiateOrderAsync �P�B���޿�j ---
                    foreach (var cartItemDto in distinctCartItems)
                    {
                        if (!Enum.TryParse<ProductCategory>(cartItemDto.ProductType, true, out var itemCategory))
                        {
                            return BadRequest($"���䴩���ӫ~����: {cartItemDto.ProductType}�C");
                        }

                        if (itemCategory == ProductCategory.GroupTravel)
                        {
                            var groupTravel = await _context.GroupTravels.Include(gt => gt.OfficialTravelDetail).ThenInclude(otd => otd.OfficialTravel)
                                .AsNoTracking().FirstOrDefaultAsync(gt => gt.GroupTravelId == cartItemDto.ProductId && gt.GroupStatus == "�i���W");

                            if (groupTravel?.OfficialTravelDetail?.OfficialTravel == null || groupTravel.OfficialTravelDetail.State != DetailState.Locked)
                                return BadRequest($"�����{ (ID: GT{cartItemDto.ProductId}) ��T������Υ���w����C");

                            var priceDetail = groupTravel.OfficialTravelDetail;
                            decimal unitPrice = cartItemDto.OptionType.ToUpper() switch
                            {
                                "���H" or "ADULT" => priceDetail.AdultPrice ?? 0,
                                "�ൣ�[��" => priceDetail.AdultPrice ?? 0,
                                "�ൣ����" => priceDetail.ChildPrice ?? 0,
                                "�ൣ������" => priceDetail.BabyPrice ?? 0,
                                "�ൣ" or "CHILD" => priceDetail.ChildPrice ?? 0,
                                "����" or "BABY" => priceDetail.BabyPrice ?? 0,
                                _ => 0
                            };
                            if (unitPrice <= 0 && cartItemDto.Quantity > 0) return BadRequest($"�ӫ~ '{groupTravel.OfficialTravelDetail.OfficialTravel.Title}' ���ﶵ '{cartItemDto.OptionType}' ���沧�`�C");

                            var groupOrderDetail = new OrderDetail
                            {
                                Order = order,
                                Category = itemCategory,
                                ItemId = cartItemDto.ProductId,
                                Description = $"{groupTravel.OfficialTravelDetail.OfficialTravel.Title} - {cartItemDto.OptionType}",
                                Quantity = cartItemDto.Quantity,
                                Price = unitPrice,
                                TotalAmount = cartItemDto.Quantity * unitPrice,
                                StartDate = groupTravel.DepartureDate,
                                EndDate = groupTravel.ReturnDate,
                                OrderParticipants = new List<OrderParticipant>()
                            };

                            for (int i = 0; i < groupOrderDetail.Quantity; i++)
                            {
                                if (participantDtoGlobalIndex >= orderUpdateDto.Participants.Count) return BadRequest("�ȫȸ�Ƽƶq���� (����ȹC)�C");
                                var participantDto = orderUpdateDto.Participants[participantDtoGlobalIndex++];
                                var participant = MapParticipantFromDto(participantDto, order, groupOrderDetail);
                                groupOrderDetail.OrderParticipants.Add(participant);
                                order.OrderParticipants.Add(participant);
                            }
                            order.OrderDetails.Add(groupOrderDetail);
                        }
                        else if (itemCategory == ProductCategory.CustomTravel)
                        {
                            var customTravel = await _context.CustomTravels.AsNoTracking().FirstOrDefaultAsync(ct => ct.CustomTravelId == cartItemDto.ProductId && ct.Status == CustomTravelStatus.Approved);
                            if (customTravel == null) return BadRequest($"�Ȼs�Ʀ�{ (ID: CT{cartItemDto.ProductId}) �L�ĩΪ��A���šC");
                            if (customTravel.People <= 0) return BadRequest("�Ȼs�Ʀ�{�H�ƥ����j�� 0�C");

                            decimal perPersonPrice = customTravel.TotalAmount / customTravel.People;

                            for (int i = 0; i < customTravel.People; i++)
                            {
                                if (participantDtoGlobalIndex >= orderUpdateDto.Participants.Count) return BadRequest("�ȫȸ�Ƽƶq�P�Ȼs�Ʀ�{�H�Ƥ��šC");

                                var customOrderDetail = new OrderDetail
                                {
                                    Order = order,
                                    Category = itemCategory,
                                    ItemId = cartItemDto.ProductId,
                                    Description = $"{customTravel.Note ?? "�Ȼs�Ʀ�{"} - {cartItemDto.OptionType}",
                                    Quantity = 1,
                                    Price = perPersonPrice,
                                    TotalAmount = perPersonPrice,
                                    StartDate = customTravel.DepartureDate,
                                    EndDate = customTravel.EndDate,
                                    OrderParticipants = new List<OrderParticipant>()
                                };

                                var participantDto = orderUpdateDto.Participants[participantDtoGlobalIndex++];
                                var participant = MapParticipantFromDto(participantDto, order, customOrderDetail);

                                customOrderDetail.OrderParticipants.Add(participant);
                                order.OrderDetails.Add(customOrderDetail);
                                order.OrderParticipants.Add(participant);
                            }
                        }
                    }
                    // --- �i�֤߭��c�ϰ쵲���j ---

                    order.TotalAmount = order.OrderDetails.Sum(d => d.TotalAmount);

                    // 4. �B�z�`�ήȫȪ���s (�p�G�ݭn)
                    if (orderUpdateDto.TravelerProfileActions != null && orderUpdateDto.TravelerProfileActions.Any())
                    {
                        // ... �z�쥻�� TravelerProfileActions �޿�i�H��b�o�� ...
                    }

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    await _context.SaveChangesAsync();
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
    