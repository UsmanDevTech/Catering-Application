using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Commands;

public sealed record MarkAsSeenCommand(int notificationId):IRequest<Result>;

public sealed class MarkAsSeenCommandHandler : IRequestHandler<MarkAsSeenCommand, Result>
{
    private readonly IApplicationDbContext _applicationDbcontext;
    private readonly ICurrentUserService _currentUser;
    private readonly IidentityService _identityService;

    public MarkAsSeenCommandHandler(IApplicationDbContext applicationDbcontext, ICurrentUserService currentUser, IidentityService identityService)
    {
        _applicationDbcontext = applicationDbcontext;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result> Handle(MarkAsSeenCommand request, CancellationToken cancellationToken)
    {
        await _identityService.CheckUserExistAsync(_currentUser.UserId, cancellationToken);

        var notification = await _applicationDbcontext.Notifications.Where(e => e.Id == request.notificationId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Notification does not exist.");

        notification.IsSeen = true;

        await _applicationDbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}