using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.History.Command;

public sealed record OrderRatingCommand(int orderId, int RatingValue) : IRequest<Result>;

public sealed class OrderRatingHandler : IRequestHandler<OrderRatingCommand, Result>
{
    private readonly IApplicationDbContext _applicationDbcontext;
    private readonly ICurrentUserService _currentUser;
    private readonly IidentityService _identityService;

    public OrderRatingHandler(IApplicationDbContext applicationDbcontext, ICurrentUserService currentUser, IidentityService identityService)
    {
        _applicationDbcontext = applicationDbcontext;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result> Handle(OrderRatingCommand request, CancellationToken cancellationToken)
    {
        //checking if user exist
        await _identityService.CheckUserExistAsync(_currentUser.UserId, cancellationToken);

        //checks if order exists
        var order = _applicationDbcontext.Orders.Where(x => x.Id == request.orderId )
            .FirstOrDefault();
        if (order == null)
        {
            throw new NotFoundException($"Order with this id: {request.orderId} does not exist in database");
        }

        var orderRating = new OrderRating
        {
            Value = request.RatingValue,
            PostedDate = DateTime.UtcNow,
            OrderId = order.Id,
        };

        _applicationDbcontext.OrderRating.Add(orderRating);

        await _applicationDbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
