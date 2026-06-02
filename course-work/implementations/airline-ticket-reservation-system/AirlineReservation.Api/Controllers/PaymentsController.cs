using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[Route("api/payments")]
public class PaymentsController : CrudControllerBase<PaymentFilterRequest, PaymentCreateRequest, PaymentUpdateRequest, PaymentResponse>
{
    public PaymentsController(IPaymentService service) : base(service)
    {
    }
}
