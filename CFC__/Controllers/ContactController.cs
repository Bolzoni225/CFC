using CFC.Dto;
using NPoco;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private Database _db;
        public ContactController()
        {
            _db = new Database(ConfigurationManager.ConnectionStrings["cfcFinances"].ConnectionString, ConfigurationManager.ConnectionStrings["cfcFinances"].ProviderName);
        }


        [HttpPost]
       public ActionResult SendMail(FormCollection Form)
        {
            var url = Request.Url.GetLeftPart(UriPartial.Authority);
            try
            {
                Sql sql = new Sql("SELECT * FROM TB_SMTP");
            var ParametreSmtp = _db.Fetch<SmtpDto>(sql).FirstOrDefault();




            string email = Form["email"].ToString();
            string phone = Form["phone"].ToString();
            string objet = Form["objet"].ToString();
            string message = Form["message"].ToString();
            message = message + "\n\n\n" + "Numéro de Téléphone " + phone;

            var client = new SmtpClient() {
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(ParametreSmtp.NomUtilisateur, ParametreSmtp.Mdp),
                Host =ParametreSmtp.NomServeur,
                Port = Convert.ToInt32(ParametreSmtp.Port),
                EnableSsl = true
            };
            //client.Port = 25;
            //client.Host = "localhost";
            //client.Credentials = new NetworkCredential("user1@test.com", "cedricanselme");
            
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mailMessage = new MailMessage(email,ParametreSmtp.NomUtilisateur);
            mailMessage.Subject = objet;
            mailMessage.Body = message;
            
          
                client.Send(mailMessage);
                
                return Redirect(url+"/success");
                

            }
            catch (Exception ex)
            {

                return Redirect(url + "/Echec");
            }

            return null;
        }
    }
}