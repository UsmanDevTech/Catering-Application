using Application.Accounts.Commands;
using Application.Accounts.Queries;
using Application.Common.Models;
using Application.Meals.Queries;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqftAPI.Controllers;
using WarrensCatering.Filters;

namespace WarrensCatering.Controllers
{

    public class MealController : BaseApiController
    {
        [ServiceFilter(typeof(ActionFilter))]
        [HttpGet(Routes.Meals.get_today_organization_meals)]
        public async Task<List<GetOrgMealContract>> GetTodayOrgMealsAsync(CancellationToken cancellationToken)
        {
            return await Mediator.Send(new GetTodayOrganizationMealQuery(), cancellationToken);
        }

        [ServiceFilter(typeof(ActionFilter))]
        [HttpPost(Routes.Meals.add_to_cart)]
        public async Task<Result> AddToCartAsync([FromBody] AddToCartCommand addToCart, CancellationToken cancellationToken)
        {
            return await Mediator.Send(addToCart, cancellationToken);
        }

        [ServiceFilter(typeof(ActionFilter))]
        [HttpGet(Routes.Meals.get_cart)]
        public async Task<GetCartContract> GetCartAsync(CancellationToken cancellationToken)
        {
            return await Mediator.Send(new GetCartQuery(), cancellationToken);
        }

        [ServiceFilter(typeof(ActionFilter))]
        [HttpPost(Routes.Meals.place_order)]
        public async Task<Result> PlaceOrderAsync([FromBody] PlaceOrderCommand placeOrder, CancellationToken cancellationToken)
        {
            return await Mediator.Send(placeOrder, cancellationToken);
        }

    }
}
