using Application.Common.Interfaces;
using Domain.Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Queries
{
    public sealed record GetAccountProfileQuery(string? userId):IRequest<UserProfileContract>;

    internal sealed class GetAccountProfileHandler : IRequestHandler<GetAccountProfileQuery, UserProfileContract>
    {
        private readonly IidentityService _identityService;
        public GetAccountProfileHandler(IidentityService identityService)
        {
            _identityService = identityService;
        }
        public async Task<UserProfileContract> Handle(GetAccountProfileQuery request, CancellationToken cancellationToken)
        {
            return await _identityService.GetUserProfileAsync(request);
        }
    }
}
