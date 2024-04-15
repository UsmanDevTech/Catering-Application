using Application.Common.Interfaces;
using Domain.Contracts;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Queries
{
    public sealed record GetCartQuery():IRequest<GetCartContract>;
    public sealed record GetCartHandler:IRequestHandler<GetCartQuery, GetCartContract>
    {
        private readonly IidentityService _identityService;
        public GetCartHandler(IidentityService identityService)
        {
              _identityService=identityService;
        }

        public async Task<GetCartContract> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            return await _identityService.GetCartAsync(request, cancellationToken);
        }
    }
     
}
