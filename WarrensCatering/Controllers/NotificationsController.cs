using Application.Common.Models;
using Application.Meals.Queries;
using Application.Notifications.Commands;
using Application.Notifications.Queries;
using Application.Notifications.Query;
using Domain.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqftAPI.Controllers;
using WarrensCatering.Filters;

namespace WarrensCatering.Controllers
{
    public class NotificationsController : BaseApiController
    {
        [ServiceFilter(typeof(ActionFilter))]
        [HttpGet(Routes.Notifications.get_all_notifications)]
        public async Task<PagedResponse<NotificationsContract>> GetAllNotifications([FromQuery] PaginationParams param, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new GetAllNotificationsPaginatedQuery(param), cancellationToken);
        }
        [ServiceFilter(typeof(ActionFilter))]
        [HttpPut(Routes.Notifications.mark_as_seen)]
        public async Task<Result> MarkSeen([FromQuery] MarkAsSeenCommand command, CancellationToken cancellationToken)
        {
            return await Mediator.Send(command, cancellationToken);
        }
        [ServiceFilter(typeof(ActionFilter))]
        [HttpGet(Routes.Notifications.get_order_status)]
        public async Task<GetOrderStatusContract> GetOrderStatus( CancellationToken cancellationToken)
        {
            return await Mediator.Send(new GetOrderStatusQuery(), cancellationToken);
        }

    }
}
