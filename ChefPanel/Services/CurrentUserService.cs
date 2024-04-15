using Application.Common.Interfaces;
using Domain.Enums;
using System.Security.Claims;

namespace ChefPanel.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;


    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);


    public string? UserEmail =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    //will be null when request comes from other than API, as not using JWT
    public string? LoginRole =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(nameof(LoginRole));

    //will be null when request comes from other than API, as not using JWT
    public string? TimeZoneId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(nameof(TimeZoneId));
}
