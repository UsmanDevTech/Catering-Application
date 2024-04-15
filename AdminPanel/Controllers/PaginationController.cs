using AdminPanel.Models;
using AdminPanel.ViewModels;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Dynamic.Core;
using TimeZoneConverter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AdminPanel.Controllers;

[Authorize]
[Route("pagination")]
public class PaginationController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public PaginationController(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    //Data Pagination Query
    [HttpPost("getTableData/{route}")]
    public async Task<IActionResult> getPaginationDataAsync(int route, CancellationToken cancellationToken)
    {
        try
        {
            var statusRaw = Request.Form["status"].FirstOrDefault();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            var companyIdsString = Request.Form["companyIds"];
            List<int> companies = new List<int>();
            List<string> chefs = new List<string>();

            if (!string.IsNullOrEmpty(companyIdsString))
            {
                companies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(companyIdsString);
            }
            companies = companies.Count() > 0 ? companies : null;

            var chefIds = Request.Form["chefIds"];
            if (!string.IsNullOrEmpty(chefIds))
            {
                chefs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(chefIds);
            }
            chefs = chefs.Count() > 0 ? chefs : null;

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int? status = statusRaw != null ? Convert.ToInt32(statusRaw) : null;

            var queryParameter = new DataTablePaginationFilter(skip, pageSize, status, sortColumn,
                sortColumnDirection, searchValue);
            dynamic query = null!;

            if (route == 1)
                query = GetAllItemsPaginated(queryParameter, cancellationToken);
            else if (route == 2)
                query = GetAllMealsPaginated(queryParameter, cancellationToken);
            else if (route == 3)
                query = GetAllRequestedUsersPaginated(queryParameter, cancellationToken);
            else if (route == 4)
                query = GetAllUsersPaginated(queryParameter, cancellationToken);
            else if (route == 5)
                query = GetAllOrganizationsPaginated(queryParameter, cancellationToken);
            else if (route == 6)
                query = GetAllOrdersPaginated(queryParameter,companies,chefs,cancellationToken);
            else if (route == 7)
                query = GetAllChefsPaginated(queryParameter, cancellationToken);

            var queryExcute = await query;

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = queryExcute.totalRecords,
                recordsTotal = queryExcute.totalRecords,
                data = queryExcute.data
            };

            return Ok(jsonData);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.GetBaseException().Message);
        }
    }

    public async Task<DatatableResponse<List<ItemsGetVM>>> GetAllItemsPaginated(DataTablePaginationFilter
        filter, CancellationToken cancellationToken)
    {
        var validFilter = new DataTablePaginationFilter(filter?.pageNumber, filter?.pageSize,
            filter?.status, filter?.sortColumn, filter?.sortColumnDirection, filter?.searchValue);

        TimeZone localZone = TimeZone.CurrentTimeZone;
        var lzone = localZone.StandardName.ToString();
        // Get the IANA time zone identifier
        string TimeZoneId = TZConvert.WindowsToIana(TimeZoneInfo.Local.Id);
        //gets time zone
        string timeZoneId = await _dbContext.Users.Where(e => e.Id == _currentUser.UserId)
            .Select(e => e.TimezoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? TimeZoneId;

        //get query
        var query = _dbContext.Items.AsQueryable();

        //account status filter
        if (validFilter.status != null && validFilter.status != 0)
        {
            //Active
            if (validFilter.status.Value == 1)
                query = query.Where(u => u.IsDeleted == false);
            //Blocked
            else if (validFilter.status.Value == 2)
                query = query.Where(u => u.IsDeleted == true);
        }

        //search filter
        if (!string.IsNullOrEmpty(validFilter.searchValue))
        {
            string searchString = validFilter.searchValue.Trim();

            query = query.Where(e => e.Name.Contains(searchString));
        }

        //sorting
        //query = query.OrderByDescending(e => e.CreatedDate);
        if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
        {
            query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
        }

        //pagination
        int totalRecords = await query.CountAsync(cancellationToken);

        var pagedData = await query.Select(e => new ItemsGetVM
        {
            id = e.Id,
            name = e.Name,
            createdAt = e.CreatedDate.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt"),
            isDeleted = e.IsDeleted
        })
        .Skip(validFilter.pageNumber ?? 0)
        .Take(validFilter.pageSize ?? 0)
        .ToListAsync(cancellationToken);

        var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
        return pagedReponse;
    }

    public async Task<DatatableResponse<List<MealsGetVM>>> GetAllMealsPaginated(DataTablePaginationFilter
    filter, CancellationToken cancellationToken)
    {
        var validFilter = new DataTablePaginationFilter(filter?.pageNumber, filter?.pageSize,
            filter?.status, filter?.sortColumn, filter?.sortColumnDirection, filter?.searchValue);

        // Get the IANA time zone identifier
        string TimeZoneId = TZConvert.WindowsToIana(TimeZoneInfo.Local.Id);

        //gets time zone
        string timeZoneId = await _dbContext.Users.Where(e => e.Id == _currentUser.UserId)
            .Select(e => e.TimezoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? TimeZoneId;

        //get query
        var query = _dbContext.MealType.Where(x=>x.CreatedByType == UserType.Admin).AsQueryable();

        //account status filter
        if (validFilter.status != null && validFilter.status != 0)
        {
            //Active
            if (validFilter.status.Value == 1)
                query = query.Where(u => u.IsDeleted == false);
            //Blocked
            else if (validFilter.status.Value == 2)
                query = query.Where(u => u.IsDeleted == true);
        }

        //search filter
        if (!string.IsNullOrEmpty(validFilter.searchValue))
        {
            string searchString = validFilter.searchValue.Trim();

            query = query.Where(e => e.Name.Contains(searchString));
        }

        //sorting
        if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
        {
            query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
        }

        //pagination
        int totalRecords = await query.CountAsync(cancellationToken);

        var pagedData = await query.Select(e => new MealsGetVM
        {
            id = e.Id,
            picture = e.PictureUrl,
            name = e.Name,
            timmings = e.StartTime.ToString("hh:mm tt") + " - " + e.EndTime.ToString("hh:mm tt"),
            createdAt = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt"),
            isDeleted = e.IsDeleted
        })
        .Skip(validFilter.pageNumber ?? 0)
        .Take(validFilter.pageSize ?? 0)
        .ToListAsync(cancellationToken);

        var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
        return pagedReponse;
    }

    public async Task<DatatableResponse<List<UserVM>>> GetAllRequestedUsersPaginated(DataTablePaginationFilter filter, CancellationToken cancellationToken)
    {
        var validFilter = new DataTablePaginationFilter(filter?.pageNumber, filter?.pageSize,
            filter?.status, filter?.sortColumn, filter?.sortColumnDirection, filter?.searchValue);

        // Get the IANA time zone identifier
        string TimeZoneId = TZConvert.WindowsToIana(TimeZoneInfo.Local.Id);

        //gets time zone
        string timeZoneId = await _dbContext.Users.Where(e => e.Id == _currentUser.UserId)
            .Select(e => e.TimezoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? TimeZoneId;

        //get query
        var query = _dbContext.Users.Where(x=>x.UserApprovalStatus == Domain.Enums.UserApprovalStatusEnum.Pending && x.LoginRole == Domain.Enums.UserType.User).AsQueryable();

        ////account status filter
        //if (validFilter.status != null && validFilter.status != 0)
        //{
        //    //Active
        //    if (validFilter.status.Value == 1)
        //        query = query.Where(u => u.IsDeleted == false);
        //    //Blocked
        //    else if (validFilter.status.Value == 2)
        //        query = query.Where(u => u.IsDeleted == true);
        //}

        //search filter
        if (!string.IsNullOrEmpty(validFilter.searchValue))
        {
            string searchString = validFilter.searchValue.Trim();

            query = query.Where(e => e.FullName.Contains(searchString));
        }

        //sorting
        if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
        {
            query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
        }

        //pagination
        int totalRecords = await query.CountAsync(cancellationToken);

        var pagedData = await query.Select(e => new UserVM
        {
            id = e.Id,
            pictureUrl = e.ImageUrl,
            fullName = e.FullName,
            email = e.Email,
            phoneNumber = e.PhoneNumber,
            companyId = e.OrganizationId,
            companyName = _dbContext.Organizations.Where(x=>x.CompanyId == e.OrganizationId).Select(x=>x.Name).FirstOrDefault(),
            allergies = _dbContext.Allergies.Where(x=>x.UserId == e.Id).Select(u=> new UserAllergiesVM
            {
                id = u.Id,
                name = u.Name
            }).ToList(),
            rejectionReason = e.RejectionReason,
            isBlocked = e.IsUserBlocked,
            createdAt = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt"),
        })
        .Skip(validFilter.pageNumber ?? 0)
        .Take(validFilter.pageSize ?? 0)
        .ToListAsync(cancellationToken);

        var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
        return pagedReponse;
    }

    public async Task<DatatableResponse<List<UserVM>>> GetAllUsersPaginated(DataTablePaginationFilter filter, CancellationToken cancellationToken)
    {
        var validFilter = new DataTablePaginationFilter(filter?.pageNumber, filter?.pageSize,
            filter?.status, filter?.sortColumn, filter?.sortColumnDirection, filter?.searchValue);

        // Get the IANA time zone identifier
        string TimeZoneId = TZConvert.WindowsToIana(TimeZoneInfo.Local.Id);

        //gets time zone
        string timeZoneId = await _dbContext.Users.Where(e => e.Id == _currentUser.UserId)
            .Select(e => e.TimezoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? TimeZoneId;

        //get query
        var query = _dbContext.Users.Where(u=>u.LoginRole == UserType.User && u.UserApprovalStatus == UserApprovalStatusEnum.Approved).AsQueryable();

        //account status filter
        if (validFilter.status != null && validFilter.status != 0)
        {
            //Active
            if (validFilter.status.Value == 1)
                query = query.Where(u => u.IsUserBlocked == false && u.UserApprovalStatus == UserApprovalStatusEnum.Approved);
            //Blocked
            else if (validFilter.status.Value == 2)
                query = query.Where(u => u.IsUserBlocked == true && u.UserApprovalStatus == UserApprovalStatusEnum.Approved);
            //Rejected
            else if (validFilter.status.Value == 3)
                query = _dbContext.Users.Where(u => u.LoginRole == UserType.User && u.UserApprovalStatus == UserApprovalStatusEnum.Rejected);
        }

        //search filter
        if (!string.IsNullOrEmpty(validFilter.searchValue))
        {
            string searchString = validFilter.searchValue.Trim();

            query = query.Where(e => e.FullName.Contains(searchString));
        }

        //sorting
        if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
        {
            query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
        }

        //pagination
        int totalRecords = await query.CountAsync(cancellationToken);

        var pagedData = await query.Select(e => new UserVM
        {
            id = e.Id,
            pictureUrl = e.ImageUrl,
            fullName = e.FullName,
            email = e.Email,
            phoneNumber = e.PhoneNumber,
            companyId = e.OrganizationId,
            companyName = _dbContext.Organizations.Where(x => x.CompanyId == e.OrganizationId).Select(x => x.Name).FirstOrDefault(),
            allergies = _dbContext.Allergies.Where(x => x.UserId == e.Id).Select(u => new UserAllergiesVM
            {
                id = u.Id,
                name = u.Name
            }).ToList(),
            rejectionReason = e.RejectionReason,
            userStatus = e.UserApprovalStatus,
            isBlocked = e.IsUserBlocked,
            createdAt = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt"),
        })
        .Skip(validFilter.pageNumber ?? 0)
        .Take(validFilter.pageSize ?? 0)
        .ToListAsync(cancellationToken);

        var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
        return pagedReponse;
    }

    public async Task<DatatableResponse<List<OrganizationGetVM>>> GetAllOrganizationsPaginated(DataTablePaginationFilter
        filter, CancellationToken cancellationToken)
    {
        var validFilter = new DataTablePaginationFilter(filter?.pageNumber, filter?.pageSize,
            filter?.status, filter?.sortColumn, filter?.sortColumnDirection, filter?.searchValue);

        ////gets time zone
        //string timeZoneId = await _dbContext.Users.Where(e => e.Id == _currentUser.UserId)
        //    .Select(e => e.TimezoneId)
        //    .FirstOrDefaultAsync(cancellationToken)
        //    ?? "Asia/Karachi";

        //get query
        var query = _dbContext.Organizations.AsQueryable();

        //account status filter
        if (validFilter.status != null && validFilter.status != 0)
        {
            //Active
            if (validFilter.status.Value == 1)
                query = query.Where(u => u.IsDeleted == false);
            //Blocked
            else if (validFilter.status.Value == 2)
                query = query.Where(u => u.IsDeleted == true);
        }

        //search filter
        if (!string.IsNullOrEmpty(validFilter.searchValue))
        {
            string searchString = validFilter.searchValue.Trim();

            query = query.Where(e => e.Name.Contains(searchString) ||
                e.CompanyId == searchString);
        }

        //sorting
        if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
        {
            query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
        }

        //pagination
        int totalRecords = await query.CountAsync(cancellationToken);

        var pagedData = await query.Select(e => new OrganizationGetVM
        {
            Id = e.Id,
            CompanyId = e.CompanyId,
            ImageUrl = e.ImageUrl,
            Name = e.Name,
            ContactName = e.ContactName,
            Email = e.Email,
            Number = e.Number,
            TotalEmployees = e.Employees.ToString(),
            RegisteredEmployees = _dbContext.Users.Where(u => u.OrganizationId == e.CompanyId
                && u.UserApprovalStatus == UserApprovalStatusEnum.Approved)
                .Count().ToString(),
            ChefId = e.ChefId,
            ChefName = (e.ChefId == null)? "Not Assigned" : _dbContext.Users.First(u => u.Id == e.ChefId).FullName,
            IsDeleted = e.IsDeleted
        })
        .Skip(validFilter.pageNumber ?? 0)
        .Take(validFilter.pageSize ?? 0)
        .ToListAsync(cancellationToken);

        var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
        return pagedReponse;
    }

    public async Task<DatatableResponse<List<OrderVM>>> GetAllOrdersPaginated(DataTablePaginationFilter filter,List<int>? companyIds,List<string>? chefIds, CancellationToken cancellationToken)
    {
        var validFilter = new DataTablePaginationFilter(filter?.pageNumber, filter?.pageSize,
            filter?.status, filter?.sortColumn, filter?.sortColumnDirection, filter?.searchValue);

        // Get the IANA time zone identifier
        string TimeZoneId = TZConvert.WindowsToIana(TimeZoneInfo.Local.Id);

        //gets time zone
        string timeZoneId = await _dbContext.Users.Where(e => e.Id == _currentUser.UserId)
            .Select(e => e.TimezoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? TimeZoneId;

        if(companyIds != null && (chefIds != null && chefIds.Count > 0))
        {
            //get query
            var query = _dbContext.Orders
                .Where(x =>
                    (companyIds.Contains(_dbContext.Organizations
                        .Where(o => o.CompanyId == _dbContext.Users
                            .Where(u => u.Id == x.UserId)
                            .Select(x => x.OrganizationId)
                            .FirstOrDefault())
                        .Select(d => d.Id)
                        .FirstOrDefault())
                    ||
                    chefIds.Contains(_dbContext.Organizations
                        .Where(o => o.CompanyId == _dbContext.Users
                            .Where(u => u.Id == x.UserId)
                            .Select(x => x.OrganizationId)
                            .FirstOrDefault())
                        .Select(o => o.ChefId)
                        .FirstOrDefault())))
                .AsQueryable();

            //account status filter
            if (validFilter.status != null && validFilter.status != 0)
            {
                //Pending
                if (validFilter.status.Value == 1)
                    query = query.Where(u => u.Status == StatusEnum.Pending);
                //Completed
                else if (validFilter.status.Value == 2)
                    query = query.Where(u => u.Status == StatusEnum.Completed);
            }

            //search filter
            if (!string.IsNullOrEmpty(validFilter.searchValue))
            {
                string searchString = validFilter.searchValue.Trim();

                query = query.Where(e => e.MealName.Contains(searchString));
            }

            //sorting
            if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
            {
                query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
            }

            //pagination
            int totalRecords = await query.CountAsync(cancellationToken);

            var pagedData = await query.Select(e => new OrderVM
            {
                id = e.Id,
                userId = e.UserId,
                userPic = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.ImageUrl).FirstOrDefault(),
                userName = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.FullName).FirstOrDefault(),
                companyName = _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(g => g.Name).FirstOrDefault(),
                chefName = _dbContext.Users.Where(c => c.Id == _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(x => x.ChefId).FirstOrDefault()).Select(f => f.FullName).FirstOrDefault() ?? "N/A",
                orderId = e.OrderID,
                timeSlot = e.TimeSlot,
                orderReceived = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") ?? "N/A",
                orderDelivered = e.CompletedAt.HasValue ? e.CompletedAt.Value.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") : "Not yet",
                orderStatus = (int)e.Status,
                orderRating = _dbContext.OrderRating.Where(x => x.OrderId == e.Id).Select(x => x.Value).FirstOrDefault(),
                mealName = e.MealName,
                mealChoice = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).Select(x => x.Name).FirstOrDefault()).Select(x => x.Name).FirstOrDefault(),
                mealSide = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).OrderBy(x => x.Id).Select(x => x.Name).LastOrDefault()).Select(x => x.Name).FirstOrDefault(),
                specialRequest = e.SpecialRequest,
            })
            .Skip(validFilter.pageNumber ?? 0)
            .Take(validFilter.pageSize ?? 0)
            .ToListAsync(cancellationToken);

            var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
            return pagedReponse;
        }
        else if(companyIds != null && (chefIds == null))
        {
            //get query
            var query = _dbContext.Orders
                .Where(x =>
                    companyIds.Contains(_dbContext.Organizations
                        .Where(o => o.CompanyId == _dbContext.Users
                            .Where(u => u.Id == x.UserId)
                            .Select(x => x.OrganizationId)
                            .FirstOrDefault())
                        .Select(d => d.Id)
                        .FirstOrDefault())).AsQueryable();

            //account status filter
            if (validFilter.status != null && validFilter.status != 0)
            {
                //Pending
                if (validFilter.status.Value == 1)
                    query = query.Where(u => u.Status == StatusEnum.Pending);
                //Completed
                else if (validFilter.status.Value == 2)
                    query = query.Where(u => u.Status == StatusEnum.Completed);
            }

            //search filter
            if (!string.IsNullOrEmpty(validFilter.searchValue))
            {
                string searchString = validFilter.searchValue.Trim();

                query = query.Where(e => e.MealName.Contains(searchString));
            }

            //sorting
            if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
            {
                query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
            }

            //pagination
            int totalRecords = await query.CountAsync(cancellationToken);

            var pagedData = await query.Select(e => new OrderVM
            {
                id = e.Id,
                userId = e.UserId,
                userPic = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.ImageUrl).FirstOrDefault(),
                userName = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.FullName).FirstOrDefault(),
                companyName = _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(g => g.Name).FirstOrDefault(),
                chefName = _dbContext.Users.Where(c => c.Id == _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(x => x.ChefId).FirstOrDefault()).Select(f => f.FullName).FirstOrDefault() ?? "N/A",
                orderId = e.OrderID,
                timeSlot = e.TimeSlot,
                orderReceived = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") ?? "N/A",
                orderDelivered = e.CompletedAt.HasValue ? e.CompletedAt.Value.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") : "Not yet",
                orderStatus = (int)e.Status,
                orderRating = _dbContext.OrderRating.Where(x => x.OrderId == e.Id).Select(x => x.Value).FirstOrDefault(),
                mealName = e.MealName,
                mealChoice = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).Select(x => x.Name).FirstOrDefault()).Select(x => x.Name).FirstOrDefault(),
                mealSide = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).OrderBy(x => x.Id).Select(x => x.Name).LastOrDefault()).Select(x => x.Name).FirstOrDefault(),
                specialRequest = e.SpecialRequest,
            })
            .Skip(validFilter.pageNumber ?? 0)
            .Take(validFilter.pageSize ?? 0)
            .ToListAsync(cancellationToken);

            var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
            return pagedReponse;
        }
        else if(companyIds == null && (chefIds != null && chefIds.Count > 0))
        {
            //get query
            var query = _dbContext.Orders
                .Where(x =>
                    chefIds.Contains(_dbContext.Organizations
                        .Where(o => o.CompanyId == _dbContext.Users
                            .Where(u => u.Id == x.UserId)
                            .Select(x => x.OrganizationId)
                            .FirstOrDefault())
                        .Select(o => o.ChefId)
                        .FirstOrDefault())).AsQueryable();

            //account status filter
            if (validFilter.status != null && validFilter.status != 0)
            {
                //Pending
                if (validFilter.status.Value == 1)
                    query = query.Where(u => u.Status == StatusEnum.Pending);
                //Completed
                else if (validFilter.status.Value == 2)
                    query = query.Where(u => u.Status == StatusEnum.Completed);
            }

            //search filter
            if (!string.IsNullOrEmpty(validFilter.searchValue))
            {
                string searchString = validFilter.searchValue.Trim();

                query = query.Where(e => e.MealName.Contains(searchString));
            }

            //sorting
            if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
            {
                query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
            }

            //pagination
            int totalRecords = await query.CountAsync(cancellationToken);

            var pagedData = await query.Select(e => new OrderVM
            {
                id = e.Id,
                userId = e.UserId,
                userPic = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.ImageUrl).FirstOrDefault(),
                userName = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.FullName).FirstOrDefault(),
                companyName = _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(g => g.Name).FirstOrDefault(),
                chefName = _dbContext.Users.Where(c => c.Id == _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(x => x.ChefId).FirstOrDefault()).Select(f => f.FullName).FirstOrDefault() ?? "N/A",
                orderId = e.OrderID,
                timeSlot = e.TimeSlot,
                orderReceived = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") ?? "N/A",
                orderDelivered = e.CompletedAt.HasValue ? e.CompletedAt.Value.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") : "Not yet",
                orderStatus = (int)e.Status,
                orderRating = _dbContext.OrderRating.Where(x => x.OrderId == e.Id).Select(x => x.Value).FirstOrDefault(),
                mealName = e.MealName,
                mealChoice = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).Select(x => x.Name).FirstOrDefault()).Select(x => x.Name).FirstOrDefault(),
                mealSide = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).OrderBy(x => x.Id).Select(x => x.Name).LastOrDefault()).Select(x => x.Name).FirstOrDefault(),
                specialRequest = e.SpecialRequest
            })
            .Skip(validFilter.pageNumber ?? 0)
            .Take(validFilter.pageSize ?? 0)
            .ToListAsync(cancellationToken);

            var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
            return pagedReponse;
        }
        else
        {
            //get query
            var query = _dbContext.Orders.AsQueryable();

            //account status filter
            if (validFilter.status != null && validFilter.status != 0)
            {
                //Pending
                if (validFilter.status.Value == 1)
                    query = query.Where(u => u.Status == StatusEnum.Pending);
                //Completed
                else if (validFilter.status.Value == 2)
                    query = query.Where(u => u.Status == StatusEnum.Completed);
            }

            //search filter
            if (!string.IsNullOrEmpty(validFilter.searchValue))
            {
                string searchString = validFilter.searchValue.Trim();

                query = query.Where(e => e.MealName.Contains(searchString));
            }

            //sorting
            if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
            {
                query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
            }

            //pagination
            int totalRecords = await query.CountAsync(cancellationToken);

            var pagedData = await query.Select(e => new OrderVM
            {
                id = e.Id,
                userId = e.UserId,
                userPic = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.ImageUrl).FirstOrDefault(),
                userName = _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.FullName).FirstOrDefault(),
                companyName = _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(g => g.Name).FirstOrDefault(),
                chefName = _dbContext.Users.Where(c => c.Id == _dbContext.Organizations.Where(o => o.CompanyId == _dbContext.Users.Where(x => x.Id == e.UserId).Select(x => x.OrganizationId).FirstOrDefault()).Select(x => x.ChefId).FirstOrDefault()).Select(f => f.FullName).FirstOrDefault() ?? "N/A",
                orderId = e.OrderID,
                timeSlot = e.TimeSlot,
                orderReceived = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") ?? "N/A",
                orderDelivered = e.CompletedAt.HasValue ? e.CompletedAt.Value.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt") : "Not yet",
                orderStatus = (int)e.Status,
                orderRating = _dbContext.OrderRating.Where(x => x.OrderId == e.Id).Select(x => x.Value).FirstOrDefault(),
                mealName = e.MealName,
                mealChoice = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).Select(x => x.Name).FirstOrDefault()).Select(x => x.Name).FirstOrDefault(),
                mealSide = e.OrderItems.Where(x => x.OrderId == e.Id && x.Category == _dbContext.OrgMealCategories.Where(c => c.OrgMealId == e.OrgMealId).OrderBy(x => x.Id).Select(x => x.Name).LastOrDefault()).Select(x => x.Name).FirstOrDefault(),
                specialRequest = e.SpecialRequest
            })
            .Skip(validFilter.pageNumber ?? 0)
            .Take(validFilter.pageSize ?? 0)
            .ToListAsync(cancellationToken);

            var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
            return pagedReponse;
        }
    }

    public async Task<DatatableResponse<List<ChefVM>>> GetAllChefsPaginated(DataTablePaginationFilter filter, CancellationToken cancellationToken)
    {
        var validFilter = new DataTablePaginationFilter(filter?.pageNumber, filter?.pageSize,
            filter?.status, filter?.sortColumn, filter?.sortColumnDirection, filter?.searchValue);

        // Get the IANA time zone identifier
        string TimeZoneId = TZConvert.WindowsToIana(TimeZoneInfo.Local.Id);

        //gets time zone
        string timeZoneId = await _dbContext.Users.Where(e => e.Id == _currentUser.UserId)
            .Select(e => e.TimezoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? TimeZoneId;

        //get query
        var query = _dbContext.Users.Where(u => u.LoginRole == UserType.Chef).AsQueryable();

        //account status filter
        if (validFilter.status != null && validFilter.status != 0)
        {
            //Active
            if (validFilter.status.Value == 1)
                query = query.Where(u => u.IsUserBlocked == false);
            //Blocked
            else if (validFilter.status.Value == 2)
                query = query.Where(u => u.IsUserBlocked == true);
        }

        //search filter
        if (!string.IsNullOrEmpty(validFilter.searchValue))
        {
            string searchString = validFilter.searchValue.Trim();

            query = query.Where(e => e.FullName.Contains(searchString));
        }

        //sorting
        if (!string.IsNullOrEmpty(validFilter.sortColumn) && !string.IsNullOrEmpty(validFilter.sortColumnDirection))
        {
            query = query.OrderBy(validFilter.sortColumn + " " + validFilter.sortColumnDirection);
        }

        //pagination
        int totalRecords = await query.CountAsync(cancellationToken);

        var pagedData = await query.Select(e => new ChefVM
        {
            id = e.Id,
            pictureUrl = e.ImageUrl,
            fullName = e.FullName,
            email = e.Email,
            phoneNumber = e.PhoneNumber,
            companyId = _dbContext.Organizations.Where(x => x.ChefId == e.Id).Select(x => x.CompanyId).FirstOrDefault(),
            companyName = _dbContext.Organizations.Where(x => x.ChefId == e.Id).Select(x => x.Name).FirstOrDefault(),
            isBlocked = e.IsUserBlocked,
            createdAt = e.CreatedAt.UtcToLocal(timeZoneId).ToString("dd MMM yyyy hh:mm tt"),
        })
        .Skip(validFilter.pageNumber ?? 0)
        .Take(validFilter.pageSize ?? 0)
        .ToListAsync(cancellationToken);

        var pagedReponse = PaginationHelper.CreateDatatableReponse(pagedData, validFilter, totalRecords);
        return pagedReponse;
    }


}
