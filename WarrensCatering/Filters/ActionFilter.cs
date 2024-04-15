using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WarrensCatering.Filters
{
    public class ActionFilter : IActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        public ActionFilter(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //var userId = _currentUser.UserId;
            //if (userId == null)
            //{
            //    throw new NotFoundException("User not found");
            //}
            //var isBlocked = _userManager.Users.Where(x => x.IsUserBlocked == true && x.Id == userId).FirstOrDefault();
            //if (isBlocked != null)
            //{
            //    context.Result = new ContentResult
            //    {
            //        Content = "Your account is blocked. Please contact admin.",
            //        StatusCode = 433, // 433 Blocked User
            //    };
            //}

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = _currentUser.UserId;
            if (userId == null)
            {
                throw new NotFoundException("User not found");
            }
            var isBlocked = _userManager.Users.Where(x => x.IsUserBlocked == true && x.Id == userId).FirstOrDefault();
            if (isBlocked != null)
            {
                context.Result = new ContentResult
                {
                    Content = "Your account is blocked. Please contact admin.",
                    StatusCode = 433, // 433 Blocked User
                };
            }
        }
    }
}
