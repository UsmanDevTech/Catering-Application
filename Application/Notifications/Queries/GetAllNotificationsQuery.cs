using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Queries;

public sealed record GetAllNotificationsPaginatedQuery(PaginationParams Pagination) : IRequest<PagedResponse<NotificationsContract>>;

internal sealed class GetAllNotificationsQueryHandler : IRequestHandler<GetAllNotificationsPaginatedQuery, PagedResponse<NotificationsContract>>
{
    private readonly IApplicationDbContext dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IidentityService _identityService;

    public GetAllNotificationsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IidentityService identityService)
    {
        this.dbContext = dbContext;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<PagedResponse<NotificationsContract>> Handle(GetAllNotificationsPaginatedQuery request,
        CancellationToken cancellationToken)
    {
        await _identityService.CheckUserExistAsync(_currentUser.UserId, cancellationToken);

        string timeZoneId = await _identityService.GetTimeZoneId(_currentUser.UserId, cancellationToken);

        var query = dbContext.Notifications.Where(e => e.NotifiedToId == _currentUser.UserId)
            .OrderByDescending(e => e.CreatedDate);

        int totalItems = await query.CountAsync(cancellationToken);

        var returnItems = await query
            .Select(e => new NotificationsContract
            {
                Id = e.Id,
                Description = e.Description,
                IsSeen = e.IsSeen,
                NotifType = e.NotifType
            })
            .Skip(request.Pagination.PageSize * (request.Pagination.PageNumber - 1))
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        //creating Paginated Response
        var pagedResponse = new PagedResponse<NotificationsContract>(returnItems, totalItems,
            request.Pagination.PageNumber, request.Pagination.PageSize);

        return pagedResponse;

    }

}