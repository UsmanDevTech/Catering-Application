using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Contracts;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Query
{
    public sealed class GetOrderStatusQuery:IRequest<GetOrderStatusContract>
    {
    }

    public sealed class GetOrderStatusHandler : IRequestHandler<GetOrderStatusQuery, GetOrderStatusContract>
    {
        private readonly IApplicationDbContext _applicationDbcontext;
        private readonly ICurrentUserService _currentUser;
        private readonly IidentityService _identityService;

        public GetOrderStatusHandler(IApplicationDbContext applicationDbcontext, ICurrentUserService currentUser, IidentityService identityService)
        {
            _applicationDbcontext = applicationDbcontext;
            _currentUser = currentUser;
            _identityService = identityService;
        }

        public async Task<GetOrderStatusContract> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
        {
            await _identityService.CheckUserExistAsync(_currentUser.UserId, cancellationToken);

            //gets time zone
            string timeZoneId = "Asia/Karachi";

            var orgmealId = _applicationDbcontext.Orders.Where(x => x.UserId == _currentUser.UserId).OrderByDescending(x => x.CreatedAt).Select(x => x.OrgMealId).FirstOrDefault();
            var starttime = _applicationDbcontext.OrgMeals.Where(x=>x.Id == orgmealId).Select(x=>x.StartTime).FirstOrDefault();
            var endtime = _applicationDbcontext.OrgMeals.Where(x => x.Id == orgmealId).Select(x => x.EndTime).FirstOrDefault();
            
            if((starttime.TimeOfDay <= DateTime.Now.TimeOfDay && endtime.TimeOfDay >= DateTime.Now.TimeOfDay))
            {
                // getting list of orders of specific loggedIn user
                var latestOrder = await _applicationDbcontext.Orders
                    .Where(x => x.UserId == _currentUser.UserId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(e => new GetOrderStatusContract
                    {
                        OrderID = e.OrderID,
                        Status = e.Status
                    })
                    .FirstOrDefaultAsync()
                    ?? new GetOrderStatusContract();

                return latestOrder;
            }
            else
            {
                // getting list of orders of specific loggedIn user
                var latestOrder = await _applicationDbcontext.Orders
                    .Where(x => x.UserId == _currentUser.UserId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(e => new GetOrderStatusContract
                    {
                        OrderID = e.OrderID,
                        Status = (int) StatusEnum.Pending
                    })
                    .FirstOrDefaultAsync()
                    ?? new GetOrderStatusContract();

                return latestOrder;
            }
            //_applicationDbcontext.OrgMeals.Where(e => e.Id == x.OrgMealId).Select(o => o.StartTime).FirstOrDefault().TimeOfDay <= DateTime.Now.TimeOfDay && _applicationDbcontext.OrgMeals.Where(e => e.Id == x.OrgMealId).Select(o => o.EndTime).FirstOrDefault().TimeOfDay >= DateTime.Now.TimeOfDay
            //DateTime latestorderdate = await _applicationDbcontext.Orders
            //    .Where(x => x.UserId == _currentUser.UserId)
            //    .OrderByDescending(x => x.CreatedAt)
            //    .Select(e => e.CompletedAt.Value.UtcToLocal(timeZoneId))
            //    .FirstOrDefaultAsync();
            //var latesthour =  latestorderdate.Hour;
            //var latestmin = latestorderdate.Minute;

            //string timeslot = await _applicationDbcontext.Orders
            //    .Where(x => x.UserId == _currentUser.UserId)
            //    .OrderByDescending(x => x.CreatedAt)
            //    .Select(e => e.TimeSlot)
            //    .FirstOrDefaultAsync();
        }
    }
}
