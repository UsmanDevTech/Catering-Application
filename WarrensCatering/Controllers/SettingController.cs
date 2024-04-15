using Application.Accounts.Commands;
using Application.Accounts.Queries;
using Application.Common.Exceptions;
using Application.Common;
using Application.Common.Models;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqftAPI.Controllers;
using Domain.Helpers;
using WarrensCatering.Filters;

namespace WarrensCatering.Controllers
{
     
    public class SettingController : BaseApiController
    {
        [ServiceFilter(typeof(ActionFilter))]
        [HttpGet(Routes.Account.get_profile)]
        public async Task<UserProfileContract> GetProfileAsync([FromQuery]string? userId, CancellationToken token)
        {
            return await Mediator.Send(new GetAccountProfileQuery( userId), token);
        }

        [ServiceFilter(typeof(ActionFilter))]
        [HttpPut(Routes.Account.update_profile)]
        public async Task<Result>UpdateUserProfile(UpdateProfileCommand request, CancellationToken token)
        {
            return await Mediator.Send(request, token);
        }



        [AllowAnonymous]
        [HttpPost(Routes.Account.upload_base64_file), DisableRequestSizeLimit]
        public ResponseKey UploadFile([FromBody] ResponseKey source)
        {
            try
            {
                if (string.IsNullOrEmpty(source.key))
                    throw new NotFoundException("No file attached.");

                var folderName = Path.Combine(@"wwwroot", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                //Check if directory exist
                if (!Directory.Exists(pathToSave))
                    Directory.CreateDirectory(pathToSave); //Create directory if it doesn't exist

                string imageName = "Base" + DateTime.Now.Ticks.ToString() + source.key.GetBase64FileType();

                //set the image path
                string imgPath = Path.Combine(pathToSave, imageName);

                byte[] imageBytes = Convert.FromBase64String(source.key);

                System.IO.File.WriteAllBytes(imgPath, imageBytes);
                //Get Base Url From Request
                var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                //Final Return Url
                var dbPath = $"{baseUrl}/Images/{imageName}";

                return new ResponseKey { key = dbPath };
            }
            catch (Exception ex)
            {
                throw new CustomInvalidOperationException(ex.GetBaseException().Message);
            }

        }



        [AllowAnonymous]
        [HttpGet(Routes.Account.get_terms)]
        public async Task<GenericAppDocumentContract> GetTermsAsync(CancellationToken token)
        {
            return await Mediator.Send(new GetTermsQuery(Domain.Enums.ContentType.Terms), token);
        }

        [AllowAnonymous]
        [HttpGet(Routes.Account.get_policy)]
        public async Task<GenericAppDocumentContract> GetPolicyAsync(CancellationToken token)
        {
            return await Mediator.Send(new GetTermsQuery(Domain.Enums.ContentType.PrivacyPolicy), token);
        }



    }
}
