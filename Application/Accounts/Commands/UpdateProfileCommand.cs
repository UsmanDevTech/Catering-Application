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
    public sealed record UpdateProfileCommand(string companyId, string fullName, string imageUrl, List<string>? allergy) :IRequest<Result>;

    public sealed class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result>
    {
        private readonly IidentityService _identityService;
        public UpdateProfileHandler(IidentityService identityService)
        {
            _identityService = identityService;
        }
        public Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            return _identityService.UpdateProfileDetailAsync(request, cancellationToken);
        }
    }
}
