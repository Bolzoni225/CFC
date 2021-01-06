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
using System.Threading.Tasks;


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
                    ObjetRDV = RDV.ObjetRDV,
                    idSecteur = Convert.ToInt32(RDV.idSecteur)

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
                    _db.Insert<MotifDto>("TB_MOTIF", "ROWIDAUTO", motif);
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
        [HttpGet]
       public JsonResult ListeRDV()
        {
            Sql sql = new Sql("SELECT NomDemandeur,PrenomsDemandeur,Fonction,Telephone,Email,NomEntreprise,AnneeConstitution,ObjetRDV,ChiffreAffaire,DescriptionMotif,DateRDV,RowidAuto FROM TB_RDV");
            var liste = _db.Fetch<RdvDto>(sql);
            return Json(new { ok = true, liste = liste.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RDVUnique(string id)
        {
            Sql sql = new Sql("SELECT NomDemandeur,PrenomsDemandeur,Fonction,Telephone,Email,NomEntreprise,AnneeConstitution,ObjetRDV,ChiffreAffaire,DescriptionMotif,DateRDV,RowidAuto,idSecteur FROM TB_RDV where rowidauto="+id);
            var liste = _db.Fetch<RdvDto>(sql);
            Sql sqlSecteur = new Sql("SELECT * FROM TB_SECTEUR WHERE ID=" + liste.FirstOrDefault().idSecteur);
            var secteur = _db.Fetch<SecteurDto>(sqlSecteur).FirstOrDefault().LibelleSecteur.ToString();
            Sql sqlMotif = new Sql("SELECT  LibelleMotif from TB_MOTIF WHERE IdRDV="+id);
            var ListeMotif = _db.Fetch<MotifDto>(sqlMotif);
            return Json(new { ok = true, liste = liste.ToList(), ListMotif = ListeMotif, secteur = secteur }, JsonRequestBehavior.AllowGet);
        }

        #region Events / Coaching 
        public async Task<JsonResult> ListeEventsCoaching()
        {
            var liste = await _db.FetchAsync<EventDto>(new Sql().Select("*").From("TB_EVENT"));
            return Json(new { data = liste }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> AjouterEventCoaching(string value)
        {
            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var eventCoaching = JsonConvert.DeserializeObject<EventDto>(value);
                    if (eventCoaching.ROWID == 0)
                    {
                        await _db.InsertAsync<EventDto>("TB_EVENT", "ROWID", true, eventCoaching);
                        return Json(new { data = true }, JsonRequestBehavior.AllowGet);
                    }
                    var ancien = await _db.SingleAsync<EventDto>(new Sql().Select("*").From("TB_EVENT").Where("ROWID = @0", eventCoaching.ROWID));
                    ancien.LIEU = eventCoaching.LIEU;
                    ancien.NOM = eventCoaching.NOM;
                    ancien.TARIF = eventCoaching.TARIF;
                    ancien.TITRE = eventCoaching.TITRE;
                    ancien.TYPE = eventCoaching.TYPE;
                    ancien.URLIMAGE = eventCoaching.URLIMAGE;
                    ancien.DESCRIPTION = eventCoaching.DESCRIPTION;
                    ancien.ESTPAYANT = eventCoaching.ESTPAYANT;
                    ancien.ESTPUBLIER = eventCoaching.ESTPUBLIER;
                    _db.Update("TB_EVENT", "ROWID", ancien);
                    return Json(new { data = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { data = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { data = false }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<JsonResult> SupprimerEventCoaching(int ID)
        {
            try
            {
                if (ID > 0)
                {
                    var eventCoaching = await _db.SingleAsync<EventDto>(new Sql().Select("*").From("TB_EVENT").Where("ROWID = @0", ID));
                    _db.Delete("TB_EVENT", "ROWID", eventCoaching);
                    return Json(new { data = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { data = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { data = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> EventCoaching(int ID)
        {
            try
            {
                if (ID > 0)
                {
                    var eventCoaching = await _db.SingleAsync<EventDto>(new Sql().Select("*").From("TB_EVENT").Where("ROWID = @0", ID));
                    return Json(new { data = eventCoaching }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { data = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = false }, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult Intermediaire ()
        {
             return Json(new { data = true }, JsonRequestBehavior.AllowGet);
        }


        #endregion


        //#region Liste Participant ***by brice***
        //[HttpGet]
        //public JsonResult ListeParticipants(string id)
        //{
        //    // _db.Fetch<EventDto>(new Sql().Select("*").From("TB_EVENT"));
        //    //var liste = await _db.FetchAsync<EventDto>(new Sql().Select("*").From("TB_EVENT"));
        //    try
        //    {
        //        Sql sql = new Sql("SELECT * FROM TB_PARTICPANT TBP, TB_PARTICIPER TBPAR WHERE TBP.ROWID = TBPAR.rowidparticipant AND TBPAR.rowidevenement =" + id);
        //        var liste = _db.Fetch<ParticipantDto>(sql);
        //        return Json(new { ok = true, data = liste }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        var message = ex.Message;
        //        return Json(new { ok = false, data = "", message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }

        //}
        //#endregion

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

        [HttpGet]
        public JsonResult Secteur()
        {
            Sql sql = new Sql("SELECT * FROM TB_SECTEUR");
            var ListeSecteur = _db.Fetch<SecteurDto>(sql);
            return Json(new { ok = true, liste = ListeSecteur.ToList() },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListeParticipants(int jsonObject)  
        {
            string message = string.Empty;
            try
            {
                //int id = Convert.ToInt32(jsonObject);
                Sql sql = new Sql("SELECT * FROM TB_PARTICPANT TBP, TB_PARTICIPER TBPAR WHERE TBP.ROWID = TBPAR.rowidparticipant AND TBPAR.rowidevenement =" + jsonObject);
                var liste = _db.Fetch<ParticipantDto>(sql);
                return Json(new { ok = true, data = liste }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return Json(new { ok = false, data = "", message = "Succès" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VueParticipants(int id)   
        {
            TempData["ROWID"] = id;
            var url = Request.Url.GetLeftPart(UriPartial.Authority);
            //tu peux faire passer les parametres ici et les communiquer à la vue soit avec un viewbag ou un viewdata
            return Redirect(url + "/ListeParticipant");
        }
    }
}
