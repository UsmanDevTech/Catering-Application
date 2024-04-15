using AdminPanel.Models;
using AdminPanel.ViewModels;
using Application.Common.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AdminPanel.Controllers;

public class AccountsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;

    public AccountsController(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager, ICurrentUserService currentUser,
    IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _currentUser = currentUser;
        _emailService = emailService;
    }

    //gets the base url from the request
    public string BaseUrl()
    {
        string url = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
        return url;
    }


    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    //[ValidateAntiForgeryToken]  //To prevent CSRF (cross-site request forgery request) Attack
    public async Task<IActionResult> Login([FromQuery] string? ReturnUrl, LoginVM loginVM)
    {
        //if (!String.IsNullOrEmpty(ReturnUrl))
        //    ReturnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return View();

        //checks if user exists
        var user = await _userManager.FindByEmailAsync(loginVM.Email);

        if (user is null)
        {
            ModelState.AddModelError(nameof(loginVM.Email), "Invalid Email! User does not exist");
            return View();
        }

        //checks password
        var IsPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginVM.Password);

        if (!IsPasswordCorrect)
        {
            ModelState.AddModelError(nameof(loginVM.Password), "Incorrect Password.");
            return View();
        }

        //checks user's roles
        if (user.LoginRole != Domain.Enums.UserType.Admin)
        {
            TempData["Error"] = "The user is not Authorized to access Admin panel";
            return View();
        }

        ////SignIn
        //await signInManager.SignInAsync(user, isPersistent: loginVM.IsRemember);

        //SignIn
        var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password,
            loginVM.IsRemember, false);

        if (!result.Succeeded)
        {
            TempData["Error"] = $"Signin failed ({result.ToString()})";
            return View();
        }

        //if (Url.IsLocalUrl(ReturnUrl))
        //    return Redirect(ReturnUrl);

        //redirects to ReturnUrl instead of main dashboard
        if (!String.IsNullOrEmpty(ReturnUrl))
            return Redirect(ReturnUrl);

        //redirect
        return RedirectToAction("dashboard", "management");

    }

    [HttpPost]
    public async Task<IActionResult> SendEmailAsync([FromQuery] string email, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            //Check if admin does'nt exists
            if (user == null)
                return BadRequest(new { status = false, result = "Invalid Email! Email does not exist" });

            //string Email = "goharjutt1998@gmail.com";
            string Email = email;

            //Change password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //Sending email with OTP
            string messageSubject = "Reset your Password";

            var url = BaseUrl() + "/Accounts/ResetPassword?email=" + email + "&code=" + token;
            //var callbackUrl = Url.Action("changepassword", "account", new { Email = email, userId = user.Id, code = token }, protocol: BaseUrl());
            string messageBody = url;

            _emailService.SendPasswordResetEmail(email, messageBody);

            return Ok(new { status = true, result = "password reset link is sent to your email. please check email to proceed!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = false, result = ex.GetBaseException().Message });
        }
    }

    public IActionResult ResetPassword(string email, string code)
    {
        var resetPasswordVM = new ResetPasswordVM
        {
            Email = email,
            Code = code
        };

        return View(resetPasswordVM);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPasswordAsync([FromQuery] string email, string password,
        string code, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            //Check if user doesn't exist
            if (user == null)
                return StatusCode(StatusCodes.Status404NotFound,
                    new Response { Message = "Invalid Email! User does not exist" });

            //Change password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, token, password);

            //var resetPassResult = await userManager.ResetPasswordAsync(user, code, password);

            if (resetPassResult.Succeeded)
                return Ok(new { status = true, result = "" });
            else
                return BadRequest(new { status = false, result = resetPassResult.ToString() });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = false, result = ex.GetBaseException().Message });
        }
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [Authorize]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var adminVM = await _userManager.Users
            .Where(x => x.Id == _currentUser.UserId)
            .Select(u => new AdminProfileVM()
            {
                Email = u.Email,
                FullName = u.FullName,
                PictureUrl = u.ImageUrl,
            }).FirstOrDefaultAsync(cancellationToken);

        return View(adminVM);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateProfileAsync([FromQuery] string fullName, CancellationToken cancellationToken)
    {
        try
        {
            //gets user from DB
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);

            //updates user
            if (string.IsNullOrEmpty(fullName))
                return BadRequest(new ResponseModel { status = false, result = "Name can not be empty" });

            user.FullName = fullName.Trim();

            //commit changes to DB         
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(new { status = true, result = "" });
            else
                return BadRequest(new { status = false, result = result.Errors.ToString() });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = false, result = ex.GetBaseException().Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateProfilePictureAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            string url = "";
            if (file == null)
                return BadRequest(new { status = false, result = "file not found" });

            url = await UploadFile(file);

            // Get the existing student from the db
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            // Update it with the values from the view model
            if (!string.IsNullOrEmpty(url))
                user.ImageUrl = url;

            // Apply the changes if any to the db
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(new { status = true, result = "" });
            else
                return BadRequest(new { status = false, result = result.Errors.ToString() });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = false, result = ex.GetBaseException().Message });
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
