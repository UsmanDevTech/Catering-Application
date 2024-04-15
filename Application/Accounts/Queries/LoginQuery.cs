using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Contracts;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Contracts.ResponseKey;

namespace Application.Accounts.Queries
{
    public sealed record LoginQuery(string email, string password, string? fcmToken, string? timeZoneId) : IRequest<ResponseKeyContract>;
    public sealed class LoginQueryHandler : IRequestHandler<LoginQuery, ResponseKeyContract>
    {
        private readonly IidentityService _users;
        public LoginQueryHandler(IidentityService users)
        {
            _users = users;
        }

        public async Task<ResponseKeyContract> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            return await _users.AuthenticateUserAsync(request);
        }
    }
}
