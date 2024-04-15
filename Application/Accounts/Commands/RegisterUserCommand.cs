using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Contracts;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands
{
    public class RegisterUserCommand : IRequest<Result>
    {
        public string OrganizationId { get; set; }
        public string? FullName { get; set; }
        public List<string>? Allergy { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } = null!;
    }
        public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result>
    {
        private readonly IidentityService _identityService;
        public RegisterUserHandler(IidentityService identityService)
        {
            _identityService = identityService;
        }
        public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
         
            var result= await _identityService.Register(request, cancellationToken);
            return result;
            //return new ResponseKey { Status = StatusCodes.Status201Created, Message = "User Created Successfully" };
         }

       
    }
}
