using Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Contracts.ResponseKey;

namespace Application.Accounts.Commands
{
    public sealed record ConfirmEmailCommand(string email, string otp) : IRequest<ResponseKeyContract>;
    internal sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ResponseKeyContract>
    {
        private readonly IidentityService _identityService;
        public ConfirmEmailCommandHandler(IidentityService identityService, ICurrentUserService currentUser)
        {
            _identityService = identityService;
        }

        public async Task<ResponseKeyContract> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.ConfirmEmailAsync(request, cancellationToken);
        }
    }
}
