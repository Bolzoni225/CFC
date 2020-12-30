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

        #region Liste Participant ***by brice***
        [HttpGet]
        public JsonResult ListeParticipants()
        {
            // _db.Fetch<EventDto>(new Sql().Select("*").From("TB_EVENT"));
            //var liste = await _db.FetchAsync<EventDto>(new Sql().Select("*").From("TB_EVENT"));
            try
            {
                var liste = _db.Fetch<ParticipantDto>(new Sql().Select("*").From("TB_PARTICPANT"));
                return Json(new { ok = true, data = liste }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return Json(new { ok = false, data = "", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region News Events ***by brice***
        [HttpGet]
        public JsonResult NewsEvents()
        {
            //var liste = _db.Fetch<EventDto>(new Sql().Select("*").From("TB_EVENT").Where("ESTPUBLIER = @estPublier", "ESTPAYANT = @estPayant", new { estPublier = false, estPayant = true }));

            try
            {
                var liste = _db.Fetch<EventDto>(new Sql().Select("*").From("TB_EVENT").Where("ESTPUBLIER = @estPublier", new { estPublier = true }));
                return Json(new { ok = true, data = liste }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return Json(new { ok = false, data = "", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]//pas soumit au service
        public JsonResult CreateCurrentEvent(string jsonObject)
        {
            try
            {
                var currentEvent = JsonConvert.DeserializeObject<CurrentEventModel>(jsonObject);
                if (currentEvent != null)
                {
                    Session["CurrentEvent"] = currentEvent;

                    return Json(new { ok = true, message = "", url = "/FormParticipation" }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { ok = false, message = "Recherche Impossible" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message);
                return Json(new { ok = false, message = ex.Message });
            }

        }
        #endregion
    }
}