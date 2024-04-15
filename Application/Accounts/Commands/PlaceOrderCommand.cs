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
    public sealed class PlaceOrderCommand : IRequest<Result>
    {
        public int OrgMealId { get; set; }
        public List<OrderItemContract> OrderItems { get; set; }
        public string? SpecialRequest { get; set; }
        public string TimeSlot { get; set; }
    }

    public sealed class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Result>
    {
        private readonly IidentityService  _identityService;
        public PlaceOrderHandler(IidentityService identityService)
        {
            _identityService = identityService;
        }
        public Task<Result> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            return _identityService.PlaaceOrderAsync(request, cancellationToken);
        }
    }
}
