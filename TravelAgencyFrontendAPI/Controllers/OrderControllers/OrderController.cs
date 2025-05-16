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
    // [Authorize] // <--- ��ĳ�[�W�o�ӡA�T�O�u���n�J�|���~��U��
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger; // For logging

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

            // 1. �����e�n�J�� MemberId
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("�ϥΪ̥��n�J�εL�k�ѧO�����C");
            }

            // 2. ���� Member �O�_�s�b (�i��A����ĳ)
            var memberExists = await _context.Members.AnyAsync(m => m.MemberId == currentUserId.Value);
            if (!memberExists)
            {
                return NotFound($"�|�� ID {currentUserId.Value} ���s�b�C");
            }

            // 3. �}�l�إ� Order ����
            var order = new Order
            {
                MemberId = currentUserId.Value,

                //OrdererName = orderCreateDto.OrdererInfo.Name, // �q�ʤH�m�W
                //OrdererPhone = orderCreateDto.OrdererInfo.MobilePhone, // �q�ʤH�q��
                //OrdererEmail = orderCreateDto.OrdererInfo.Email, // �q�ʤHEmail

                TotalAmount = orderCreateDto.TotalAmount,
                PaymentMethod = orderCreateDto.SelectedPaymentMethod, // �ϥΪ̿�ܪ��I�ڤ覡
                Status = OrderStatus.Pending, // ��l���A�]�� Pending (�� AwaitingPayment)
                CreatedAt = DateTime.UtcNow, // �ϥ� UTC �ɶ�
                Note = orderCreateDto.OrderNotes,

                // �B�z�q�ʤH��T (�Y�ϱq�|����Ʊa�J�A�]�O���q���U���ַ�)
                // �o�̰��] Order Model �����������x�s�q�ʤH���m�W/�q��/Email�A
                // �o�Ǹ�T�D�n��{�b OrderParticipant (�p�G�q�ʤH�]�O�ȫ�)
                // �Ϊ̱z�i�H�Ҽ{�b Order Model �s�W OrdererName, OrdererPhone, OrdererEmail ���
                // �H�U�O���] Order Model ���o����쪺���p�G
                // OrdererName = orderCreateDto.OrdererInfo.Name,
                // OrdererPhone = orderCreateDto.OrdererInfo.MobilePhone,
                // OrdererEmail = orderCreateDto.OrdererInfo.Email,

                // �B�z�o���ШD��T
                InvoiceOption = orderCreateDto.InvoiceRequestInfo.InvoiceOption,
                InvoiceDeliveryEmail = orderCreateDto.InvoiceRequestInfo.InvoiceDeliveryEmail,
                InvoiceUniformNumber = orderCreateDto.InvoiceRequestInfo.InvoiceUniformNumber,
                InvoiceTitle = orderCreateDto.InvoiceRequestInfo.InvoiceTitle,
                InvoiceAddBillingAddr = orderCreateDto.InvoiceRequestInfo.InvoiceAddBillingAddr,
                InvoiceBillingAddress = orderCreateDto.InvoiceRequestInfo.InvoiceBillingAddress
                // �p�G�z�� "���صo��" �b Order Model �s�W�F IsInvoiceDonated ���:
                // IsInvoiceDonated = orderCreateDto.InvoiceRequestInfo.DonateInvoice,
            };

            // 4. �B�z�ȫȦC�� (OrderParticipants)
            foreach (var participantDto in orderCreateDto.Participants)
            {
                var participant = new OrderParticipant();

                if (participantDto.FavoriteTravelerId.HasValue && participantDto.FavoriteTravelerId.Value > 0)
                {
                    // ���ձq�`�ήȫ�Ū�����
                    var favoriteTraveler = await _context.MemberFavoriteTravelers
                        .FirstOrDefaultAsync(ft => ft.FavoriteTravelerId == participantDto.FavoriteTravelerId.Value &&
                                                   ft.MemberId == currentUserId.Value && // �T�O�O�ӷ|�����`�ήȫ�
                                                   ft.Status == FavoriteStatus.Active); // �T�O�`�ήȫȬO���Ī�

                    if (favoriteTraveler != null)
                    {
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

                        // �O���O�q���ӱ`�ήȫȨӪ� (�i��)
                        // participant.SourceFavoriteTravelerId = favoriteTraveler.FavoriteTravelerId;
                    }
                    else
                    {
                        // �䤣�즳�Ī��`�ήȫȡA�O��ĵ�i�A�ç����̿� DTO �����
                        _logger.LogWarning($"�䤣�� MemberId: {currentUserId.Value} ���`�ήȫ� FavoriteTravelerId: {participantDto.FavoriteTravelerId.Value}�A�θӮȫȪ��A�D Active�C�N�����ϥ� DTO ���Ѫ��ȫȸ�ơC");
                    }
                }

                // �L�׬O�_�q�`�ήȫȸ��J�A�����\ DTO ��������л\�δ��� (�p�G DTO ��즳��)
                // �o�̪��޿�O�G�p�G DTO ���ѤF�ȡA�N�� DTO ���F�_�h�A�p�G�q�`�ήȫȸ��J�F�ȡA�N�O�d�C
                // �t�@�ص����O�G�`�ήȫ��u���ADTO�ȥΩ�s�W�ΨS���`�ήȫ�ID�����p�C
                // �H�U�ĥ� DTO �u�������� (�p�G DTO ���ȴN�л\�`�ήȫȪ���)
                // ����� Name, Phone, Email ���������A�p�G�`�ήȫȨS���ADTO �����ѡC

                // �T�O Name, Phone, Email ���֤߸�T�Ӧ� DTO (�]�� DTO �o�����O Required)
                participant.Name = participantDto.Name;
                participant.Phone = participantDto.Phone;
                participant.Email = participantDto.Email;

                // ���i�����A�p�G DTO �����ѴN�ϥΡA�_�h�O�d�q�`�ήȫȸ��J���� (�p�G������)
                // DateTime �M Enum �� Nullable �P�_��� tricky�A�]�� DTO ���q�`�����O DateTime/Enum �Ӥ��O DateTime?/Enum?
                // �� OrderParticipantDto ���� BirthDate, Gender, DocumentType �O����

                participant.BirthDate = participantDto.BirthDate;
                participant.IdNumber = participantDto.IdNumber; // DTO�� IdNumber �]�O����
                participant.Gender = participantDto.Gender;     // DTO�� Gender �]�O����
                participant.DocumentType = participantDto.DocumentType; // DTO�� DocumentType �]�O����

                // �i�諸�A�p�GDTO���ȴN�л\
                if (!string.IsNullOrEmpty(participantDto.DocumentNumber))
                    participant.DocumentNumber = participantDto.DocumentNumber;
                if (!string.IsNullOrEmpty(participantDto.PassportSurname))
                    participant.PassportSurname = participantDto.PassportSurname;
                if (!string.IsNullOrEmpty(participantDto.PassportGivenName))
                    participant.PassportGivenName = participantDto.PassportGivenName;
                if (participantDto.PassportExpireDate.HasValue)
                    participant.PassportExpireDate = participantDto.PassportExpireDate.Value;
                if (!string.IsNullOrEmpty(participantDto.Nationality))
                    participant.Nationality = participantDto.Nationality;
                if (!string.IsNullOrEmpty(participantDto.Note)) // �i�H�Ҽ{�X�ֱ`�ήȫȪ��Ƶ��MDTO���Ƶ�
                    participant.Note = participantDto.Note;


                // �p�G participantDto.MemberIdAsParticipant ���ȡA��ܳo��ȫȦP�ɤ]�O�t�η|��
                // �z�i�H�ھڦ� ID ���B�~�ˬd�ΰO���A�� OrderParticipant �����S�������� MemberId FK
                // participant.AssociatedMemberId = participantDto.MemberIdAsParticipant; (�p�G OrderParticipant �������)

                order.OrderParticipants.Add(participant);
            }
            // 5. �x�s���Ʈw
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "�إ߭q��ɸ�Ʈw��s���ѡC");
                // �i�H�ˬd ex.InnerException �������ԲӪ����~
                return StatusCode(StatusCodes.Status500InternalServerError, "�إ߭q�楢�ѡA�еy��A�աC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�إ߭q��ɵo�ͥ��w�����~�C");
                return StatusCode(StatusCodes.Status500InternalServerError, "�إ߭q��ɵo�Ϳ��~�C");
            }

            // 6. �ǳƦ^�����e�ݪ����
            // �^�ǭq��ID�A�H�Τ�I�һݪ�������T (�o�������M��z�P��I�h�D����X�覡)
            var orderSummary = new OrderSummaryDto
            {
                OrderId = order.OrderId,
                OrderStatus = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                SelectedPaymentMethod = order.PaymentMethod?.ToString() ?? "N/A",
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
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id) //  OrderDto �Ω���ܭq��Ա�
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var order = await _context.Orders
                                .Include(o => o.OrderParticipants)
                                .Include(o => o.OrderDetails) // �I�ڧ�����~�|�����
                                .FirstOrDefaultAsync(o => o.OrderId == id && o.MemberId == currentUserId.Value);

            if (order == null)
            {
                return NotFound($"�䤣��q�� ID {id}�A�αz�L�v�s�����q��C");
            }

            // �o�̻ݭn�N Order ����M�g��@�� OrderDto (�]�t�q��Ҧ��ԲӸ�T�� DTO)
            // �o�� OrderDto �|�� OrderSummaryDto ��Բ�
            // �H�U��²�ƽd�ҡA�����^�� order (������M�g�� DTO)
            // var orderDto = MapOrderToOrderDto(order); // �z�ݭn��@�o�ӬM�g��k
            // return Ok(orderDto);

            // �Ȯɦ^�� order ����A�z���إߨèϥΤ@�ӸԲӪ� OrderDto
            return Ok(new{
                order.OrderId,
                order.MemberId,
                //OrdererName = order.OrdererName, // �^�ǧַӪ��q�ʤH�m�W
                //OrdererPhone = order.OrdererPhone, // �^�ǧַӪ��q�ʤH�q��
                //OrdererEmail = order.OrdererEmail, // �^�ǧַӪ��q�ʤHEmail
                order.TotalAmount,
                PaymentMethod = order.PaymentMethod?.ToString(),
                Status = order.Status.ToString(),
                order.CreatedAt,
                order.PaymentDate,
                order.Note,
                order.InvoiceOption,
                order.InvoiceDeliveryEmail,
                order.InvoiceUniformNumber,
                order.InvoiceTitle,
                order.InvoiceAddBillingAddr,
                order.InvoiceBillingAddress,
                Participants = order.OrderParticipants.Select(p => new {
                    p.Name,
                    p.Phone,
                    p.Email, // �^�������ȫȸ�T
                    p.BirthDate,
                    p.IdNumber,
                    p.Gender,
                    p.DocumentType
                }).ToList(),
                OrderDetails = order.OrderDetails.Select(od => new {
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