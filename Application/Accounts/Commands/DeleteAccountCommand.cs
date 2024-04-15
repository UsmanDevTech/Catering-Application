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
    public sealed record DeleteAccountCommand(string password) : IRequest<Result>;
    public sealed class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result>
    {
        private readonly IidentityService _identity;

        public DeleteAccountCommandHandler(IidentityService identity)
        {
            _identity = identity;
        }

        public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            return await _identity.RequestForAccountDeleteAsync(request.password);
        }
    }
}
