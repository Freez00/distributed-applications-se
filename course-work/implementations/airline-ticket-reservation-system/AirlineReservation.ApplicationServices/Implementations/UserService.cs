using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Data.Entities;
using AirlineReservation.Data.Enums;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public class UserService : EntityServiceBase<UserEntity, UserFilterRequest, UserCreateRequest, UserUpdateRequest, UserResponse>, IUserService
{
    public UserService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    protected override IRepository<UserEntity> Repository => UnitOfWork.Users;

    protected override IQueryable<UserEntity> ApplyFilters(IQueryable<UserEntity> query, UserFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.FirstName))
        {
            query = query.Where(x => x.FirstName.Contains(filter.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(filter.LastName))
        {
            query = query.Where(x => x.LastName.Contains(filter.LastName));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(x => x.Email.Contains(filter.Email));
        }

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            var role = ParseEnum<UserRole>(filter.Role);
            query = query.Where(x => x.Role == role);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        return query;
    }

    protected override IQueryable<UserEntity> ApplySorting(IQueryable<UserEntity> query, UserFilterRequest filter)
        => (filter.SortBy.ToLowerInvariant(), filter.SortDirection) switch
        {
            ("firstname", "desc") => query.OrderByDescending(x => x.FirstName),
            ("firstname", _) => query.OrderBy(x => x.FirstName),
            ("lastname", "desc") => query.OrderByDescending(x => x.LastName),
            ("lastname", _) => query.OrderBy(x => x.LastName),
            ("email", "desc") => query.OrderByDescending(x => x.Email),
            ("email", _) => query.OrderBy(x => x.Email),
            ("role", "desc") => query.OrderByDescending(x => x.Role),
            ("role", _) => query.OrderBy(x => x.Role),
            _ when filter.SortDirection == "desc" => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

    protected override async Task BeforeCreateAsync(UserEntity entity, UserCreateRequest request, CancellationToken cancellationToken)
    {
        if (await UnitOfWork.Users.Query().AnyAsync(x => x.Email == entity.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }
    }

    protected override async Task BeforeUpdateAsync(UserEntity entity, UserUpdateRequest request, CancellationToken cancellationToken)
    {
        if (await UnitOfWork.Users.Query().AnyAsync(x => x.Id != entity.Id && x.Email == entity.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }
    }

    public override async Task<bool> ForceDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await UnitOfWork.Users.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var reservations = await UnitOfWork.Reservations.Query()
            .Where(x => x.UserId == id)
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            UnitOfWork.Reservations.Remove(reservation);
        }

        UnitOfWork.Users.Remove(user);
        await SaveDeleteChangesAsync(cancellationToken);
        return true;
    }

    protected override UserEntity MapCreateRequest(UserCreateRequest request) => new()
    {
        FirstName = request.FirstName.Trim(),
        LastName = request.LastName.Trim(),
        Email = request.Email.Trim().ToLowerInvariant(),
        PasswordHash = PasswordHashingService.Hash(request.Password),
        Role = ParseEnum<UserRole>(request.Role),
        PhoneNumber = request.PhoneNumber,
        IsActive = request.IsActive
    };

    protected override void ApplyUpdateRequest(UserEntity entity, UserUpdateRequest request)
    {
        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Email = request.Email.Trim().ToLowerInvariant();
        entity.Role = ParseEnum<UserRole>(request.Role);
        entity.PhoneNumber = request.PhoneNumber;
        entity.IsActive = request.IsActive;
    }

    protected override UserResponse MapToResponse(UserEntity entity) => new()
    {
        Id = entity.Id,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email,
        Role = entity.Role.ToString(),
        PhoneNumber = entity.PhoneNumber,
        CreatedAt = entity.CreatedAt,
        IsActive = entity.IsActive
    };
}
