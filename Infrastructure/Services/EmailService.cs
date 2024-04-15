using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public class EmailService: IEmailService
    {
        public bool SendEmail(string email, int otp)
        {
            try
            {
                using (MailMessage mail = new())
                {
                    mail.From = new("usman00017@devassort.com", "Verification");
                    mail.To.Add(email);
                    mail.Subject = "Verfication Process";
                    mail.Body = "<div class='card' style='align-content:center'><div class='card-body'> <h4 class='card - title'>Verification Code</h4><p>Your Verification code for verifying your email: <strong>" + otp
                    + "</strong></p><div>Your OTP get expire soon</div>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new("sg2plzcpnl486133.prod.sin2.secureserver.net", 587))
                    {
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential("usman00017@devassort.com", "++wBR?)qv.Fj");
                        try
                        {
                            smtp.Send(mail);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            throw new CustomInvalidOperationException(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomInvalidOperationException(ex.Message);
            }

        }

        public bool SendPasswordResetEmail(string toEmail, string body)
        {
            try
            {
                using (MailMessage mail = new())
                {
                    mail.From = new("usman00017@devassort.com", "Warren's Catering");
                    mail.To.Add(toEmail);
                    mail.Subject = "Reset your Password";
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new("sg2plzcpnl486133.prod.sin2.secureserver.net", 587))
                    {
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential("usman00017@devassort.com", "++wBR?)qv.Fj");
                        try
                        {
                            smtp.Send(mail);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
