using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OTPhistory
    {
        public OTPhistory(string token, DateTime expireDateTime, OTPmediaTypeEnum otpMediaType, string replacementValue, string? originalValue)
        {
            Token = token;
            ExpireDateTime = expireDateTime;
            OtpMediaType = otpMediaType;
            ReplacementValue = replacementValue;
            OriginalValue = originalValue;

        }
        public OTPhistory(string token, DateTime expireDateTime, OTPmediaTypeEnum otpMediaType, string replacementValue,string? originalValue, string? createdBy)
        {
            Token = token;
            ExpireDateTime = expireDateTime;
            OtpMediaType = otpMediaType;
            ReplacementValue = replacementValue;
            OriginalValue = originalValue;

            CreatedBy = createdBy;
        }
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public OTPmediaTypeEnum OtpMediaType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpireDateTime { get; set; }
        public string CreatedBy { get; set; }//FK UserId
        public string? ReplacementValue { get; set; }
        public string? OriginalValue { get; private set; }


        public static OTPhistory Create(string code, DateTime expiryDateTime, OTPmediaTypeEnum otpMediaType, string? replacementValue, string? originalValue, string? createdBy = null)
        {
            if (string.IsNullOrEmpty(createdBy))
                return new OTPhistory(code, expiryDateTime, otpMediaType, replacementValue, originalValue);

            return new OTPhistory(code, expiryDateTime, otpMediaType, replacementValue, originalValue, createdBy);

        }
    }
    
}
