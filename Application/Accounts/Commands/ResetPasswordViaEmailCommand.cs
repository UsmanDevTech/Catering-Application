using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands
{
    public sealed record ResetPasswordViaEmailCommand(string email, int? resetOption, string resetValue, string password) : IRequest<Result>;
    internal sealed class ResetPasswordViaEmailCommandHandler : IRequestHandler<ResetPasswordViaEmailCommand, Result>
    {
        private readonly IidentityService _identityService;

        public ResetPasswordViaEmailCommandHandler(IidentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Result> Handle(ResetPasswordViaEmailCommand request, CancellationToken cancellationToken)
        {
            var response = await _identityService.ResetPasswordViaEmailAsync(request);
            return response.Result;
        }
    }
}
