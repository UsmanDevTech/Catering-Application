using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Contracts;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Meals.Queries;

public sealed record GetTodayOrganizationMealQuery() : IRequest<List<GetOrgMealContract>>;

internal sealed class GetTodayOrganizationMealQueryHandler : IRequestHandler<GetTodayOrganizationMealQuery, List<GetOrgMealContract>>
{
    private readonly IApplicationDbContext dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IidentityService _identityService;

    public GetTodayOrganizationMealQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IidentityService identityService)
    {
        this.dbContext = dbContext;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<List<GetOrgMealContract>> Handle(GetTodayOrganizationMealQuery request,
        CancellationToken cancellationToken)
    {
        await _identityService.CheckUserExistAsync(_currentUser.UserId, cancellationToken);

        string timeZoneId = await _identityService.GetTimeZoneId(_currentUser.UserId, cancellationToken);

        var currentDayOfWeek = (int)DateTime.UtcNow.UtcToLocal(timeZoneId).DayOfWeek;

        int orgId = await _identityService.GetUserOrganizationId(_currentUser.UserId, cancellationToken);

        //var start = new DateTime(2023, 10, 10, 6, 30, 0);
        //var end = new DateTime(2023, 10, 10, 9, 0, 0);
        //var slots = _identityService.GenerateTimeSlots(start, end, 30);

        var orgMeals = await dbContext.OrgMeals.Where(e => e.OrganizationId == orgId &&
            e.WeekDay == (WeekDays) currentDayOfWeek)
            .OrderBy(e => e.StartTime)
            .Select(e => new GetOrgMealContract
            {
                Id = e.Id,
                StartTime = e.StartTime.ToString("h:mm tt"),
                EndTime = e.EndTime.ToString("h:mm tt"),
                WeekDay = e.WeekDay,
                MealTypeId = e.MealTypeId,
                MealName = e.MealType.Name,
                MealPictureUrl = e.MealType.PictureUrl,
                TimeSlots = _identityService.GenerateTimeSlots(e.StartTime, e.EndTime, 30),
                Categories = e.OrgMealCategories.Select(o => new OrgMealCategoryContract
                {
                    Id = o.Id,
                    Name = o.Name,
                    ImageUrl = o.ImageUrl,
                    Items = o.OrgMealCatAndOrgMealitems.Select(oi => oi.Items.Name).ToList()
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return orgMeals;
    }

}

