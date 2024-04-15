using AdminPanel.Models;
using AdminPanel.ViewModels;
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

namespace AdminPanel.Controllers;
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

        dashbordVM.OrganizationsCount = await _context.Organizations
            .CountAsync(cancellationToken);

        dashbordVM.CheifsCount = await _context.Users
            .Where(x => x.LoginRole == Domain.Enums.UserType.Chef)
            .CountAsync(cancellationToken);

        dashbordVM.UsersCount = await _context.Users
            .Where(e => e.LoginRole == Domain.Enums.UserType.User && e.UserApprovalStatus == UserApprovalStatusEnum.Approved)
            .CountAsync(cancellationToken);

        dashbordVM.OrdersCount = await _context.Orders
            .CountAsync(cancellationToken);

        return View(dashbordVM);
    }

    public IActionResult Meals()
    {
        return View();
    }

    public IActionResult Items()
    {
        return View();
    }

    public IActionResult Users()
    {
        return View();
    }

    public IActionResult Orders()
    {
        return View();
    }

    public IActionResult Chefs()
    {
        return View();
    }

    //For Block and Unblock User
    [HttpPost]
    public async Task<IActionResult> softDeleteUserAsync([FromQuery] string id, int isStatus, CancellationToken token)
    {
        try
        {
            var user = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (user == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "User Not Found" });
            }
            else
            {
                if (isStatus == 1)
                {
                    user.IsUserBlocked = true;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync(token);
                }
                else
                {
                    user.IsUserBlocked = false;
                    _context.Users.Update(user);
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

    //For Block and Unblock User
    [HttpPost]
    public async Task<IActionResult> softDeleteChefAsync([FromQuery] string id, int isStatus, CancellationToken token)
    {
        try
        {
            var user = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (user == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "User Not Found" });
            }
            else
            {
                if (isStatus == 1)
                {
                    var organizations = _context.Organizations.Where(x => x.ChefId == id).ToList();
                    if (organizations != null && organizations.Count > 0)
                    {
                        foreach (var chef in organizations)
                        {
                            chef.ChefId = null;
                            _context.Organizations.Update(chef);
                            await _context.SaveChangesAsync(token);
                        }
                    }

                    user.IsUserBlocked = true;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync(token);
                }
                else
                {
                    user.IsUserBlocked = false;
                    _context.Users.Update(user);
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
                     companyId =  u.CompanyId,
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

    //For Approve and Reject User Request
    [HttpPost]
    public async Task<IActionResult> UpdateUserRequestStatusAsync([FromQuery] string id, int isStatus,string? reason, CancellationToken token)
    {
        try
        {
            var user = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (user == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "User Not Found" });
            }
            else
            {
                if (isStatus == (int)Domain.Enums.UserApprovalStatusEnum.Approved)
                {
                    //var companyId = _context.Users.Where(x => x.Id == id).Select(e=>e.OrganizationId).FirstOrDefault();
                    //var users = _context.Users.Where(x => x.OrganizationId.ToUpper() == companyId.ToUpper() && x.LoginRole == UserType.User).ToList();
                    //var strength = _context.Organizations.Where(x=>x.CompanyId == companyId.ToUpper()).Select(x=>x.Employees).FirstOrDefault();
                    //if(strength < users.Count)
                    //{
                    //    return BadRequest(new ResponseModel { status = false, result = "Your User limit is full kindly increase your organization strength" });
                    //}
                    //else
                    //{
                    //    user.UserApprovalStatus = Domain.Enums.UserApprovalStatusEnum.Approved;
                    //    _context.Users.Update(user);
                    //    await _context.SaveChangesAsync(token);
                    //}
                    user.UserApprovalStatus = Domain.Enums.UserApprovalStatusEnum.Approved;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync(token);
                }
                else
                {
                    user.UserApprovalStatus = Domain.Enums.UserApprovalStatusEnum.Rejected;
                    user.RejectionReason = reason;
                    _context.Users.Update(user);
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


    //For Block or Unblock
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
            List<MealType> existingMeals = _context.MealType.ToList();

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
                    CreatedByType = Domain.Enums.UserType.Admin,
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
                 .Where(x => x.Id == mealId && x.CreatedByType == Domain.Enums.UserType.Admin)
                 .Select(u => new MealGetVM
                 {
                     id =  u.Id,
                     mealName =  u.Name,
                     mealPic =  u.PictureUrl,
                     startTime = u.StartTime.ToString("HH:mm"),
                     endTime = u.EndTime.ToString("HH:mm"),
                     categories = u.MealCategories.Where(x=>x.MealTypeId == u.Id).Select(x=> new mealCatGetVM
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
            List<MealType> existingMeals = _context.MealType.ToList();

            foreach (MealType existingMeal in existingMeals)
            {
                if(existingMeal.Id != mealVM.id)
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
                return BadRequest(new ResponseModel { status = false, result = "Your Meal's Time overlaps with another Meal" });
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
                     orderItems = u.OrderItems.Where(x=>x.OrderId == u.Id).Select(o=> new orderItem
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

    [HttpPost]
    public async Task<IActionResult> ChefCreateAsync([FromBody] ChefCreateVM chefVM, CancellationToken cancellationToken)
    {
        try
        {
            // check existing user
            var userExist = await _userManager.FindByEmailAsync(chefVM.email);
            if (userExist != null)
            {
                throw new UserAlreadyExistsException();
            }

            //var checkName = _context.Users.Where(x => x.FullName == chefVM.fullName && x.LoginRole == Domain.Enums.UserType.Chef).FirstOrDefault();
            //if (checkName != null)
            //    return BadRequest(new ResponseModel { status = false, result = "Name Already Exist" });

            //add user 
            ApplicationUser user = new()
            {
                OrganizationId = null,
                Email = chefVM.email,
                UserName = chefVM.email,
                FullName = chefVM.fullName,
                PhoneNumber = chefVM.phoneNumber,
                UserApprovalStatus = UserApprovalStatusEnum.Pending,
                ImageUrl = chefVM.pictureUrl,
                LoginRole = UserType.Chef,
                TimezoneId = "Asia/Karachi"   //should be dynamic not static
            };

            var result = await _userManager.CreateAsync(user, chefVM.password);// if duplicate name, dont create user
            if (!result.Succeeded)
            {
                throw new Exception($"New user could not be created ({result.ToString()})");
            }

            //return (result.ToApplicationResult());
            return Ok(new ResponseModel { status = true, result = "success" });

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChefUpdateAsync([FromBody] ChefUpdateVM chefVM, CancellationToken cancellationToken)
    {
        try
        {

            // check existing user
            var userExist = await _userManager.FindByEmailAsync(chefVM.email);
            //if (userExist != null)
            //{
            //    throw new UserAlreadyExistsException();
            //}

            // Get the existing student from the db
            var user = await _userManager.FindByIdAsync(chefVM.id);
            // Update it with the values from the view model

            if (!string.IsNullOrEmpty(chefVM.pictureUrl))
                user.ImageUrl = chefVM.pictureUrl;

            //if (!string.IsNullOrEmpty(chefVM.email))
            //    user.Email = chefVM.email;

            if (!string.IsNullOrEmpty(chefVM.fullName))
                user.FullName = chefVM.fullName;

            if (!string.IsNullOrEmpty(chefVM.phoneNumber))
                user.PhoneNumber = chefVM.phoneNumber;

            // Apply the changes if any to the db
            var result = await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(chefVM.password))
            {
                //Change password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPassResult = await _userManager.ResetPasswordAsync(user, token,chefVM.password);

                if (resetPassResult.Succeeded)
                    return Ok(new { status = true, result = "" });
                else
                    return BadRequest(new { status = false, result = resetPassResult.ToString() });
            }

            if (result.Succeeded)
                return Ok(new { status = true, result = "success" });
            else
                return BadRequest(new { status = false, result = result.Errors.ToString() });

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }


    [HttpGet]
    public async Task<JsonResult> GetChefDetailAsync([FromQuery] string chefId, CancellationToken cancellationToken)
    {
        try
        {
            dynamic query = null!;
            query = await _context.Users
                 .Where(x => x.Id == chefId)
                 .Select(u => new GetChefVM
                 {
                     id = u.Id,
                     pictureUrl = u.ImageUrl,
                     fullName = u.FullName,
                     email = u.Email,
                     phoneNumber = u.PhoneNumber,
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

    //For Assign Company to Chef
    [HttpPost]
    public async Task<IActionResult> assignCompanyAsync([FromQuery] string id,int companyId,CancellationToken token)
    {
        try
        {
            var user = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (user == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "User Not Found" });
            }
            else
            {
                if(companyId != null)
                {
                    var organizations = _context.Organizations.Where(x => x.ChefId == id).ToList();
                    if(organizations !=null && organizations.Count > 0)
                    {
                        foreach(var chef in organizations)
                        {
                            chef.ChefId = null;
                            _context.Organizations.Update(chef);
                            await _context.SaveChangesAsync(token);
                        }
                    }
                    var org = await _context.Organizations.Where(x => x.Id == companyId).FirstOrDefaultAsync(token);
                    org.ChefId = id;
                    _context.Organizations.Update(org);
                    await _context.SaveChangesAsync(token);
                }
                else
                {
                    return BadRequest(new ResponseModel { status = false, result = "You need to select Company" });
                }
            }

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    //For Orders Filters
    [HttpPost]
    public async Task<IActionResult> applyOrderFiltersAsync([FromBody] List<int> companyIds, List<int> chefIds, CancellationToken token)
    {
        try
        {
            var userId = _currentUser.UserId;
            var companies = _context.Organizations.Where(x => x.IsDeleted == false).ToList();

            if(companyIds != null || chefIds != null)
            {

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
            if(query.Count == 0)
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
                 .Where(x => x.NotifiedToId == _currentUser.UserId).OrderByDescending(x=>x.CreatedDate)
                 .Select(u => new GetNotificationVM
                 {
                     id = u.Id,
                     description = u.Description,
                     isSeen = u.IsSeen,
                     notifType = u.NotifType,
                     userName = _userManager.Users.Where(x=> x.Id == u.NotifiedById).Select(x => x.FullName).FirstOrDefault(),
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
                foreach(var notify in notificationlist)
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
    public async Task<IActionResult> GetCompaniesandChefsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var getAll = new GetOrdersCompaniesVM()
            {
                companies = _context.Organizations.Where(x => x.IsDeleted == false).Select(u => new Companies()
                {
                    id = u.Id,
                    companyName = u.Name,
                    companyUrl = u.ImageUrl,
                }).ToList(),
                chefs = _context.Users.Where(x => x.IsUserBlocked == false && x.LoginRole == UserType.Chef).Select(c => new Chefs()
                {
                    id = c.Id,
                    chefName = c.FullName,
                    chefPic = c.ImageUrl
                }).ToList(),
            };

            return Ok(getAll);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    public IActionResult Organizations()
    {
        return View();
    }

    //For Block or Unblock
    [HttpPost]
    public async Task<IActionResult> SoftDeleteOrganizationAsync([FromQuery] int id, int isDeleted, CancellationToken token)
    {
        try
        {
            var org = await _context.Organizations.Where(x => x.Id == id).FirstOrDefaultAsync(token);
            if (org == null)
            {
                return BadRequest(new ResponseModel { status = false, result = "Organization Not Found" });
            }
            else
            {
                if (isDeleted == 1)
                {
                    org.RemoveDelete();
                    await _context.SaveChangesAsync(token);
                }
                else
                {
                    org.SoftDelete();
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
    public async Task<IActionResult> OrganizationCreateAsync([FromBody] OrganizationCreateVM createVM, CancellationToken cancellationToken)
    {
        try
        {
            //already exist check
            var checkItem = _context.Organizations.Where(x => x.Email == createVM.Email).FirstOrDefault();
            if (checkItem != null)
                return BadRequest(new ResponseModel
                {
                    status = false,
                    result = "Organization Already Exists with this Email"
                });

            //generate company id
            string companyID = "CID" + RandomString(4);

            //unique check
            bool notUnique = false;
            do
            {                
                notUnique = await _context.Organizations.AnyAsync(e => e.CompanyId == companyID, cancellationToken);
                if (notUnique)
                {
                    companyID = "CID" + RandomString(4);
                }
            }
            while (notUnique) ;

            //create entity
            var entity = new Organization()
            {
                CompanyId = companyID,
                ImageUrl = createVM.ImageUrl,
                Name = createVM.Name,
                Email = createVM.Email,
                Number = createVM.Number,
                ContactName = createVM.ContactName,
                Employees = createVM.TotalEmployees
            };
            _context.Organizations.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrganizationAsync([FromQuery] int orgId, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Organizations
                 .Where(x => x.Id == orgId)
                 .Select(e => new OrganizationCreateVM
                 {
                     ImageUrl = e.ImageUrl,
                     Name = e.Name,
                     Email = e.Email,
                     Number = e.Number,
                     ContactName = e.ContactName,
                     TotalEmployees = e.Employees
                 })
                 .FirstAsync(cancellationToken);

            //return new JsonResult(query);
            return Ok(entity);
        }
        catch (Exception ex)
        {
            //throw new Exception(ex.GetBaseException().Message);
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateOrganizationAsync([FromBody] OrganizationEditVM editVM, CancellationToken token)
    {
        try
        {

            var org = await _context.Organizations.Where(x => x.Id == editVM.Id).FirstOrDefaultAsync(token);

            if (org == null)
                return BadRequest(new ResponseModel { status = false, result = "Organization Not Found" });

            org.ImageUrl = editVM.ImageUrl;
            org.Name = editVM.Name;
            org.Email = editVM.Email;
            org.Number = editVM.Number;
            org.ContactName = editVM.ContactName;
            org.Employees = editVM.TotalEmployees;

            await _context.SaveChangesAsync(token);

            return Ok(new ResponseModel { status = true, result = "success" });

            //if(editVM.TotalEmployees < org.Employees)
            //{
            //    return BadRequest(new ResponseModel { status = false, result = "You can only increase the strength" });
            //}
            //else
            //{
            //    org.ImageUrl = editVM.ImageUrl;
            //    org.Name = editVM.Name;
            //    org.Email = editVM.Email;
            //    org.Number = editVM.Number;
            //    org.ContactName = editVM.ContactName;
            //    org.Employees = editVM.TotalEmployees;

            //    await _context.SaveChangesAsync(token);

            //    return Ok(new ResponseModel { status = true, result = "success" });
            //}
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
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
    public async Task<IActionResult> GetCompaniesForChefAsync([FromQuery] string? orgName, CancellationToken cancellationToken)
    {
        try
        {
            var companies = await _context.Organizations
                .Where(e => !e.IsDeleted && e.ChefId == null)
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

    [HttpPost]
    public IActionResult CompanyMealsInter([FromBody] int companyId )
    {
        //stores companyIds in cookies
        TempData["companyId"] = companyId;

        //stores companyNamesSring in cookies
        var compName = _context.Organizations.Where(e => e.Id == companyId)
            .Select(e => e.Name)
            .FirstOrDefault();

        //string companyName = compName.ToString();

        //string companyNamesSring = string.Join(", ", compNames);

        TempData["companyNamesSring"] = compName;

        return new JsonResult("success");
    }

    [HttpGet]
    public IActionResult CompanyMeals()
    {
        //var a = TempData["companyIds"];
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMealTypesAsync( CancellationToken cancellationToken)
    {
        try
        {
            var mealTypes = await _context.MealType
            .Where(e => !e.IsDeleted && e.CreatedByType == Domain.Enums.UserType.Admin)
            .Select(e => new MealTypeGetVM
            {
                Id = e.Id,
                PictureUrl = e.PictureUrl,
                Name = e.Name,
                StartTime = e.StartTime.ToString("hh:mm tt"),
                EndTime = e.EndTime.ToString("hh:mm tt"),
            })
            .ToListAsync(cancellationToken);

            return Ok(mealTypes);
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
            ////get chef's company Id
            //int companyId = await _context.Organizations.Where(e => e.ChefId == _currentUser.UserId)
            //    .Select(e => e.Id)
            //    .FirstAsync(cancellationToken);

            if (mealVM.mealTypeIds != null && mealVM.mealTypeIds.Count > 0)
            {
                var executeRequest = true;
                foreach (var mealtype in mealVM.mealTypeIds)
                {
                    //check
                    var existOrgMeal = await _context.OrgMeals.Where(e => e.WeekDay == (WeekDays)mealVM.WeekDay && e.OrganizationId == mealVM.companyId &&
                        e.MealTypeId == mealtype)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existOrgMeal != null)
                    {
                        return BadRequest(new ResponseModel { status = false, result = "Meal already exists. Please select a different Meal" });
                        executeRequest = false;
                    }

                    if (executeRequest == true)
                    {
                        //get mealType
                        var meal = await _context.MealType.Where(e => e.Id == mealtype).FirstAsync(cancellationToken);

                        var orgMeal = new OrgMeal
                        {
                            MealTypeId = mealtype,
                            OrganizationId = mealVM.companyId,
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

            var addmeal = _context.OrgMeals.Where(x => x.WeekDay == (WeekDays)mealVM.WeekDay && x.OrganizationId == mealVM.companyId).Select(u => new OrganizationsMealsVM
            {
                id = u.Id,
                Name = u.MealType.Name,
                StartTime = u.MealType.StartTime.ToString("hh:mm tt"),
                EndTime = u.MealType.EndTime.ToString("hh:mm tt"),
                PictureUrl = u.MealType.PictureUrl,
                WeekDay = u.WeekDay,
                
            }).ToList();

            return Ok(new ResponseModel { status = true, result = addmeal });

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }



    [HttpPost]
    public IActionResult CompanyMealItemsInter([FromBody] mealTypeVM mealVM)
    {
        //stores orgMeal data in cookies
        //TempData["mealTypeId"] = mealVM.mealTypeId;
        TempData["orgMealId-edit"] = mealVM.orgMealId;
        TempData["companyId"] = mealVM.companyId;

        return new JsonResult("success");
    }

    [HttpGet]
    public IActionResult CompanyMealItems()
    {
        //var a = TempData["companyIds"];
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMealCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            //gets mealTypeId from cookies
            var mealTypeIdObj = TempData.Peek("orgMealId-edit");
            int orgMealId = Convert.ToInt32(mealTypeIdObj);
            var mealTypeId = _context.OrgMeals.Where(x => x.Id == orgMealId).Select(x => x.MealTypeId).FirstOrDefault();
            
            var mealId = _context.OrgMeals.Where(x => x.Id == mealTypeId).Select(x => x.MealTypeId).FirstOrDefault();

            var cats = await _context.MealCategories
                .Where(e => e.MealTypeId == mealId)
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
    public async Task<IActionResult> GetAllItemsAsync(CancellationToken cancellationToken)
    {
        try
        {

            var cats = await _context.Items
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.Name)
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

    [HttpPost]
    public async Task<IActionResult> OrgMealsCreateAsync([FromBody] List<SelectedItemCreateVM> createVM, CancellationToken cancellationToken)
    {
        try
        {
            //getting values from cookies
            var companyIdsObj = TempData.Peek("companyId");
            //var weekDayObj = TempData.Peek("weekDay");
            var mealTypeIdObj = TempData.Peek("mealTypeId");
            //var startTimeObj = TempData.Peek("startTime");
            //var endTimeObj = TempData.Peek("endTime");
            //TempData["companyNamesSring"] = companyNamesSring;
            //TempData["mealTypeName"] = mealVM.MealTypeName;

            //converting values
            int companyIdsArray = Convert.ToInt32(companyIdsObj);
            //int weekDay = Convert.ToInt32(weekDayObj);
            int mealTypeId = Convert.ToInt32(mealTypeIdObj);
            //TimeSpan startTime = TimeSpan.Parse(startTimeObj.ToString());
            //TimeSpan endTime = TimeSpan.Parse(endTimeObj.ToString());

            var isExist = await _context.OrgMeals.AnyAsync(e =>
                e.MealTypeId == mealTypeId,
                cancellationToken);

            if (!isExist)
            {
                var orgMeal = _context.OrgMeals.Where(x => x.MealTypeId == mealTypeId).Select(x => x.Id).FirstOrDefault();

                //creates orgMeal categories
                var categories = await _context.MealCategories.Where(e => e.MealTypeId == mealTypeId)
                    .ToListAsync(cancellationToken);
                foreach (var category in categories)
                {
                    var orgMealCategory = new OrgMealCategory
                    {
                        Name = category.Name,
                        ImageUrl = category.ImageUrl,
                        CreatedDate = DateTime.UtcNow,
                        OrgMealId = orgMeal
                    };
                    _context.OrgMealCategories.Add(orgMealCategory);

                    //create items relations
                    foreach (var item in createVM)
                    {
                        if (Convert.ToInt32(item.CategoryId) == category.Id)
                        {
                            var orgMealItem = new OrgMealCatAndOrgMealitem
                            {
                                OrgMealCategoryId = orgMealCategory.Id,
                                ItemId = Convert.ToInt32(item.ItemId)
                            };
                            orgMealCategory.OrgMealCatAndOrgMealitems.Add(orgMealItem);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    [HttpGet]
    public IActionResult CompanyMealsEditInter([FromQuery] int companyId)
    {
        TempData["companyId-edit"] = companyId;

        string companyName = _context.Organizations.Single(e => e.Id == companyId).Name;
        TempData["companyName-edit"] = companyName;

        return RedirectToAction("CompanyMealsEdit");
    }

    [HttpGet]
    public IActionResult CompanyMealsEdit()
    {

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetOrgMealsAsync([FromQuery] int weekDay, CancellationToken cancellationToken)
    {
        try
        {
            int companyId = Convert.ToInt32(TempData.Peek("companyId"));

            var orgMeals = await _context.OrgMeals
                .Where(e => e.WeekDay == (WeekDays)weekDay && e.OrganizationId == companyId )
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
            int companyId = Convert.ToInt32(TempData.Peek("companyId"));

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
                e.WeekDay == (WeekDays) mealVM.WeekDay)
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

                //create notification
                await CreateTimeEditNotificationAsync(orgMeal, cancellationToken);

                //commit changes to DB
                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new ResponseModel { status = true, result = mealVM });
            }

        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    private async Task CreateTimeEditNotificationAsync(OrgMeal orgMeal, CancellationToken cancellationToken)
    {
        //get meal
        var meal = await _context.MealType.Where(e => e.Id == orgMeal.MealTypeId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("MealType not found");

        string description = $"Time schedule for {meal.Name} has been updated for {orgMeal.WeekDay.ToString()}. New" +
            $" time will be {orgMeal.StartTime.ToString("hh:mm tt")} - {orgMeal.EndTime.ToString("hh:mm tt")}. ";

        //gets all company users
        int companyId = Convert.ToInt32(TempData.Peek("companyId-edit"));
        string compId = await _context.Organizations.Where(e => e.Id == companyId)
            .Select(e => e.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        var compUsers = await _context.Users.Where(e => e.OrganizationId == compId &&
            e.IsUserBlocked == false &&
            e.UserApprovalStatus == UserApprovalStatusEnum.Approved)
            .ToListAsync(cancellationToken);

        foreach (var user in compUsers)
        {
            var notification = new Notification
            {
                Description = description,
                CreatedDate = DateTime.UtcNow,
                NotifType = NotificationType.TimeNotif,
                NotifiedById = _currentUser.UserId,
                NotifiedToId = user.Id
            };
            _context.Notifications.Add(notification);
        }

        //commit to DB
        //await _context.SaveChangesAsync(cancellationToken);

    }

    [HttpPost]
    public async Task<IActionResult> RemoveOrgMealItemAsync([FromBody] OrgMealItems mealItems, CancellationToken cancellationToken)
    {
        try
        {
            var mealcategory = await _context.OrgMealCatAndOrgMealitems.Where(u => u.ItemId == mealItems.itemId && u.OrgMealCategoryId == mealItems.categoryId).FirstOrDefaultAsync(cancellationToken);

            if (mealcategory == null)
                return BadRequest(new ResponseModel { status = false, result = "Meal Menu Not Found" });

            ///////////////////////////////////////////////////////////
            ///CANT DELETE TODAY MEAL//////////////////////////////////
            ///////////////////////////////////////////////////////////

            //deleting room and all relevant entities
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
        //TempData["weekDay-edit"] = mealVM.WeekDay;

        TempData["mealTypeName-edit"] = mealVM.MealTypeName;
        TempData["companyId-edit"] = mealVM.companyId;
        return new JsonResult("success");
    }

    [HttpGet]
    public IActionResult CompanyMealItemsEdit()
    {
        //var a = TempData["companyIds"];
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
            int companyId = Convert.ToInt32(TempData.Peek("companyId-edit"));
            //var mealCatIds = updateVM.Select(e => Convert.ToInt32(e.CategoryId) ).Distinct().ToList();

            ////deletes existing selected items
            //foreach(int catId in mealCatIds)
            //{
            //    var catItems = await _context.OrgMealCatAndOrgMealitems.Where(e => e.OrgMealCategoryId == catId)
            //        .ToListAsync(cancellationToken);

            //    _context.OrgMealCatAndOrgMealitems.RemoveRange(catItems);
            //}

            // creates new slect relations
            foreach(var item in updateVM)
            {
                var catItem = new OrgMealCatAndOrgMealitem
                {
                    ItemId = Convert.ToInt32(item.ItemId),
                    OrgMealCategoryId = Convert.ToInt32(item.CategoryId)
                };
                _context.OrgMealCatAndOrgMealitems.Add(catItem);
            }

            //create notification
            await CreateMenuUpdateNotificationAsync(cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ResponseModel { status = true, result = "success" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { status = false, result = ex.GetBaseException().Message });
        }
    }

    private async Task CreateMenuUpdateNotificationAsync(CancellationToken cancellationToken)
    {

        string description = "We've updated the menu for this week. Check it out and order something new.";

        //gets all company users
        int companyId = Convert.ToInt32(TempData.Peek("companyId-edit"));
        string compId = await _context.Organizations.Where(e => e.Id == companyId)
            .Select(e => e.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        var compUsers = await _context.Users.Where(e => e.OrganizationId == compId &&
            e.IsUserBlocked == false &&
            e.UserApprovalStatus == UserApprovalStatusEnum.Approved)
            .ToListAsync(cancellationToken);

        foreach (var user in compUsers)
        {
            var notification = new Notification
            {
                Description = description,
                CreatedDate = DateTime.UtcNow,
                NotifType = NotificationType.MenueNotif,
                NotifiedById = _currentUser.UserId,
                NotifiedToId = user.Id
            };
            _context.Notifications.Add(notification);
        }

        //commit to DB
        //await _context.SaveChangesAsync(cancellationToken);

    }

    //************ Copy Meals

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
                .Select(e => (int)e.WeekDay)
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

            foreach (int weekDay in mealVM.WeekDays)
            {
                //create new orgMeal
                var newOrgMeal = new OrgMeal
                {
                    MealTypeId = orgMeal.MealTypeId,
                    OrganizationId = orgMeal.OrganizationId,
                    WeekDay = (WeekDays)weekDay,
                    StartTime = orgMeal.StartTime,
                    EndTime = orgMeal.EndTime,
                };
                _context.OrgMeals.Add(newOrgMeal);

                //create orgMealCategories
                foreach (var cat in orgMeal.OrgMealCategories)
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
                    foreach (var itemRelation in cat.OrgMealCatAndOrgMealitems)
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
        string path_1 = Path.Combine(@"wwwroot", "Images");
        string uploadsFolder = Path.Join(Directory.GetCurrentDirectory(), path_1);
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
        string uniqueFileName = Guid.NewGuid().ToString() + extension;
        string filePath = Path.Join(uploadsFolder, uniqueFileName);
        using (var _fileStream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(_fileStream);

        return BaseUrl() + "/Images/" + uniqueFileName;
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
