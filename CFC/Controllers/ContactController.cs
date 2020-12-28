using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace CFC.Controllers
{
    public class ContactController : SurfaceController
    {
        [HttpPost]
       public ActionResult SendMail(FormCollection Form)
        {
            string email = Form["email"].ToString();
            string phone = Form["phone"].ToString();
            string objet = Form["objet"].ToString();
            string message = Form["message"].ToString();

            var client = new SmtpClient() {
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential("cfc@test.com", "123456"),
                Host = "localhost",
                Port = 25,
                EnableSsl = false
            };
            //client.Port = 25;
            //client.Host = "localhost";
            //client.Credentials = new NetworkCredential("user1@test.com", "cedricanselme");
            
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mailMessage = new MailMessage(email,"user2@test.com");
            mailMessage.Subject = objet;
            mailMessage.Body = message;
            try
            {
                client.Send(mailMessage);
                var url = Request.Url.GetLeftPart(UriPartial.Authority);
                return Redirect(url+"/success");
                

            }
            catch (Exception ex)
            {

                throw;
            }

            return null;
        }
    }
}