using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands
{
    public sealed record SendEmailOtpCommand(string existingEmail, string newEmail) : IRequest<Result>;

    internal sealed class SendEmailOtpCommandHandler : IRequestHandler<SendEmailOtpCommand, Result>
    {
        private readonly IidentityService _identityService;

        public SendEmailOtpCommandHandler(IidentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Result> Handle(SendEmailOtpCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.SendEmailOTPAsync(request, cancellationToken);
        }
    }
}
