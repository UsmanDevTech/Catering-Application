using Application.Common.Interfaces;
using Domain.Contracts;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Queries
{
    public sealed record GetTermsQuery(ContentType type) : IRequest<GenericAppDocumentContract>;
    internal sealed class GetTermsHandler : IRequestHandler<GetTermsQuery, GenericAppDocumentContract>
    {
        private readonly IApplicationDbContext _context;
        public GetTermsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GenericAppDocumentContract> Handle(GetTermsQuery request, CancellationToken cancellationToken)
        {
            return await _context.AppContents.Where(u => u.ContentType == request.type).Select(u => new GenericAppDocumentContract
            {
                id = u.Id,
                name = u.Title,
                value = u.Content,
               // iconUrl = u.Icon,
            }).FirstOrDefaultAsync(cancellationToken) ?? new GenericAppDocumentContract();
        }
    }
}
