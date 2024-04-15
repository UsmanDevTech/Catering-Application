using Application.Accounts.Commands;
using Application.Accounts.Queries;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Extensions;
using Domain.Contracts;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static Domain.Contracts.ResponseKey;
using Domain.Common;
using Application.Common.Extensions;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;

namespace Infrastructure.Services
{
    public class IdentityService:IidentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenProvider _jwtToken;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _applicationDbcontext;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;
        private readonly IDateTime _dateTime;

        public IFormatProvider provider { get; private set; }


        public IdentityService(UserManager<ApplicationUser> userManager, IDateTime dateTime, IEmailService emailService,ITokenProvider jwtToken, IConfiguration configuration, ICurrentUserService currentUser,
        ApplicationDbContext applicationDbcontext, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager/*, IEmailService emailService*/)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration= configuration;
            _applicationDbcontext = applicationDbcontext;
            _jwtToken= jwtToken;
            _currentUser=currentUser;
            _emailService = emailService;
            _dateTime = dateTime;
        }


        public async Task<ResponseKeyContract> AuthenticateUserAsync(LoginQuery request)
        {
            //Get User Detail From database and check user exist 
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == request.email.Trim());
            if (user == null)
                throw new NotFoundException($"User does not exist");

           //// Checking if account deleted
           // if (user.RequestForPermanentDelete == true)
           //     throw new NotFoundException("Error! account is in deletion process.");


            //Checking Password
            if (!(await _userManager.CheckPasswordAsync(user, request.password)))
                throw new NotFoundException("Invalid password! try again with new password.");
            if (user.IsUserBlocked==true)
                throw new NotFoundException($"User is blocked by Admin");

            //if (user.UserApprovalStatus == UserApprovalStatusEnum.Pending)
            //    throw new NotFoundException("User Approval Status is Pending");

            //if(user.UserApprovalStatus==UserApprovalStatusEnum.Rejected)
            //    throw new NotFoundException("User Approval Status is Rejected");

            //organization block check
            string orgId = await _applicationDbcontext.Users.Where(u => u.Id == user.Id)
                .Select(u => u.OrganizationId)
                .SingleAsync();

            bool isDeleted = await _applicationDbcontext.Organizations
                .Where(e => e.CompanyId == orgId)
                .Select(e => e.IsDeleted)
                .SingleAsync();

            if (isDeleted)
                throw new NotFoundException("Organization has been blocked by Admin");


            var result = await _signInManager.PasswordSignInAsync(user, request.password, true, false);

            //if (result.IsLockedOut)
            //    throw new CustomInvalidOperationException("This account is locked out");

            //update user
            user.TimezoneId = request.timeZoneId;
            user.fcmToken = request.fcmToken;
            await _userManager.UpdateAsync(user);

            //Create Jwt token for Authenticate
            DateTime expiry = DateTime.UtcNow.AddDays(20);
            var accessToken = _jwtToken.CreateToken(new JwtUserContract { email = user.Email, id = user.Id, userName = user.UserName, loginRole = (int)user.LoginRole }, expiry);

            //Return User Contract
            return new ResponseKeyContract
            {
                key = accessToken,
                emailConfirmationRequired = user.EmailConfirmed == false ? true : false,
                UserApprovalStatus = user.UserApprovalStatus,
                RejectionReason = user.RejectionReason
            };

        }

        public async Task<Result> Register([FromBody] RegisterUserCommand registerUser, CancellationToken cancellationToken)
        {
            // check existing user
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null)
            {
                throw new UserAlreadyExistsException();
                // return StatusCodeResult(new ResponseKey { Status = StatusCodes.Status412PreconditionFailed, Message = "User already exist" });
                //return new ResponseKey { Status = StatusCodes.Status302Found, Message = "User is already exist" };
            }
            var CheckCompany = await _applicationDbcontext.Organizations.Where(x => x.CompanyId == registerUser.OrganizationId).FirstOrDefaultAsync();
            if (CheckCompany == null) 
            {
                throw new UserAlreadyExistsException($"Company Does not exist with this id: {registerUser.OrganizationId} ");
            }
            //add user 
            ApplicationUser user = new()
            {
                OrganizationId = registerUser.OrganizationId,
                Email = registerUser.Email,
                UserName = registerUser.Email,
                FullName = registerUser.FullName,
                UserApprovalStatus = UserApprovalStatusEnum.Pending,
                //ImageUrl = registerUser.ImageUrl,
                // Password = registerUser.Password,
                LoginRole = UserType.User,
            };
            foreach (var alergy in registerUser.Allergy)
            {
                var alergyName = new Allergy
                {
                    Name = alergy,
                    UserId=user.Id,
                };
               await _applicationDbcontext.Allergies.AddAsync(alergyName);
               
            }

            await CreateApprovalNotificationAsync(user, cancellationToken);

            var result = await _userManager.CreateAsync(user, registerUser.Password);// if duplicate name, dont create user
            if (!result.Succeeded)
            {
                throw new Exception($"New user could not be created ({result.ToString()})");
                //return new ResponseKeyContract { Status = StatusCodes.Status412PreconditionFailed, Message = "User Creation Failed" };
            }

            return (result.ToApplicationResult());
            //return new ResponseKeyContract { Status = StatusCodes.Status201Created, Message = "User created Successfully" };
        }

        public async Task<Result> RequestForAccountDeleteAsync(string password)
        {
            // Get the existing user from the database
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);

            //thrown error if user not found
            if (user != null)
            {
                var ispasswordvalid = await _userManager.CheckPasswordAsync(user, password);
               // var isPasswordValid =await _userManager.Users.Where(x=>x.Password==user.Password).FirstOrDefaultAsync();
                if (!ispasswordvalid)
                {
                    throw new CustomInvalidOperationException();
                }
                else
                {                   
                    //deleting allergy
                    var deleteAllergies = await _applicationDbcontext.Allergies.Where(x => x.UserId == user.Id).ToListAsync();
                    _applicationDbcontext.Allergies.RemoveRange(deleteAllergies);

                    //deleting cart
                    var getCart= await _applicationDbcontext.Carts.Where(x=>x.UserId==user.Id).FirstOrDefaultAsync();
                    if (getCart != null)
                    {
                        _applicationDbcontext.Carts.Remove(getCart); 
                    }
                    //deleting orders
                    var getOrders = await _applicationDbcontext.Orders.Where(x => x.UserId == user.Id).ToListAsync();
                    _applicationDbcontext.Orders.RemoveRange(getOrders);

                    //deleting notifications
                    var nots = await _applicationDbcontext.Notifications.Where(e => e.NotifiedById ==  user.Id
                        || e.NotifiedToId == user.Id).ToListAsync();

                    _applicationDbcontext.Notifications.RemoveRange(nots);

                    //deleting user
                    var result = _applicationDbcontext.Users.Remove(user);

                    await _applicationDbcontext.SaveChangesAsync();
                    
                    return Result.Success();
                }
               
            }
            else
            {
                throw new NotFoundException("user does not exist");
            }
           
        }
        public async Task<Result> SendEmailOTPAsync(SendEmailOtpCommand request, CancellationToken token)
        {
            //Init application user entity to null
            if (!request.newEmail.Equals("#"))
            {
                var IsUnique = await BeUniqueEmailAsync(request.newEmail.Trim(), token);
                if (!IsUnique)
                    throw new NotFoundException(InvalidOperationErrorMessage.AlreadyExistsErrorMessage("User", "replacement email"));
            }

            //Fetch user info
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == request.existingEmail.Trim(), token);
            if (user == null)
                throw new NotFoundException($"User with {request.existingEmail.Trim()} does not exist");


            //Remove all previous otp code belongs to this user
            _applicationDbcontext.OTPhistories.RemoveRange(await _applicationDbcontext.OTPhistories.Where(u => u.CreatedBy == user.Id).ToListAsync(token));

            Random rnd = new Random();
            var otp = rnd.Next(111111, 999999);

            //Send otp to email
            _emailService.SendEmail(request.existingEmail, otp);

            var expiryDate = _dateTime.NowUTC;
            OTPhistory accountOtpCode = OTPhistory.Create(otp.ToString(), expiryDate.AddDays(1), OTPmediaTypeEnum.Email, request.newEmail, request.existingEmail, user.Id);

            await _applicationDbcontext.OTPhistories.AddAsync(accountOtpCode);
            await _applicationDbcontext.SaveChangesAsync(token);
            return Result.Success();
        }
        public async Task<Result> LogoutAsync()
        {
            // Get the existing user from the database
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);

            //thrown error if user not found
            if (user == null)
                throw new NotFoundException("user does not exist");

            user.fcmToken = null;

            // Apply the changes if any to the db
            var result = await _userManager.UpdateAsync(user);
            return result.ToApplicationResult();
        }
        public async Task<bool> BeUniqueEmailAsync(string email, CancellationToken token)
        {
            var emailExist = await _userManager.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (emailExist != null)
            {
                if (emailExist.RequestForPermanentDelete == true)
                    throw new NotFoundException("Error! account is in deletion process.");
                else
                    return false;
            }

            return true;
        }
        public async Task<ResponseKeyContract> ConfirmEmailAsync(ConfirmEmailCommand info, CancellationToken cancellationToken)
        {
            //Check if user does'nt exists
            var user = await _userManager.FindByEmailAsync(info.email.Trim());
            if (user == null)
                throw new NotFoundException($"User does not exist");

            //Check if otp does'nt exist
            var userOtp = await _applicationDbcontext.OTPhistories.FirstOrDefaultAsync(u => u.CreatedBy == user.Id && u.OtpMediaType == OTPmediaTypeEnum.Email && u.Token == info.otp.Trim(), cancellationToken);
            if (userOtp == null)
                throw new CustomInvalidOperationException("OTP Code not found");

            //Check if otp is expired
            if (_dateTime.NowUTC > userOtp.ExpireDateTime)
                throw new CustomInvalidOperationException("OTP code has expired");

            string newEmail = "";

            if (!userOtp.ReplacementValue.Equals("#"))
            {
                //Check if email already exist or not
                var IsUnique = await BeUniqueEmailAsync(userOtp.ReplacementValue, cancellationToken);
                if (!IsUnique)
                    throw new CustomInvalidOperationException(InvalidOperationErrorMessage.AlreadyTakenErrorMessage("Email", userOtp.ReplacementValue));
                newEmail = userOtp.ReplacementValue;
            }
            else
            {
                newEmail = user.Email;
            }
            //Change email
            var validToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var result = await _userManager.ChangeEmailAsync(user, newEmail, validToken);
            //Remove all previous otp code belongs to this user
            if (result.Succeeded)
            {
                _applicationDbcontext.OTPhistories.RemoveRange(await _applicationDbcontext.OTPhistories.Where(u => u.CreatedBy == user.Id).ToListAsync(cancellationToken));
                await _applicationDbcontext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new CustomInvalidOperationException(result.Errors.ToString() ?? "email not confirmed");
            }
            //Create Jwt token for Authenticate
            DateTime expiry = DateTime.UtcNow.AddDays(20);
            var accessToken = _jwtToken.CreateToken(new JwtUserContract { email = user.Email, id = user.Id, userName = user.UserName, loginRole = (int)user.LoginRole }, expiry);

            //Return User Contract
            return new ResponseKeyContract
            {
                key = accessToken,
                emailConfirmationRequired = user.EmailConfirmed == false ? true : false,
                UserApprovalStatus = user.UserApprovalStatus,
                RejectionReason = user.RejectionReason
            };
        }
        public async Task<(Result Result, string UserId)> ResetPasswordViaEmailAsync(ResetPasswordViaEmailCommand request)
        {
            ApplicationUser user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == request.email.Trim());

            //Check if user does'nt exists
            if (user == null)
                throw new NotFoundException($"User does not exist");

            //Authentication
            if (request.resetOption == (int)ResetPasswordOptionEnum.Otp)
            {
                //Check if otp does'nt exist
                var userOtp = await _applicationDbcontext.OTPhistories.FirstOrDefaultAsync(u => u.CreatedBy == user.Id && u.OtpMediaType == OTPmediaTypeEnum.Email && u.Token == request.resetValue.Trim());
                if (userOtp == null)
                    throw new CustomInvalidOperationException(InvalidOperationErrorMessage.otp_code_not_found);

                //Check if otp is expired
                if (_dateTime.NowUTC > userOtp.ExpireDateTime)
                    throw new CustomInvalidOperationException(InvalidOperationErrorMessage.otp_code_has_expired);
            }
            else
            {
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.resetValue);
                if (!isPasswordValid)
                    throw new CustomInvalidOperationException(InvalidOperationErrorMessage.invalid_password);
            }


            //Change password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, token, request.password);
            //Return Result
            return (resetPassResult.ToApplicationResult(), user.Id);
        }
        public async Task<UserProfileContract>GetUserProfileAsync(GetAccountProfileQuery request)
        {
            string userId = "";

            if (string.IsNullOrEmpty(request.userId))
                userId = _currentUser.UserId;
            else
                userId = request.userId;

            var user = await _userManager.FindByIdAsync(userId);
            if (user==null)
            {
                throw new NotFoundException($"User does not exist");
            }

            return new UserProfileContract
            {
                Id=user.Id,
                FullName = user.FullName,
                Email=user.Email,
                ImageUrl=user.ImageUrl,
                OrganizationId = user.OrganizationId,
                Allergies = _applicationDbcontext.Allergies.Where(e => e.UserId == userId)
                    .Select(e => e.Name)
                    .ToList(),
                UserApprovalStatus = user.UserApprovalStatus,
                RejectionReason = user.RejectionReason,
                LoginRole = user.LoginRole,
                fcmToken = user.fcmToken,
                IsUserBlocked = user.IsUserBlocked
            };
        }
        public async Task<Result>UpdateProfileDetailAsync(UpdateProfileCommand request, CancellationToken token)
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if (user==null)
            {
                throw new NotFoundException("User not Found");
            }

            var CheckCompany = await _applicationDbcontext.Organizations.Where(x => x.CompanyId == request.companyId).FirstOrDefaultAsync();
            if (CheckCompany == null)
            {
                throw new UserAlreadyExistsException($"Company Does not exist");
            }

            if (!string.IsNullOrEmpty(request.companyId))
                user.OrganizationId = request.companyId;

            if (!string.IsNullOrEmpty(request.fullName))
                user.FullName = request.fullName;

            if(!string.IsNullOrEmpty(request.imageUrl))
                user.ImageUrl = request.imageUrl;

            //to remove existing alergy
            var userAlergies = await _applicationDbcontext.Allergies.Where(x => x.UserId == user.Id).ToListAsync();
            foreach (var alergy in userAlergies)
            {
                if (alergy != null)
                    _applicationDbcontext.Allergies.Remove(alergy);
            }
            
            //adding new allergies
            if (request.allergy!=null)
            {
                foreach (var alergy in request.allergy)
                {                

                    var alergyName = new Allergy
                    {
                        Name = alergy.ToString(),
                        UserId = user.Id,
                    };
                    await _applicationDbcontext.Allergies.AddAsync(alergyName);

                }
            }

            //approval status
            if(user.UserApprovalStatus == UserApprovalStatusEnum.Rejected)
            {
                user.UserApprovalStatus = UserApprovalStatusEnum.Pending;

                //add notification
                await CreateResubmitApprovalNotificationAsync(user, token);
            }

            var result = await _userManager.UpdateAsync(user);

            return result.ToApplicationResult();

        }

        public async Task<Result>AddToCartAsync(AddToCartCommand request, CancellationToken cancellationToken)
        {
            // Get the existing user from the database
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if (user == null)
                throw new NotFoundException("user does not exist");

            //******* Organization checks
            var org = await _applicationDbcontext.Organizations
                .Where(e => e.CompanyId == user.OrganizationId)
                .Select(e => new
                {
                    e.IsDeleted,
                    e.ChefId
                })
                .SingleOrDefaultAsync()
                ?? throw new NotFoundException("Organization does not exist");

            //organization block check
            if (org.IsDeleted)
                throw new NotFoundException("Organization has been blocked by Admin");

            //chef assigned check
            if (org.ChefId == null)
                throw new NotFoundException("No Chef assigned to company");


            //gets and checks orgMeal
            var orgMeal = await _applicationDbcontext.OrgMeals.Where(e => e.Id == request.OrgMealId)
                .Select(e => new
                {
                    Id = e.Id,
                    MealName = e.MealType.Name
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Organization Meal does not exist");

            //checks if there is an order with same orgMeal in current day
            var startDateTime = DateTime.UtcNow.UtcToLocal(user.TimezoneId).Date.LocalToUtc(user.TimezoneId);
            var endDateTime = startDateTime.AddDays(1);
            bool isExist = await _applicationDbcontext.Orders
                .AnyAsync(e => e.CreatedAt >= startDateTime &&
                e.CreatedAt < endDateTime && e.OrgMealId == request.OrgMealId &&
                e.UserId == _currentUser.UserId,
                cancellationToken);
            if (isExist) throw new Exception($"{orgMeal.MealName} meal is already ordered.");

            //checks currently allowed orgMeal
            var currentDayOfWeek = (int)DateTime.UtcNow.UtcToLocal(user.TimezoneId).DayOfWeek;
            var currentTime = DateTime.UtcNow.UtcToLocal(user.TimezoneId).TimeOfDay;
            int orgId = await GetUserOrganizationId(_currentUser.UserId, cancellationToken);

            var orgMeals = await _applicationDbcontext.OrgMeals.Where(e => e.OrganizationId == orgId &&
                e.WeekDay == (WeekDays)currentDayOfWeek)
                .Select(e => new
                {
                    e.MealType.Name,
                    e.StartTime,
                    e.EndTime,
                    e.Id
                })
                .OrderBy(e => e.StartTime.TimeOfDay)
                .ToListAsync(cancellationToken);

            var allowedOrgMeal = orgMeals.FirstOrDefault(e => e.EndTime.TimeOfDay >= currentTime)
                ?? throw new Exception("You can not order any Meal at this time.");

            //if (request.OrgMealId != allowedOrgMeal.Id)
            //    throw new Exception($"You can only order {allowedOrgMeal.Name} meal at this time.");

            //gets and removes previous cart and cartItems
            var getCart = _applicationDbcontext.Carts.Where(x => x.UserId == user.Id).SingleOrDefault();// it check if record is singel and return single record
            if (getCart != null)
            {
                _applicationDbcontext.Carts.Remove(getCart);
            }

            //adding cart and cart items
            var cart = new Cart
            {
                CreatedDate = DateTime.UtcNow,
                UserId = user.Id,
                OrgMealId = request.OrgMealId
            };
            foreach (var itemData in request.CartItems)
            {
                var cartItem = new CartItem
                {
                    CategoryName = itemData.Category,
                    CategoryImage = itemData.CategoryImage,
                    ItemName = itemData.ItemName,
                    Cart = cart // Assuming you have a reference to the cart object
                };
                await _applicationDbcontext.CartItems.AddAsync(cartItem);
            }
            await _applicationDbcontext.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<GetCartContract> GetCartAsync(GetCartQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User does not exist");
            }

            //if(user.IsUserBlocked == true)
            //{
            //    throw new 
            //}

            //get cart
            var cart = await _applicationDbcontext.Carts
                .Where(x => x.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                throw new NotFoundException($"Cart is empty");
                //return StatusCodes();
            }

            //checks and removes cart if it is from a previous day
            if(cart.CreatedDate.UtcToLocal(user.TimezoneId).Date < DateTime.UtcNow.UtcToLocal(user.TimezoneId).Date )
            {
                _applicationDbcontext.Carts.Remove(cart);
                await _applicationDbcontext.SaveChangesAsync(cancellationToken);
                throw new NotFoundException($"Cart is empty"); ;
            }

            //check currently allowed orgMeal and removes cart if it's for a different orgMeal
            var currentDayOfWeek = (int)DateTime.UtcNow.UtcToLocal(user.TimezoneId).DayOfWeek;
            var currentTime = DateTime.UtcNow.UtcToLocal(user.TimezoneId).TimeOfDay;
            int orgId = await GetUserOrganizationId(_currentUser.UserId, cancellationToken);

            var orgMeals = await _applicationDbcontext.OrgMeals.Where(e => e.OrganizationId == orgId &&
                e.WeekDay == (WeekDays)currentDayOfWeek)
                .Select(e => new
                {
                    e.MealType.Name,
                    e.StartTime,
                    e.EndTime,
                    e.Id
                })
                .OrderBy(e => e.StartTime.TimeOfDay)
                .ToListAsync(cancellationToken);

            var allowedOrgMeal = orgMeals.FirstOrDefault(e => e.EndTime.TimeOfDay >= currentTime);

            if (allowedOrgMeal == null) //when current time has exceeded last day of meal
            {
                _applicationDbcontext.Carts.Remove(cart);
                await _applicationDbcontext.SaveChangesAsync(cancellationToken);
                throw new NotFoundException($"Cart is empty");
            }

            //if (cart.OrgMealId != allowedOrgMeal.Id)
            //{
            //    _applicationDbcontext.Carts.Remove(cart);
            //    await _applicationDbcontext.SaveChangesAsync(cancellationToken);
            //    throw new NotFoundException($"Cart is empty"); ;
            //}            

            //get cart items
            var cartItems = await _applicationDbcontext.CartItems
                .Where(x => x.CartId == cart.Id)
                .ToListAsync();

            if (cartItems == null)
            {
                //throw new NotFoundException($"CartItems not found");
                return new GetCartContract
                {
                    Id = cart.Id,
                    CreatedDate = cart.CreatedDate,
                };
            }

            //get orgMeal
            var orgMeal = orgMeals.Single(e => e.Id == cart.OrgMealId);

            return new GetCartContract
            {
                Id = cart.Id,
                CreatedDate = cart.CreatedDate,
                OrgMealId = cart.OrgMealId,
                CartItems = cartItems.Select(ci => new CartItemContract
                {
                    ItemName = ci.ItemName,
                    Category = ci.CategoryName,
                    CategoryImage = ci.CategoryImage,
                }).ToList(),
                TimeSlots = GenerateTimeSlots(orgMeal.StartTime, orgMeal.EndTime, 30)
            };

        }

        public async Task<Result> PlaaceOrderAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            // gets and checks user
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if (user == null)
                throw new NotFoundException("User does not exist");

            //******* Organization checks
            var org = await _applicationDbcontext.Organizations
                .Where(e => e.CompanyId == user.OrganizationId)
                .Select(e => new
                {
                    e.IsDeleted,
                    e.ChefId
                })
                .SingleOrDefaultAsync()
                ?? throw new NotFoundException("Organization does not exist");

            //organization block check
            if (org.IsDeleted)
                throw new NotFoundException("Organization has been blocked by Admin");

            //chef assigned check
            if (org.ChefId == null)
                throw new NotFoundException("No Chef assigned to company");


            //gets and checks orgMeal
            var orgMeal = await _applicationDbcontext.OrgMeals.Where(e => e.Id == request.OrgMealId)
                .Select(e => new
                {
                    Id = e.Id,
                    MealName = e.MealType.Name
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Organization Meal does not exist");

            //checks if there is on order with same orgMeal in current day
            var startDateTime = DateTime.UtcNow.UtcToLocal(user.TimezoneId).Date.LocalToUtc(user.TimezoneId);
            var endDateTime = startDateTime.AddDays(1);

            bool isExist = await _applicationDbcontext.Orders
                .AnyAsync(e => e.CreatedAt >= startDateTime &&
                e.CreatedAt < endDateTime && e.OrgMealId == request.OrgMealId &&
                e.UserId == _currentUser.UserId,
                cancellationToken);
            if (isExist) throw new Exception($"{orgMeal.MealName} meal is already ordered.");

            //checks currently allowed orgMeal
            var currentDayOfWeek = (int)DateTime.UtcNow.UtcToLocal(user.TimezoneId).DayOfWeek;
            var currentTime = DateTime.UtcNow.UtcToLocal(user.TimezoneId).TimeOfDay;
            int orgId = await GetUserOrganizationId(_currentUser.UserId, cancellationToken);

            var orgMeals = await _applicationDbcontext.OrgMeals.Where(e => e.OrganizationId == orgId &&
                e.WeekDay == (WeekDays)currentDayOfWeek)
                .Select(e => new
                {
                    e.MealType.Name,
                    e.StartTime,
                    e.EndTime,
                    e.Id
                })
                .OrderBy(e => e.StartTime.TimeOfDay)
                .ToListAsync(cancellationToken);

            var allowedOrgMeal = orgMeals.FirstOrDefault(e => e.EndTime.TimeOfDay >= currentTime)
                ?? throw new Exception("You can not order any Meal at this time.");

            //if (request.OrgMealId != allowedOrgMeal.Id)
            //    throw new Exception($"You can only order {allowedOrgMeal.Name} meal at this time.");

            //generates OrderID
            string orderID = "WC" + RandomString(7);

            //adding order
            var order = new Order
            {
                OrderID = orderID,

                TimeSlot = request.TimeSlot,
                SpecialRequest = request.SpecialRequest,
                UserId = user.Id,
                Status = StatusEnum.Pending,
                CreatedAt = DateTime.UtcNow,

                MealName = orgMeal.MealName,
                OrgMealId = orgMeal.Id,
            };

            //adding orderItems
            foreach (var itemName in request.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    //OrderId = order.Id,
                    Category = itemName.Category,
                    CategoryImage = itemName.CategoryImage,
                    Name = itemName.ItemName,
                    Order = order,
                };
                _applicationDbcontext.OrderItems.Add(orderItem);
            }

            //get and remove cart and cartItems
            var getCart = _applicationDbcontext.Carts.Where(x => x.UserId == user.Id).SingleOrDefault();// it check if record is singel and return single record
            if (getCart != null)
            {
                _applicationDbcontext.Carts.Remove(getCart);
            }

            //add Notification for Chef
            await CreateOrderNotificationAsync(user, orgId, cancellationToken);

            await _applicationDbcontext.SaveChangesAsync();
            return Result.Success();
        }

        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task CreateOrderNotificationAsync(ApplicationUser user, int orgId, CancellationToken cancellationToken)
        {
            //gets chef of user's organization
            var organization = await _applicationDbcontext.Organizations.Where(e => e.Id == orgId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Organization does not exist");

            string description = $"{user.FullName} placed an order.";

            var notification = new Notification
            {
                NotifiedById = user.Id,
                NotifiedToId = organization.ChefId ?? "",
                CreatedDate = DateTime.UtcNow,
                NotifType = NotificationType.OrderNotif,
                Description = description
            };

            _applicationDbcontext.Notifications.Add(notification);
            //await _applicationDbcontext.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateApprovalNotificationAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var admin = await _applicationDbcontext.Users.Where(u => u.LoginRole == UserType.Admin)
                .FirstAsync(cancellationToken);

            string description = $"{user.FullName} has created a new account. Please approve or reject the account.";

            var notification = new Notification
            {
                NotifiedById = user.Id,
                NotifiedToId = admin.Id,
                CreatedDate = DateTime.UtcNow,
                NotifType = NotificationType.ApprovalNotif,
                Description = description
            };

            _applicationDbcontext.Notifications.Add(notification);
            //await _applicationDbcontext.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateResubmitApprovalNotificationAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var admin = await _applicationDbcontext.Users.Where(u => u.LoginRole == UserType.Admin)
                .FirstAsync(cancellationToken);

            string description = $"{user.FullName} has resubmitted approval request. Please approve or reject the account.";

            var notification = new Notification
            {
                NotifiedById = user.Id,
                NotifiedToId = admin.Id,
                CreatedDate = DateTime.UtcNow,
                NotifType = NotificationType.ApprovalNotif,
                Description = description
            };

            _applicationDbcontext.Notifications.Add(notification);
            //await _applicationDbcontext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetUserOrganizationId(string userId, CancellationToken cancellationToken)
        {
            string orgId = await _applicationDbcontext.Users.Where(u => u.Id == userId)
                .Select(u => u.OrganizationId)
                .SingleAsync(cancellationToken);

            int organizationId = await _applicationDbcontext.Organizations
                .Where(e => e.CompanyId == orgId)
                .Select(e => e.Id)
                .SingleAsync(cancellationToken);

            return organizationId;
        }

        public async Task<string> GetTimeZoneId(string userId, CancellationToken cancellationToken)
        {
            string timeZone = await _applicationDbcontext.Users.Where(u => u.Id == userId)
                .Select(u => u.TimezoneId)
                .SingleAsync(cancellationToken)
                ?? "Asia/Karachi";

            return timeZone;
        }

        public List<string> GenerateTimeSlots(DateTime startDateTime,  DateTime endDateTime, int slotDuration)
        {
            // Ensure the end time is after the start time
            if (endDateTime <= startDateTime)
                throw new ArgumentException("End time must be after the start time");

            List<string> timeSlots = new();

            var timeSpan = endDateTime - startDateTime;
            int numberOfSlots = (int)(timeSpan.TotalMinutes / slotDuration);

            // Generate time slots
            for (int i = 0; i <= numberOfSlots; i++)
            {
                var time = startDateTime.AddMinutes(i * slotDuration).ToString("h:mm tt");
                timeSlots.Add(time);
            }

            return timeSlots;
        }

        public async Task CheckUserExistAsync(string userId, CancellationToken cancellationToken)
        {
            bool isExist = await _applicationDbcontext.Users.AnyAsync(u => u.Id == userId, cancellationToken);

            if (!isExist) throw new NotFoundException("User does not exist in database");
        }


    }
}

