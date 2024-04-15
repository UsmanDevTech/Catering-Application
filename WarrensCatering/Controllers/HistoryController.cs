using Application.Common.Models;
using Application.History.Command;
using Application.History.Query;
using Domain.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqftAPI.Controllers;
using WarrensCatering.Filters;

namespace WarrensCatering.Controllers
{

    public class HistoryController : BaseApiController
    {
        [ServiceFilter(typeof(ActionFilter))]
        [HttpGet(Routes.History.get_order_history)]
        public async Task<PagedResponse<OrderHistoryContract>> GetOrderHistoryAsync([FromQuery] PaginationParams param, CancellationToken cancelationToken)
        {
            return await Mediator.Send(new GetOrderHistoryPaginatedQuery(param), cancelationToken);
        }

        [ServiceFilter(typeof(ActionFilter))]
        [HttpPost(Routes.History.give_rating)]
        public async Task<Result> GiveRating([FromBody] OrderRatingCommand request, CancellationToken cancelationToken)
        {
            return await Mediator.Send(request, cancelationToken);
        }

    }
}
