using Application.Accounts.Commands;
using Application.Accounts.Queries;
using Application.Common.Models;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Contracts.ResponseKey;

namespace Application.Common.Interfaces
{
    public interface IidentityService
    {
        public Task<ResponseKeyContract> AuthenticateUserAsync(LoginQuery request);
        public Task<Result>Register(RegisterUserCommand request, CancellationToken cancellationToken);
        public Task<Result> RequestForAccountDeleteAsync(string password);
        public Task<Result> SendEmailOTPAsync(SendEmailOtpCommand request, CancellationToken token);

        // public Task<ResponseKey> SendOtp([FromBody] string email);
        public Task<Result> LogoutAsync();
        public Task<ResponseKeyContract> ConfirmEmailAsync(ConfirmEmailCommand info, CancellationToken cancellationToken);
        public Task<(Result Result, string UserId)> ResetPasswordViaEmailAsync(ResetPasswordViaEmailCommand request);
        public Task<UserProfileContract> GetUserProfileAsync(GetAccountProfileQuery request);
        public Task<Result> UpdateProfileDetailAsync(UpdateProfileCommand request, CancellationToken token);
        public Task<Result> PlaaceOrderAsync(PlaceOrderCommand request, CancellationToken cancellationToken);
        public Task<Result> AddToCartAsync(AddToCartCommand request, CancellationToken cancellationToken);
        public Task<GetCartContract> GetCartAsync(GetCartQuery request, CancellationToken cancellationToken);

        public Task<int> GetUserOrganizationId(string userId, CancellationToken cancellationToken);
        public Task<string> GetTimeZoneId(string userId, CancellationToken cancellationToken);
        public List<string> GenerateTimeSlots(DateTime startDateTime, DateTime endDateTime, int slotDuration);

        public Task CheckUserExistAsync(string userId, CancellationToken cancellationToken);

    }
}   
