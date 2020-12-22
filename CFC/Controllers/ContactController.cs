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
       public ActionResult SendMail(FormCollection Form)
        {
            string email = Form["email"].ToString();
            string phone = Form["phone"].ToString();
            string objet = Form["objet"].ToString();
            string message = Form["message"].ToString();

            var client = new SmtpClient() {
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential("user1@test.com", "cedricanselme"),
                Host="localshot"
            };
            //client.Port = 25;
            //client.Host = "localhost";
            //client.Credentials = new NetworkCredential("user1@test.com", "cedricanselme");
            
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mailMessage = new MailMessage(email,"soumcedric16@gmail.com");
            mailMessage.Subject = objet;
            mailMessage.Body = message;
            try
            {
                client.Send(mailMessage);

            }
            catch (Exception ex)
            {

                throw;
            }














            return null;
        }
    }
}