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
using Newtonsoft.Json;

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
       public JsonResult DemandeRdv(string value)
        {

            var RDV = JsonConvert.DeserializeObject<RdvModel>(value);
            string url = string.Empty;

            try
            {


                var NewEvent = new RdvDto()
                {
                    AnneeConstitution = RDV.AnneeConstitution,
                    ChiffreAffaire = RDV.ChiffreAffaire,
                    DescriptionMotif = RDV.DescriptionMotif,
                    Email = RDV.Email,
                    NomDemandeur = RDV.NomDemandeur,
                    PrenomsDemandeur = RDV.PrenomsDemandeur,
                    Telephone = RDV.Telephone,
                    NomEntreprise = RDV.NomEntreprise,
                    DateRDV = Convert.ToDateTime(RDV.DateRDV),
                    Fonction = RDV.Fonction,
                    ObjetRDV = RDV.ObjetRDV

                };
                Sql sql = new Sql("SELECT TOP   1 * FROM TB_RDV ORDER BY ROWIDAUTO DESC");
                _db.Insert<RdvDto>("TB_RDV", "RowidAuto", NewEvent);
                var lastRDV = _db.Fetch<RdvDto>(sql).First().RowidAuto;

                foreach (var item in RDV.ListeMotif)
                {
                    var motif = new MotifDto()
                    {
                        idRDV = lastRDV,
                        LibelleMotif = item
                    };
                    _db.Insert<MotifDto>("TB_MOTIF","ROWIDAUTO",motif);
                }
                                                          
                url = Request.Url.GetLeftPart(UriPartial.Authority);
                //return Redirect(url + "/success");
                url = url + "/success";
                return Json(new { ok = true, chemin = url }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                url = url + "/Echec";
                return Json(new { ok = true, chemin = url }, JsonRequestBehavior.AllowGet);
            }
           
        }
    }
}