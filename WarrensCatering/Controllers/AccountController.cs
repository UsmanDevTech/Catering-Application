using Application.Accounts.Commands;
using Application.Accounts.Queries;
using Application.Common.Models;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using SqftAPI.Controllers;
using static Domain.Contracts.ResponseKey;

namespace WarrensCatering.Controllers
{ 
    
    public class AccountController : BaseApiController
    {

        
        [AllowAnonymous]
        [HttpPost(Routes.Account.login)]
        public async Task<ResponseKeyContract> LoginAsync([FromBody] LoginQuery loginCommand, CancellationToken token)
        {
            return await Mediator.Send(loginCommand, token);
        }

        [AllowAnonymous]
        [HttpPost(Routes.Account.register_account)]
        public async Task<Result> RegisterAccountAsync([FromBody] RegisterUserCommand createAccount, CancellationToken token)
        {

            //_identityService.AddTestingUserData(res,token);
            //return new Result(true, Array.Empty<string>());
            return await Mediator.Send(createAccount, token);
        }
        [HttpPost(Routes.Account.delete_account)]
        public async Task<Result> DeleteAccountAsync([FromBody] DeleteAccountCommand command, CancellationToken token)
        {
            return await Mediator.Send(command, token);
        }

        [AllowAnonymous]
        [HttpPost(Routes.Account.send_otp)]
        public async Task<Result> SendOtpAsync([FromBody] SendEmailOtpCommand sendOtp, CancellationToken token)
        {
            return await Mediator.Send(sendOtp, token);
        }

        [HttpPut(Routes.Account.logout)]
        public async Task<Result> LogoutProfileAsync(CancellationToken token)
        {
            return await Mediator.Send(new LogoutCommand(), token);
        }

        [AllowAnonymous]
        [HttpPut(Routes.Account.verify_account)]
        public async Task<ResponseKeyContract> VerifyOtpAsync([FromBody] ConfirmEmailCommand verifyAccount, CancellationToken token)
        {
            return await Mediator.Send(verifyAccount, token);
        }

        [AllowAnonymous]
        [HttpPut(Routes.Account.reset_password)]
        public async Task<Result> ResetPasswordAsync([FromBody] ResetPasswordViaEmailCommand resetPassword, CancellationToken token)
        {
            return await Mediator.Send(resetPassword, token);
        }
    }
}
