// File: Controllers/OrderController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For ToListAsync, Include, etc.
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyFrontendAPI.DTOs.OrderDTOs; // Your Order DTOs namespace
using System.Security.Claims; // For HttpContext.User to get MemberId
using Microsoft.AspNetCore.Authorization; // If you want to protect this endpoint

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

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("�ϥΪ̥��n�J�εL�k�ѧO�����C");
            }

            var memberExists = await _context.Members.AnyAsync(m => m.MemberId == currentUserId.Value);
            if (!memberExists)
            {
                return NotFound($"�|�� ID {currentUserId.Value} ���s�b�C");
            }

            // ��l�� Order ����
            var order = new Order
            {
                MemberId = currentUserId.Value,

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
                    OrderId = order.OrderId,
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
                    currentUserId.Value, calculatedServerTotalAmount, orderCreateDto.TotalAmount);
            }

            // 4. �B�z�ȫȦC�� (OrderParticipants)
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

                if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                {
                    // ���ձq�`�ήȫ�Ū�����
                    var favoriteTraveler = await _context.MemberFavoriteTravelers
                        .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                   ft.MemberId == currentUserId.Value && // �T�O�O�ӷ|�����`�ήȫ�
                                                   ft.Status == FavoriteStatus.Active); // �T�O�`�ήȫȬO���Ī�

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
                    await _context.SaveChangesAsync();
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
                // �z�i���ٻݭn�^�Ǥ@�ӱM������I�h�D�ϥΪ� token �έ��s�ɦV URL ���Ѽ�
                // PaymentGatewayRedirectUrl = ... (�o�ݭn�ھڤ�I�h�DAPI�M�w)
                // PaymentReference = ...
            };

            // �ϥ� CreatedAtAction �^�� 201 Created�A�ô��ѷs�귽�� URI �M���e
            // �z�ݭn���@�� GetOrder(int id) ����k���� CreatedAtAction ���`�B�@
            // ��²�ơA�o�̥������^�� Ok �� Created (���a Location Header)
            // return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, orderSummary);
            return Created($"/api/Order/{order.OrderId}", orderSummary); // ���]����|��GET api/Order/{id}
        }


        // ���U��k�G�����e�n�J�ϥΪ̪� MemberId
        // ��ڹ�@�|�ھڱz���������ҳ]�w�Ӧ��Ҥ��P
        private int? GetCurrentUserId()
        {
            return 11110;
            //var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            //if (int.TryParse(userIdClaim, out int userId))
            //{
            //    return userId;
            //}

            //// var memberIdClaim = User.FindFirstValue("MemberId");
            //// if (int.TryParse(memberIdClaim, out int memberId)) return memberId;

            //_logger.LogWarning("�L�k�q Claims ���ѪR UserId�C");
            //return null;
        }

        // GET: api/Order/{id} (�d�ҡA�Ω� CreatedAtAction �M�e�ݬd�߭q��)


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


        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id) //  OrderDto �Ω���ܭq��Ա�
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var orderData = await _context.Orders
                .Include(o => o.OrderParticipants)
                .Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.GroupTravel) // �p�G�ݭn�b�q��Ա������ GroupTravel ����T
                //.Include(o => o.OrderDetails)
                //    .ThenInclude(od => od.CustomTravel)   // �p�G�ݭn�b�q��Ա������ CustomTravel ����T
                .FirstOrDefaultAsync(o => o.OrderId == id && o.MemberId == currentUserId.Value);

            if (orderData == null)
            {
                return NotFound($"�䤣��q�� ID {id}�A�αz�L�v�s�����q��C");
            }

            // ��ʸ��J���p�� GroupTravel �M CustomTravel (�p�G�ݭn�B���b�W�� Include)
            // �o�ؤ覡�Ĳv�i��y�t�A�������[
            foreach (var detail in orderData.OrderDetails)
            {
                if (detail.Category == ProductCategory.GroupTravel)
                {
                    await _context.Entry(detail).Reference(d => d.GroupTravel).LoadAsync();
                    if (detail.GroupTravel != null)
                    {
                        await _context.Entry(detail.GroupTravel).Reference(gt => gt.OfficialTravelDetail).LoadAsync();
                        if (detail.GroupTravel.OfficialTravelDetail != null)
                        {
                            await _context.Entry(detail.GroupTravel.OfficialTravelDetail).Reference(otd => otd.OfficialTravel).LoadAsync();
                        }
                    }
                }
                else if (detail.Category == ProductCategory.CustomTravel)
                {
                    await _context.Entry(detail).Reference(d => d.CustomTravel).LoadAsync();
                }
            }

            // �Ȯɦ^�� order ����A���إߨèϥΤ@�ӸԲӪ� OrderDto
            return Ok(new
            {
                orderData.OrderId,
                orderData.MemberId,
                OrdererName = orderData.OrdererName,
                OrdererPhone = orderData.OrdererPhone,
                OrdererEmail = orderData.OrdererEmail,
                orderData.TotalAmount,
                PaymentMethod = orderData.PaymentMethod.ToString(),
                Status = orderData.Status.ToString(),
                orderData.CreatedAt,
                orderData.PaymentDate,
                orderData.Note,
                orderData.InvoiceOption,
                Participants = orderData.OrderParticipants.Select(p => new
                {
                    p.Name,
                    //p.Phone,
                    //p.Email,
                    p.BirthDate,
                    p.IdNumber,
                    p.Gender,
                    p.DocumentType
                }).ToList(),
                OrderDetails = orderData.OrderDetails.Select(od => new
                {
                    ProductTypeName = od.Category.ToString(),
                    ItemId = od.ItemId,
                    ProductName = od.Category == ProductCategory.GroupTravel ?
                                  od.GroupTravel?.OfficialTravelDetail?.OfficialTravel?.Title :
                                 (od.Category == ProductCategory.CustomTravel ? od.CustomTravel?.Note : "N/A"),
                    od.Description,
                    od.Quantity,
                    od.Price,
                    od.TotalAmount
                }).ToList()
            });
        }

    }

    // �w�q�o�� OrderSummaryDto�A�Ω� PostOrder ���^��
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string SelectedPaymentMethod { get; set; }
        // public string PaymentGatewayRedirectUrl { get; set; } // �Ω��I
        // public string PaymentReference { get; set; } // 
    }

    // �z�i���ٻݭn�@�ӧ�ԲӪ� OrderDto�A�Ω� GetOrderById
    // public class OrderDto { ... }
}