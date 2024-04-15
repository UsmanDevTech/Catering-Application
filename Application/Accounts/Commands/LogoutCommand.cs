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
    public sealed record LogoutCommand() : IRequest<Result>;
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IidentityService _identity;

        public LogoutCommandHandler(IidentityService identity)
        {
            _identity = identity;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            return await _identity.LogoutAsync();
        }
    }
}
