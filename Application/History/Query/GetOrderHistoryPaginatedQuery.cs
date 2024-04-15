using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.History.Query;

public sealed record GetOrderHistoryPaginatedQuery(PaginationParams Pagination) :
    IRequest<PagedResponse<OrderHistoryContract>>;

public class GetOrderHistoryHandler : IRequestHandler<GetOrderHistoryPaginatedQuery, PagedResponse<OrderHistoryContract>>
{
    private readonly IApplicationDbContext _applicationDbcontext;
    private readonly ICurrentUserService _currentUser;
    private readonly IidentityService _identityService;

    public GetOrderHistoryHandler(IApplicationDbContext applicationDbcontext, ICurrentUserService currentUser, IidentityService identityService)
    {
        _applicationDbcontext = applicationDbcontext;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<PagedResponse<OrderHistoryContract>> Handle(GetOrderHistoryPaginatedQuery request, CancellationToken cancellationToken)
    {
        //checking if user exist
        await _identityService.CheckUserExistAsync(_currentUser.UserId, cancellationToken);

        //getting list of orders of specific logged in user
        var query = _applicationDbcontext.Orders.Where(e => e.UserId == _currentUser.UserId)
            .OrderByDescending(e => e.CreatedAt);

        int totalItems = await query.CountAsync(cancellationToken);

        var returnItems = await query
            .Select(e => new OrderHistoryContract
            {
                Id = e.Id,
                OrderID = e.OrderID,
                MealName = e.MealName,
                SpecialRequest = e.SpecialRequest,
                TimeSlot = e.TimeSlot,
                OrderStatus = e.Status,
                Rating = e.Rating.Value,
                OrderItems = e.OrderItems.Select(o => new OrderItemContract
                {
                    Category = o.Category,
                    CategoryImage = o.CategoryImage,
                    ItemName = o.Name
                }).ToList()
            })
            .Skip(request.Pagination.PageSize * (request.Pagination.PageNumber - 1))
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        //creating Paginated Response
        var pagedResponse = new PagedResponse<OrderHistoryContract>(returnItems, totalItems,
            request.Pagination.PageNumber, request.Pagination.PageSize);

        return pagedResponse;
    }

    //public async Task<List<OrderHistoryContract>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    //{
    //    //checking if user exist
    //    await _identityService.CheckUserExistAsync(_currentUser.UserId, cancellationToken);

    //    //getting list of orders of specific logged in user
    //    var orders = await _applicationDbcontext.Orders
    //        .Where(x => x.UserId == _currentUser.UserId)
    //        .ToListAsync();

    //    if (orders == null || !orders.Any())
    //    {
    //        throw new NotFoundException($"This user has no order placed yet");
    //    }
    //    //getting list of orderItems of specific orders 
    //    var orderItems = await _applicationDbcontext.OrderItems
    //        .Where(x => orders.Select(o => o.Id).Contains(x.OrderId))
    //        .ToListAsync();

    //    if (orderItems == null)
    //    {
    //        throw new NotFoundException("Order Items not found");
    //    }
    //    //filtering ratingValue of different orders
    //    var orderRatingValues = await _applicationDbcontext.OrderRating
    //        .Where(x => orders.Select(o => o.Id).Contains(x.OrderId))
    //        .ToDictionaryAsync(x => x.OrderId, x => x.Value);

    //    //getting complete history of orders
    //    var orderHistoryContracts = orders.Select(o => new OrderHistoryContract
    //    {
    //        OrderId = o.Id,
    //        SpecialRequest = o.SpecialRequest,
    //        TimeSlot = o.TimeSlot,
    //        OrderStatus = o.Status,
    //        Rating = orderRatingValues.TryGetValue(o.Id, out var rating) ? rating : 0, // Use 0 or some default value if not found
    //        OrderItems = orderItems
    //            .Where(oi => oi.OrderId == o.Id)
    //            .Select(oi => new OrderItemContract
    //            {
    //                ItemName = oi.Name,
    //                Category = oi.Category,
    //            })
    //            .ToList(),

    //        // Assign other properties as needed
    //    }).ToList();

    //    return orderHistoryContracts;
    //}


}
