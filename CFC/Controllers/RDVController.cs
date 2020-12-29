using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using CFC;

namespace CFC.Controllers
{
    public class RDVController : SurfaceController
    {
        [HttpPost]
       public ActionResult DemandeRdv(FormCollection Data)
        {
            DAL dal = new DAL();
            dal.OuverTureConnexion();
            return View();
        }
    }
}