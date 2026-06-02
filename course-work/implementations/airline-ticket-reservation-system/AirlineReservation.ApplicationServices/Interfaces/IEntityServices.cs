using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;

namespace AirlineReservation.ApplicationServices.Interfaces;

public interface IUserService : ICrudService<UserFilterRequest, UserCreateRequest, UserUpdateRequest, UserResponse>;

public interface IAirportService : ICrudService<AirportFilterRequest, AirportCreateRequest, AirportUpdateRequest, AirportResponse>;

public interface IAircraftService : ICrudService<AircraftFilterRequest, AircraftCreateRequest, AircraftUpdateRequest, AircraftResponse>;

public interface IFlightService : ICrudService<FlightFilterRequest, FlightCreateRequest, FlightUpdateRequest, FlightResponse>;

public interface IReservationService : ICrudService<ReservationFilterRequest, ReservationCreateRequest, ReservationUpdateRequest, ReservationResponse>;

public interface ITicketService : ICrudService<TicketFilterRequest, TicketCreateRequest, TicketUpdateRequest, TicketResponse>;

public interface IPaymentService : ICrudService<PaymentFilterRequest, PaymentCreateRequest, PaymentUpdateRequest, PaymentResponse>;
