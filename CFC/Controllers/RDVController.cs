using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using CFC;
using NPoco;
using System.Configuration;
using CFC.Dto;

namespace CFC.Controllers
{
    public class RDVController : SurfaceController
    {
        private Database _db;

        public RDVController()
        {
            _db = new Database(ConfigurationManager.ConnectionStrings["cfcFinances"].ConnectionString, ConfigurationManager.ConnectionStrings["cfcFinances"].ProviderName);
        }


        [HttpPost]
       public ActionResult DemandeRdv(FormCollection Data)
        {
            try
            {
                var nom = Data["Nom"];
                var prenoms = Data["Prenoms"];
                var phone = Data["phone"];
                var mail = Data["email"];
                var nomets = Data["nomets"];
                var anneeconstitution = Data["anneeconstitution"];
                var chiffreAffaire = Data["chiffreAffaire"];
                var effectif = Data["effectif"];
                var motif = Data["motif"];
                var description = Data["description"];

                var NewEvent = new RdvDto()
                {
                    AnneeConstitution = anneeconstitution,
                    ChiffreAffaire = chiffreAffaire,
                    DescriptionMotif = description,
                    Email = mail,
                    NomDemandeur = nom,
                    PrenomsDemandeur = prenoms,
                    Telephone = phone,
                    NomEntreprise = nomets,
                    DateRDV = DateTime.Now

                };

                _db.Insert<RdvDto>("TB_RDV", "ROWID", NewEvent);
                var url = Request.Url.GetLeftPart(UriPartial.Authority);
                return Redirect(url + "/success");
            }
            catch (Exception ex)
            {

                throw;
            }
            return View();
        }
    }
}