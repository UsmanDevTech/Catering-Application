using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Contracts;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands
{
    public sealed class AddToCartCommand : IRequest<Result>
    {
        public int OrgMealId { get; set; }
        public List<CartItemContract> CartItems { get; set; }
    }

    public sealed class AddToCartHandler : IRequestHandler<AddToCartCommand, Result>
    {

        private readonly IidentityService _identityService;
        public AddToCartHandler(IidentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Result> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.AddToCartAsync(request, cancellationToken);
        }
    }

}
