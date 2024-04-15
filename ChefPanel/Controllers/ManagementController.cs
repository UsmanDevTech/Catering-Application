using ChefPanel.Models;
using ChefPanel.ViewModels;
using Application.Common.Exceptions;
using Application.Accounts.Commands;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System;
using System.Data;
using System.Linq.Dynamic.Core;

namespace ChefPanel.Controllers;
[Authorize]
public class ManagementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public ManagementController(ApplicationDbContext dbContext, ICurrentUserService currentUser,
        UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _context = dbContext;
        _currentUser = currentUser;
        _userManager = userManager;
        _emailService = emailService;
    }

    public string BaseUrl()
    {
        string url = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
        return url;
    }

    private static Random random = new Random();
    private static string RandomString(int length)
    {
        const string chars = "0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task<IActionResult> DashboardAsync(CancellationToken cancellationToken)
    {
        var dashbordVM = new DashboardVM();

        var organization = _context.Organizations.Where(x => x.ChefId == _currentUser.UserId).Select(x => x.CompanyId).FirstOrDefault();

        var users = _context.Users.Where(x => x.OrganizationId == organization).Select(x => x.Id).ToList();

        dashbordVM.PendingOrders = await _context.Orders.Where(x=>x.Status == StatusEnum.Pending && users.Contains(x.UserId))
            .CountAsync(cancellationToken);

        dashbordVM.CompleteOrders= await _context.Orders.Where(x => x.Status == StatusEnum.Completed && users.Contains(x.UserId))
            .CountAsync(cancellationToken);

        dashbordVM.companyName = await _context.Organizations.Where(x => x.ChefId == _currentUser.UserId)
            .Select(x => x.Name).FirstOrDefaultAsync(cancellationToken);

        return View(dashbordVM);
    }

    public IActionResult Meals()
    {
        return View();
    }

    public IActionResult Menu()
    {
        return View();
    }

    public IActionResult Orders(int? status)
    {
        //ViewBag.status = status;
        return View();
    }

    [HttpGet]
    public async Task<JsonResult> GetUserDetailAsync([FromQuery] string userId, CancellationToken cancellationToken)
    {
        try
        {
            dynamic query = null!;
            query = await _context.Users
                 .Where(x => x.Id == userId)
                 .Select(u => new UserDetailVM
                 {
                     id = u.Id,
                     fullName = u.FullName,
                     email = u.Email,
                     phoneNumber = u.PhoneNumber,
                     pictureUrl = u.ImageUrl,
                     allergies = _context.Allergies.Where(x => x.UserId == userId).Select(u => new UserAllergiesVM
                     {
                         id = u.Id,
                         name = u.Name,
                     }).ToList()
                 })
                 .FirstAsync(cancellationToken);

            return new JsonResult(query);
            //return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            throw new Exception(ex.GetBaseException().Message);
            //return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }


    [HttpGet]
    public async Task<JsonResult> GetCompanyDetailAsync([FromQuery] string companyId, CancellationToken cancellationToken)
    {
        try
        {
            dynamic query = null!;
            query = await _context.Organizations
                 .Where(x => x.CompanyId == companyId)
                 .Select(u => new CompanyDetailVM
                 {
                     companyId = u.CompanyId,
                     companyName = u.Name,
                     companyImage = u.ImageUrl,
                     companyEmail = u.Email,
                     companyPhoneNumber = u.Number,
                 })
                 .FirstAsync(cancellationToken);

            return new JsonResult(query);
            //return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            throw new Exception(ex.GetBaseException().Message);
            //return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    //For Block or Unblock Item
    [HttpPost]
    public async Task<IActionResult> SoftDeleteAsync([FromQuery] int id, int isDeleted, CancellationToken token)
    {
        try
        {
            var item = await _context.Items.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (item == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "Item Not Found" });
            }
            else
            {
                if (isDeleted == 1)
                {
                    item.RemoveDelete();
                    _context.Items.Update(item);
                    await _context.SaveChangesAsync(token);
                }
                else
                {
                    item.SoftDelete();
                    _context.Items.Update(item);
                    await _context.SaveChangesAsync(token);
                }
            }

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }


    [HttpPost]
    public async Task<IActionResult> ItemCreateAsync([FromBody] ItemCreateVM itemVM, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(itemVM.name))
                return BadRequest(new ResponseModel { status = false, result = "Name can not be empty" });

            var checkItem = _context.Items.Where(x => x.Name == itemVM.name.Trim()).FirstOrDefault();
            if (checkItem != null)
                return BadRequest(new ResponseModel { status = false, result = "Item Already Exist" });

            //create item

            var item = new Item()
            {
                Name = itemVM.name.Trim(),
                CreatedDate = DateTime.UtcNow,
            };
            _context.Items.Add(item);

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetItemAsync([FromQuery] int itemId, CancellationToken cancellationToken)
    {
        try
        {
            dynamic query = null!;
            query = await _context.Items
                 .Where(x => x.Id == itemId)
                 .Select(u => new
                 {
                     u.Id,
                     u.Name
                 })
                 .FirstAsync(cancellationToken);

            return new JsonResult(query);
            //return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            throw new Exception(ex.GetBaseException().Message);
            //return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] ItemEditVM itemEdit, CancellationToken token)
    {
        try
        {

            var item = await _context.Items.Where(x => x.Id == itemEdit.id).FirstOrDefaultAsync(token);

            if (item == null)
                return BadRequest(new ResponseModel { status = false, result = "Item Not Found" });

            item.Name = itemEdit.name;
            _context.Items.Update(item);
            await _context.SaveChangesAsync(token);

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return new JsonResult("failure");
        }
    }

    [HttpPost]
    public async Task<IActionResult> MealCreateAsync([FromBody] MealCreateVM mealVM, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(mealVM.mealName))
                return BadRequest(new ResponseModel { status = false, result = "Name can not be empty" });

            var checkMeal = _context.MealType.Where(x => x.Name == mealVM.mealName).FirstOrDefault();
            if (checkMeal != null)
                return BadRequest(new ResponseModel { status = false, result = "Meal Already Exist" });

            // Parse the time string into a TimeSpan
            TimeSpan timeOfDay = TimeSpan.Parse(mealVM.startTime);

            // Create a DateTime object with today's date and the parsed time
            DateTime currentStartTime = DateTime.Today.Add(timeOfDay);

            // Parse the time string into a TimeSpan
            TimeSpan timeOfEndDay = TimeSpan.Parse(mealVM.endTime);

            // Create a DateTime object with today's date and the parsed time
            DateTime currentEndTime = DateTime.Today.Add(timeOfEndDay);
            bool hasOverlap = false;

            // Assuming you have a list of existing events
            List<MealType> existingMeals = _context.MealType.Where(x=>x.CreatedById == _currentUser.UserId || x.CreatedByType == UserType.Admin).ToList();

            foreach (MealType existingMeal in existingMeals)
            {
                if (currentStartTime.TimeOfDay < existingMeal.EndTime.TimeOfDay &&
                    currentEndTime.TimeOfDay > existingMeal.StartTime.TimeOfDay)
                {
                    // There is an overlap with an existing event
                    hasOverlap = true;
                    break; // No need to check further, we found an overlap
                }
            }

            if (hasOverlap)
            {
                return BadRequest(new ResponseModel { status = false, result = "Your Meal's Time overlaps with another Meal" });
            }
            else
            {
                //create meal
                var meal = new MealType()
                {
                    PictureUrl = mealVM.mealPic,
                    Name = mealVM.mealName,
                    StartTime = currentStartTime,
                    EndTime = currentEndTime,
                    CreatedByType = Domain.Enums.UserType.Chef,
                    CreatedById = _currentUser.UserId,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.MealType.Add(meal);
                await _context.SaveChangesAsync(cancellationToken);

                if (mealVM.categories != null && mealVM.categories.Count > 0)
                {
                    var icon = "";
                    foreach (var cat in mealVM.categories)
                    {
                        if (string.IsNullOrEmpty(cat.catImage))
                        {
                            icon = null;
                        }
                        else
                        {
                            icon = cat.catImage;
                        }

                        var category = new MealCategory()
                        {
                            Name = cat.catName,
                            ImageUrl = icon,
                            MealTypeId = meal.Id
                        };
                        _context.MealCategories.Add(category);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                return Ok(new ResponseModel { status = true, result = "success" });
            }

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetMealAsync([FromQuery] int mealId, CancellationToken cancellationToken)
    {
        try
        {
            dynamic query = null!;
            query = await _context.MealType
                 .Where(x => x.Id == mealId)
                 .Select(u => new MealGetVM
                 {
                     id = u.Id,
                     mealName = u.Name,
                     mealPic = u.PictureUrl,
                     startTime = u.StartTime.ToString("HH:mm"),
                     endTime = u.EndTime.ToString("HH:mm"),
                     categories = u.MealCategories.Where(x => x.MealTypeId == u.Id).Select(x => new mealCatGetVM
                     {
                         catId = x.Id,
                         catName = x.Name,
                         catImage = x.ImageUrl
                     }).ToList(),
                 })
                 .FirstAsync(cancellationToken);


            return new JsonResult(query);
            //return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            throw new Exception(ex.GetBaseException().Message);
            //return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    //For Block or Unblock
    [HttpPost]
    public async Task<IActionResult> SoftMealDeleteAsync([FromQuery] int id, int isDeleted, CancellationToken token)
    {
        try
        {
            var meal = await _context.MealType.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (meal == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "Meal Not Found" });
            }
            else
            {
                if (isDeleted == 1)
                {
                    meal.RemoveDelete();
                    _context.MealType.Update(meal);
                    await _context.SaveChangesAsync(token);
                }
                else
                {
                    meal.SoftDelete();
                    _context.MealType.Update(meal);
                    await _context.SaveChangesAsync(token);
                }
            }

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MealUpdateAsync([FromBody] MealUpdateVM mealVM, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(mealVM.mealName))
                return BadRequest(new ResponseModel { status = false, result = "Name can not be empty" });

            var checkMeal = _context.MealType.Where(x => x.Name == mealVM.mealName && x.Id != mealVM.id).FirstOrDefault();
            if (checkMeal != null)
                return BadRequest(new ResponseModel { status = false, result = "Meal Already Exist" });

            // Parse the time string into a TimeSpan
            TimeSpan timeOfDay = TimeSpan.Parse(mealVM.startTime);

            // Create a DateTime object with today's date and the parsed time
            DateTime currentStartTime = DateTime.Today.Add(timeOfDay);

            // Parse the time string into a TimeSpan
            TimeSpan timeOfEndDay = TimeSpan.Parse(mealVM.endTime);

            // Create a DateTime object with today's date and the parsed time
            DateTime currentEndTime = DateTime.Today.Add(timeOfEndDay);
            bool hasOverlap = false;

            // Assuming you have a list of existing events
            List<MealType> existingMeals = _context.MealType.Where(x=> x.CreatedById == _currentUser.UserId || x.CreatedByType == UserType.Admin).ToList();

            foreach (MealType existingMeal in existingMeals)
            {
                if (existingMeal.Id != mealVM.id)
                {
                    if (currentStartTime.TimeOfDay < existingMeal.EndTime.TimeOfDay &&
                        currentEndTime.TimeOfDay > existingMeal.StartTime.TimeOfDay)
                    {
                        // There is an overlap with an existing event
                        hasOverlap = true;
                        break; // No need to check further, we found an overlap
                    }
                }

            }

            if (hasOverlap)
            {
                return BadRequest(new ResponseModel { status = false, result = "Your Time belongs to our previous Meals" });
            }
            else
            {
                //create meal
                var meal = _context.MealType.Where(x => x.Id == mealVM.id).FirstOrDefault();
                if (meal == null)
                    return BadRequest(new ResponseModel { status = false, result = "Meal not Found" });


                meal.PictureUrl = mealVM.mealPic;
                meal.Name = mealVM.mealName;
                meal.StartTime = currentStartTime;
                meal.EndTime = currentEndTime;
                _context.MealType.Update(meal);
                await _context.SaveChangesAsync(cancellationToken);

                var mealcategories = _context.MealCategories.Where(x => x.MealTypeId == meal.Id).ToList();

                if (mealcategories != null && mealcategories.Count > 0)
                {
                    foreach (var cat in mealcategories)
                    {
                        _context.MealCategories.Remove(cat);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                if (mealVM.categories != null && mealVM.categories.Count > 0)
                {
                    var icon = "";
                    foreach (var cat in mealVM.categories)
                    {
                        if (string.IsNullOrEmpty(cat.catImage))
                        {
                            icon = null;
                        }
                        else
                        {
                            icon = cat.catImage;
                        }

                        var category = new MealCategory()
                        {
                            Name = cat.catName,
                            ImageUrl = icon,
                            MealTypeId = meal.Id
                        };
                        _context.MealCategories.Add(category);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                return Ok(new ResponseModel { status = true, result = "success" });
            }

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetOrderDetailAsync([FromQuery] int orderId, CancellationToken cancellationToken)
    {
        try
        {
            dynamic query = null!;
            query = await _context.Orders
                 .Where(x => x.Id == orderId)
                 .Select(u => new GetOrderDetailVM
                 {
                     id = u.Id,
                     orderId = u.OrderID,
                     orderStatus = (int)u.Status,
                     specialRequest = u.SpecialRequest,
                     timeSlot = u.TimeSlot,
                     orderRating = _context.OrderRating.Where(x => x.OrderId == u.Id).Select(x => x.Value).FirstOrDefault(),
                     orderItems = u.OrderItems.Where(x => x.OrderId == u.Id).Select(o => new orderItem
                     {
                         id = o.Id,
                         itemName = o.Name,
                         categoryName = o.Category,
                         categoryIcon = o.CategoryImage,
                     }).ToList(),
                 })
                 .FirstAsync(cancellationToken);

            return new JsonResult(query);
            //return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            throw new Exception(ex.GetBaseException().Message);
            //return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    //For Mark Order Complete
    [HttpPost]
    public async Task<IActionResult> markOrderCompleteAsync([FromQuery] int id, int isStatus, CancellationToken token)
    {
        try
        {
            var order = await _context.Orders.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (order == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "Order Not Found" });
            }
            else
            {
                if (isStatus == 1)
                {
                    order.Status = Domain.Enums.StatusEnum.Completed;
                    order.CompletedAt = DateTime.UtcNow;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync(token);
                }
            }

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationCountAsync(CancellationToken cancellationToken)
    {
        try
        {
            //gets user from DB
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            var notifybool = false;
            var query = await _context.Notifications
                 .Where(x => x.NotifiedToId == _currentUser.UserId && x.IsSeen == false).OrderByDescending(x => x.CreatedDate).ToListAsync();
            if (query.Count == 0)
            {
                notifybool = false;
            }
            else
            {
                notifybool = true;
            }
            return Ok(notifybool);
            //return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            throw new Exception(ex.GetBaseException().Message);
            //return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }


    [HttpGet]
    public async Task<IActionResult> GetNotificationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            //gets user from DB
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);

            var query = await _context.Notifications
                 .Where(x => x.NotifiedToId == _currentUser.UserId).OrderByDescending(x => x.CreatedDate)
                 .Select(u => new GetNotificationVM
                 {
                     id = u.Id,
                     description = u.Description,
                     isSeen = u.IsSeen,
                     notifType = u.NotifType,
                     userName = _userManager.Users.Where(x => x.Id == u.NotifiedById).Select(x => x.FullName).FirstOrDefault(),
                     userPic = _userManager.Users.Where(x => x.Id == u.NotifiedById).Select(x => x.ImageUrl).FirstOrDefault()
                 }).ToListAsync(cancellationToken);

            var dummyList = query.Select(u => new GetNotificationVM
            {
                id = u.id,
                description = u.description,
                isSeen = u.isSeen,
                notifType = u.notifType,
                userName = u.userName,
                userPic = u.userPic
            }).ToList();
            var notificationlist = _context.Notifications.Where(x => x.NotifiedToId == _currentUser.UserId).ToList();
            if (notificationlist != null)
            {
                foreach (var notify in notificationlist)
                {
                    notify.IsSeen = true;
                    _context.Notifications.Update(notify);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(dummyList);
            //return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            throw new Exception(ex.GetBaseException().Message);
            //return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCompaniesAsync([FromQuery] string? orgName, CancellationToken cancellationToken)
    {
        try
        {
            var companies = await _context.Organizations
                .Where(e => !e.IsDeleted)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.ImageUrl
                })
                .ToListAsync(cancellationToken);

            return Ok(companies);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMealTypesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var companies = await _context.MealType
                .Where(e => !e.IsDeleted && (e.CreatedByType == Domain.Enums.UserType.Admin ||
                (e.CreatedByType == UserType.Chef && e.CreatedById == _currentUser.UserId) ))
                .Select(e => new MealTypeGetVM
                {
                    Id = e.Id,
                    PictureUrl = e.PictureUrl,
                    Name = e.Name,
                    StartTime = e.StartTime.ToString("hh:mm tt"),
                    EndTime = e.EndTime.ToString("hh:mm tt"),
                })
                .ToListAsync(cancellationToken);

            return Ok(companies);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrganizationMealsAsync([FromQuery] int weekDay,CancellationToken cancellationToken)
    {
        try
        {
            var orgId = _context.Organizations.Where(x => x.ChefId == _currentUser.UserId).Select(x => x.Id).FirstOrDefault();

            var organizationMeals = await _context.OrgMeals
                .Where(e => e.WeekDay == (WeekDays)weekDay && e.OrganizationId == orgId)
                .OrderBy(e=>e.StartTime)
                .Select(e => new MealTypeGetVM
                {
                    Id = e.Id,
                    PictureUrl = e.MealType.PictureUrl,
                    Name = e.MealType.Name,
                    StartTime = e.StartTime.ToString("hh:mm tt"),
                    EndTime = e.EndTime.ToString("hh:mm tt"),
                })
                .ToListAsync(cancellationToken);

            return Ok(organizationMeals);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrganizationMealCategoriesAsync([FromQuery] int orgMealId, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = _context.OrgMeals.Where(x => x.Id == orgMealId).Select(x => x.Id).FirstOrDefault();

            var mealsCategories = await _context.OrgMealCategories
                .Where(e => e.OrgMealId == orgMealId)
                .Select(e => new OrganizationCategoryVM
                {
                    id = e.Id,
                    Name = e.Name,
                    MealTypeId = e.OrgMealId,
                    mealItems = e.OrgMealCatAndOrgMealitems.Where(x=>x.OrgMealCategoryId == e.Id).Select(m=> new MealItems()
                    {
                        id = m.Id,
                        ItemName = m.Items.Name,
                        MealCatId = e.Id,
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return Ok(mealsCategories);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    public IActionResult ManageMeals()
    {
        var companyname = _context.Organizations.Where(x => x.ChefId == _currentUser.UserId).Select(x => x.Name).FirstOrDefault();
        //var a = TempData["companyIds"];
        TempData["companyName"] = companyname;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetOrgMealsAsync([FromQuery] int weekDay, CancellationToken cancellationToken)
    {
        try
        {
            //get chef's company Id
            int companyId = await _context.Organizations.Where(e => e.ChefId == _currentUser.UserId)
                .Select(e => e.Id)
                .FirstAsync(cancellationToken);

            var orgMeals = await _context.OrgMeals
                .Where(e => e.WeekDay == (WeekDays)weekDay && e.OrganizationId == companyId)
                .OrderBy(e => e.StartTime.TimeOfDay)
                .Select(e => new MealTypeGetVM
                {
                    Id = e.Id,
                    PictureUrl = e.MealType.PictureUrl,
                    Name = e.MealType.Name,
                    StartTime = e.StartTime.ToString("hh:mm tt"),
                    EndTime = e.EndTime.ToString("hh:mm tt"),
                })
                .ToListAsync(cancellationToken);

            return Ok(orgMeals);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditOrgMealTimeAsync([FromBody] EditTimeVM mealVM, CancellationToken cancellationToken)
    {
        try
        {
            //get chef's company Id
            int companyId = await _context.Organizations.Where(e => e.ChefId == _currentUser.UserId)
                .Select(e => e.Id)
                .FirstAsync(cancellationToken);

            var orgMeal = _context.OrgMeals.Where(x => x.Id == mealVM.OrgMealId).FirstOrDefault();
            if (orgMeal == null)
                return BadRequest(new ResponseModel { status = false, result = "Company Meal does not Exist" });

            // Parse the time string into a TimeSpan
            TimeSpan timeOfDay = TimeSpan.Parse(mealVM.StartTime);

            // Create a DateTime object with today's date and the parsed time
            DateTime currentStartTime = DateTime.Today.Add(timeOfDay);

            // Parse the time string into a TimeSpan
            TimeSpan timeOfEndDay = TimeSpan.Parse(mealVM.EndTime);

            // Create a DateTime object with today's date and the parsed time
            DateTime currentEndTime = DateTime.Today.Add(timeOfEndDay);
            bool hasOverlap = false;

            // Assuming you have a list of existing events
            var existingOrgMeals = await _context.OrgMeals.Where(e => e.OrganizationId == companyId &&
                e.WeekDay == (WeekDays)mealVM.WeekDay)
                .ToListAsync(cancellationToken);

            foreach (OrgMeal existingMeal in existingOrgMeals)
            {
                if (existingMeal.Id != mealVM.OrgMealId)
                {
                    if (currentStartTime.TimeOfDay < existingMeal.EndTime.TimeOfDay &&
                        currentEndTime.TimeOfDay > existingMeal.StartTime.TimeOfDay)
                    {
                        // There is an overlap with an existing event
                        hasOverlap = true;
                        break; // No need to check further, we found an overlap
                    }
                }

            }

            if (hasOverlap)
            {
                return BadRequest(new ResponseModel { status = false, result = "Your Time overlaps with an existing Meal" });
            }
            else
            {
                orgMeal.StartTime = currentStartTime;
                orgMeal.EndTime = currentEndTime;

                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new ResponseModel { status = true, result = mealVM });
            }

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveOrgMealItemAsync([FromBody] OrgMealItems mealItems, CancellationToken cancellationToken)
    {
        try
        {
            var mealcategory = await _context.OrgMealCatAndOrgMealitems.Where(u => u.ItemId == mealItems.itemId && u.OrgMealCategoryId == mealItems.categoryId).FirstOrDefaultAsync(cancellationToken);

            if (mealcategory == null)
                return BadRequest(new ResponseModel { status = false, result = "Meal Menu Not Found" });

            //deleting items relation
            _context.OrgMealCatAndOrgMealitems.Remove(mealcategory);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }


    [HttpPost]
    public async Task<IActionResult> DeleteOrgMealAsync([FromQuery] int orgMealId, CancellationToken cancellationToken)
    {
        try
        {
            //gets branch
            var orgMeal = await _context.OrgMeals.Where(u => u.Id == orgMealId).FirstOrDefaultAsync(cancellationToken);
            if (orgMeal == null)
                return BadRequest(new ResponseModel { status = false, result = "Organization Meal Not Found" });

            ///////////////////////////////////////////////////////////
            ///CANT DELETE TODAY MEAL//////////////////////////////////
            ///////////////////////////////////////////////////////////

            //deleting room and all relevant entities
            _context.OrgMeals.Remove(orgMeal);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = orgMealId });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public IActionResult CompanyMealItemsEditInter([FromBody] OrgMealEditVM mealVM)
    {
        //stores orgMeal data in cookies
        TempData["orgMealId-edit"] = mealVM.OrgMealId;
        TempData["weekDay-edit"] = mealVM.WeekDay;

        TempData["mealTypeName-edit"] = mealVM.MealTypeName;

        return new JsonResult("success");
    }

    [HttpGet]
    public IActionResult CompanyMealItemsEdit()
    {
        var companyname = _context.Organizations.Where(x => x.ChefId == _currentUser.UserId)
            .Select(x => x.Name).FirstOrDefault();
        //var a = TempData["companyIds"];
        TempData["companyName"] = companyname;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetOrgMealCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            //gets mealTypeId from cookies
            int orgMealId = Convert.ToInt32(TempData.Peek("orgMealId-edit"));

            var cats = await _context.OrgMealCategories
                .Where(e => e.OrgMealId == orgMealId)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                })
                .ToListAsync(cancellationToken);

            return Ok(cats);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrgMealItemsAsync(CancellationToken cancellationToken)
    {
        try
        {
            int orgMealId = Convert.ToInt32(TempData.Peek("orgMealId-edit"));

            var cats = await _context.OrgMeals.Where(e => e.Id == orgMealId)
                .SelectMany(e => e.OrgMealCategories)
                .SelectMany(e => e.OrgMealCatAndOrgMealitems)
                .Select(e => new
                {
                    e.OrgMealCategoryId,
                    e.ItemId,
                    e.Items.Name
                })
                .ToListAsync(cancellationToken);

            return Ok(cats);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetSelectiveItemsAsync([FromBody] List<int> availableItemIds, CancellationToken cancellationToken)
    {
        try
        {

            var items = await _context.Items.Where(e => !e.IsDeleted &&
                !availableItemIds.Contains(e.Id))
                .OrderBy(e => e.Name)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                })
                .ToListAsync(cancellationToken);

            //var remainItems = items.Where(e => !availableItemIds.Contains(e.Id));

            return Ok(items);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrgMealItemsAsync([FromBody] List<SelectedItemCreateVM> updateVM, CancellationToken cancellationToken)
    {
        try
        {

            var mealCatIds = updateVM.Select(e => Convert.ToInt32(e.CategoryId)).Distinct().ToList();

            //deletes existing selected items
            //foreach (int catId in mealCatIds)
            //{
            //    var catItems = await _context.OrgMealCatAndOrgMealitems.Where(e => e.OrgMealCategoryId == catId)
            //        .ToListAsync(cancellationToken);

            //    _context.OrgMealCatAndOrgMealitems.RemoveRange(catItems);
            //}

            // creates new slect relations
            foreach (var item in updateVM)
            {
                var catItem = new OrgMealCatAndOrgMealitem
                {
                    ItemId = Convert.ToInt32(item.ItemId),
                    OrgMealCategoryId = Convert.ToInt32(item.CategoryId)
                };
                _context.OrgMealCatAndOrgMealitems.Add(catItem);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> OrgMealCreateAsync([FromBody] OrgMealCreateVM mealVM, CancellationToken cancellationToken)
    {
        try
        {
            //get chef's company Id
            int companyId = await _context.Organizations.Where(e => e.ChefId == _currentUser.UserId)
                .Select(e => e.Id)
                .FirstAsync(cancellationToken);

            if(mealVM.mealTypeIds != null && mealVM.mealTypeIds.Count > 0)
            {
                var executeRequest = true;
                foreach(var mealtype in mealVM.mealTypeIds)
                {
                    //check
                    var existOrgMeal = await _context.OrgMeals.Where(e => e.WeekDay == (WeekDays)mealVM.WeekDay && e.OrganizationId == companyId &&
                        e.MealTypeId == mealtype)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existOrgMeal != null)
                    {
                        return BadRequest(new ResponseModel { status = false, result = "Meal already exists. Please select a different Meal" });
                        executeRequest = false;
                    }

                    if(executeRequest == true)
                    {
                        //get mealType
                        var meal = await _context.MealType.Where(e => e.Id == mealtype).FirstAsync(cancellationToken);

                        var orgMeal = new OrgMeal
                        {
                            MealTypeId = mealtype,
                            OrganizationId = companyId,
                            WeekDay = (WeekDays)mealVM.WeekDay,
                            StartTime = meal.StartTime,
                            EndTime = meal.EndTime
                        };
                        _context.OrgMeals.Add(orgMeal);
                        await _context.SaveChangesAsync(cancellationToken);

                        var categories = _context.MealCategories.Where(x => x.MealTypeId == mealtype).ToList();

                        if (categories != null && categories.Count > 0)
                        {
                            foreach (var category in categories)
                            {
                                var orgCategory = new OrgMealCategory
                                {
                                    OrgMealId = orgMeal.Id,
                                    ImageUrl = category.ImageUrl,
                                    Name = category.Name,
                                    CreatedDate = DateTime.UtcNow,
                                };
                                _context.OrgMealCategories.Add(orgCategory);
                                await _context.SaveChangesAsync(cancellationToken);
                            }
                        }
                    }
                }
            }

            var addmeal = _context.OrgMeals.Where(x => x.WeekDay == (WeekDays)mealVM.WeekDay && x.OrganizationId == companyId).Select(u => new OrganizationsMealsVM
            {
                id = u.Id,
                Name = u.MealType.Name,
                StartTime = u.MealType.StartTime.ToString("hh:mm tt"),
                EndTime = u.MealType.EndTime.ToString("hh:mm tt"),
                PictureUrl = u.MealType.PictureUrl,
                WeekDay = u.WeekDay
            }).ToList();

            return Ok(new ResponseModel { status = true, result = addmeal });

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    //*************** Copy Meals

    [HttpGet]
    public async Task<IActionResult> GetInvalidDaysAsync([FromQuery] int orgMealId, CancellationToken cancellationToken)
    {
        try
        {
            //gets orgMeal
            var orgMeal = await _context.OrgMeals.Where(u => u.Id == orgMealId).FirstOrDefaultAsync(cancellationToken);
            if (orgMeal == null)
                return BadRequest(new ResponseModel { status = false, result = "Organization Meal Not Found" });

            var weekDays = await _context.OrgMeals.Where(e => e.MealTypeId == orgMeal.MealTypeId && 
                e.OrganizationId == orgMeal.OrganizationId)
                .Select(e => (int) e.WeekDay)
                .ToListAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = weekDays });
            //return Ok(weekDays);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> OrgMealCopyAsync([FromBody] OrgMealCopyVM mealVM, CancellationToken cancellationToken)
    {
        try
        {
            //gets orgMeal
            var orgMeal = await _context.OrgMeals.Where(u => u.Id == mealVM.OrgMealId)
                .Include(e => e.OrgMealCategories)
                .ThenInclude(a => a.OrgMealCatAndOrgMealitems)
                .FirstOrDefaultAsync(cancellationToken);
            if (orgMeal == null)
                return BadRequest(new ResponseModel { status = false, result = "Organization Meal Not Found" });

            foreach(int weekDay in mealVM.WeekDays)
            {
                //create new orgMeal
                var newOrgMeal = new OrgMeal
                {
                    MealTypeId = orgMeal.MealTypeId,
                    OrganizationId = orgMeal.OrganizationId,
                    WeekDay = (WeekDays) weekDay,
                    StartTime = orgMeal.StartTime,
                    EndTime = orgMeal.EndTime,
                };
                _context.OrgMeals.Add(newOrgMeal);

                //create orgMealCategories
                foreach(var cat in orgMeal.OrgMealCategories)
                {
                    //create new category
                    var newCategory = new OrgMealCategory
                    {
                        Name = cat.Name,
                        ImageUrl = cat.ImageUrl,
                        CreatedDate = DateTime.UtcNow,
                        OrgMeal = newOrgMeal
                    };
                    _context.OrgMealCategories.Add(newCategory);

                    //adding items
                    foreach(var itemRelation in cat.OrgMealCatAndOrgMealitems)
                    {
                        var newItemRel = new OrgMealCatAndOrgMealitem
                        {
                            ItemId = itemRelation.ItemId,
                            OrgMealCategories = newCategory
                        };
                        _context.OrgMealCatAndOrgMealitems.Add(newItemRel);
                    }

                }

                await _context.SaveChangesAsync(cancellationToken);

            }

            return Ok(new ResponseModel { status = true, result = "success" });

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }




    internal async Task<string> UploadFile(IFormFile file)
    {
        long fileSize = file.Length;
        string extension = Path.GetExtension(file.FileName);
        string ext = file.ContentType;
        string path_1 = Path.Combine(@"wwwroot", "ChefImages");
        string uploadsFolder = Path.Join(Directory.GetCurrentDirectory(), path_1);
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
        string uniqueFileName = Guid.NewGuid().ToString() + extension;
        string filePath = Path.Join(uploadsFolder, uniqueFileName);
        using (var _fileStream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(_fileStream);

        return BaseUrl() + "/ChefImages/" + uniqueFileName;
    }

    //File upload functions
    public async Task<JsonResult> FileUpload(IFormFile file)
    {
        var url = "";
        if (file != null)
        {
            url = await UploadFile(file);
        }
        return new JsonResult(url);
    }

}
