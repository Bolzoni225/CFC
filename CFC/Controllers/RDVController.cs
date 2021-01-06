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
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace CFC.Controllers
{
    public class RDVController : SurfaceController
    {
        private Database _db;
        protected HttpResponseMessage _response = new HttpResponseMessage();
        protected ObjetRetour Result = new ObjetRetour();

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


        public JsonResult Intermediaire()
        {
            return Json(new { data = true }, JsonRequestBehavior.AllowGet);
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

        #endregion


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

        [HttpGet]
        public JsonResult ListeSecteurActivite()
        {
            try
            {
                var liste = _db.Fetch<SecteurDto>(new Sql().Select("*").From("TB_SECTEUR"));
                return Json(new { ok = true, data = liste }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return Json(new { ok = false, data = "", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public async Task<JsonResult> AddParticipant(string jsonObject, int Operateur)
        {
            try
            {
                var ObjectParticipant = JsonConvert.DeserializeObject<ParticipantDto>(jsonObject);
                ObjectParticipant.DateCreation = DateTime.Now;

                var currentEvent = Session["CurrentEvent"] as CurrentEventModel;

                var retour = _db.Insert<ParticipantDto>("TB_PARTICPANT", "rowid", ObjectParticipant);

                //var NewParticipant = _db.Fetch<ParticipantDto>(new Sql().Select("*").From("TB_PARTICPANT").Where("Telephone = @telephone","DateCreation = @dateCreation", new { telephone = ObjectParticipant.Telephone, dateCreation = ObjectParticipant.DateCreation })).FirstOrDefault();
                //string stringSql = string.Format("SELECT * FROM TB_PARTICPANT WHERE Telephone = '{0}' AND DateCreation = '{1}'", ObjectParticipant.Telephone, Convert.ToDateTime(ObjectParticipant.DateCreation));
                var stringSql = "SELECT * FROM TB_PARTICPANT WHERE Telephone = '" + ObjectParticipant.Telephone + "' AND DateCreation >= '" + Convert.ToDateTime(ObjectParticipant.DateCreation) + "'";
                var query = new Sql(stringSql);
                var NewParticipant = _db.Fetch<ParticipantDto>(query).FirstOrDefault();

                if (NewParticipant != null)
                {
                    var NewParticiper = new ParticiperDto()
                    {
                        rowidparticipant = NewParticipant.rowid,
                        rowidevenement = currentEvent.ROWID,
                        dateparticipation = DateTime.Now
                    };

                    var result = _db.Insert<ParticiperDto>("TB_PARTICIPER", "rowid", NewParticiper);

                    if (currentEvent.ESTPAYANT == 1)
                    {
                        //Création du orderid
                        string orderid = null;
                        Random random = new Random();
                        var orders = random.Next(1000, 10000);
                        var dt = DateTime.Now.ToShortTimeString().Replace("/", "");
                        var id = orders.ToString() + dt.Replace(":", "");
                        orderid = "ASC_" + id;

                        var codePlateforme = "Asc00123";
                        if (Operateur == 2)
                        {
                            _response = await Api.Url.GetAsync("/api/Operation/InitierPaiementMoov?montant=" + currentEvent.TARIF + "&CodePlateForme=" + codePlateforme + "&orderid=" + orderid);
                        }
                        else
                        {
                            _response = await Api.Url.GetAsync("/api/Operation/InitierPaiementOrange?montant=" + currentEvent.TARIF + "&CodePlateForme=" + codePlateforme + "&orderid=" + orderid);

                        }


                        if (_response.IsSuccessStatusCode)
                        {
                            Result = JsonConvert.DeserializeObject<ObjetRetour>(_response.Content.ReadAsStringAsync().Result);
                            if (Result.Etat)
                            {
                                var DecompressResult = JsonConvert.DeserializeObject<RetourPaiement>(Result.Contenu.ToString());

                                //initiation du paiement
                                var querySql = new Sql("SELECT * FROM TB_PARTICIPER WHERE rowidevenement = '" + NewParticiper.rowidevenement + "' AND rowidparticipant = '" + NewParticiper.rowidparticipant + "'");
                                var NewQuery = _db.Fetch<ParticiperDto>(querySql).FirstOrDefault();

                                var NewPaiement = new PaiementDto()
                                {
                                    ID_PARTICIPER = NewQuery.rowid,
                                    MONTANT = currentEvent.TARIF,
                                    STATUT_PAIEMENT = 1,//initier
                                    DATE_PAIEMENT = DateTime.Now,
                                    DATE_MODIFICATION = DateTime.Now,
                                    ORDERID = DecompressResult.orderId
                                };
                                _db.Insert<PaiementDto>("TB_PAIEMENT", "ROWIDAUTO", NewPaiement);

                                return Json(new { ok = true, data = DecompressResult, aPayer = 1, url = "/Accueil", message = "Votre réservation est enregistrée, Veuillez terminer le processus SVP!" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { ok = false, message = Result.Message }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                            return Json(new { ok = false, message = string.Format("{0}: {1}", _response.StatusCode, _response.RequestMessage), JsonRequestBehavior.AllowGet });

                    }
                    else
                    {
                        return Json(new { ok = true, aPayer = 0, url = "/Accueil", message = "Votre réservation a bien été enregistrée" }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { ok = false, message = "Echec de la réservation pour cet évènement !" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {

                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpPost]//GetStatutPaiement
        public async Task<JsonResult> GetStatutPaiement(string jsonObject)
        {
            try
            {
                var ic = JObject.Parse(jsonObject);
                var operateur = (int)ic["operateur"];
                var OrderId = (string)ic["OrderId"];

                if (operateur == 2)
                {
                    _response = await Api.Url.GetAsync("/api/Operation/GetStatutPaiementMoov?orderId=" + OrderId);
                }
                else
                {
                    _response = await Api.Url.GetAsync("/api/Operation/GetStatutPaiementOrange?orderId=" + OrderId);

                }

                if (_response.IsSuccessStatusCode)
                {
                    //Update paiement
                    var querySql = new Sql("SELECT * FROM TB_PAIEMENT WHERE ORDERID = '" + OrderId + "'");
                    var UpdatePaiement = _db.Fetch<PaiementDto>(querySql).FirstOrDefault();

                    Result = JsonConvert.DeserializeObject<ObjetRetour>(_response.Content.ReadAsStringAsync().Result);
                    if (Result.Etat)
                    {

                        //UpdatePaiement.STATUT_PAIEMENT = 2;//Payer
                        if (UpdatePaiement.STATUT_PAIEMENT != 2)
                        {
                            var sql = new Sql("UPDATE TB_PAIEMENT SET STATUT_PAIEMENT = '" + 2 + "', DATE_MODIFICATION = '" + DateTime.Now + "' WHERE ROWIDAUTO = '" + UpdatePaiement.ROWIDAUTO + "'");
                            _db.Execute(sql);
                        }
                        return Json(new { ok = true, message = Result.Message }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //UpdatePaiement.STATUT_PAIEMENT = 3;//Echec

                        var sql = new Sql("UPDATE TB_PAIEMENT SET STATUT_PAIEMENT = '" + 3 + "', DATE_MODIFICATION = '" + DateTime.Now + "' WHERE ROWIDAUTO = '" + UpdatePaiement.ROWIDAUTO + "'");
                        _db.Execute(sql);

                        return Json(new { ok = false, message = Result.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json(new { ok = false, message = string.Format("{0}: {1}", _response.StatusCode, _response.RequestMessage), JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message);
                return Json(new { ok = false, message = ex.Message });
            }

        }

        [HttpPost]//NotifPaiementOrange
        public ObjetRetour NotifPaiementOrange(NotifClientModel value)
        {

            var retour = new ObjetRetour();
            try
            {
                if (value != null)
                {
                    var querySql = new Sql("SELECT * FROM TB_PAIEMENT WHERE ORDERID = '" + value.orderId + "'");
                    var UpdatePaiement = _db.Fetch<PaiementDto>(querySql).FirstOrDefault();

                    if (value.status == "SUCCESS" || value.status == "Successful")
                    {
                        //UpdatePaiement.STATUT_PAIEMENT = 2;//Payer
                        if (UpdatePaiement.STATUT_PAIEMENT != 2)
                        {
                            var sql = new Sql("UPDATE TB_PAIEMENT SET STATUT_PAIEMENT = '" + 2 + "', DATE_MODIFICATION = '" + DateTime.Now + "' WHERE ORDERID = '" + value.orderId + "'");
                            _db.Execute(sql);
                        }
                        retour.Etat = true;
                        retour.Message = $"SUCCESS: PAIEMENT EFFECTUE AVEC SUCCES";
                        //_logger.LogInfo($"SUCCESS: PAIEMENT EFFECTUE AVEC SUCCES");
                    }
                    else
                    {
                        retour.Message = $"Notification non valide! Statut : " + value.status;
                        retour.Etat = false;
                        //_logger.LogInfo($"Notification non valide! Statut : " + value.status);
                    }

                }

            }
            catch (Exception ex)
            {
                retour.Etat = false;
                retour.Message = ex.Message;
                //_logger.LogError(ex.Message);
            }
            return retour;
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
        [HttpGet]
        public JsonResult Secteur()
        {
            Sql sql = new Sql("SELECT * FROM TB_SECTEUR");
            var ListeSecteur = _db.Fetch<SecteurDto>(sql);
            return Json(new { ok = true, liste = ListeSecteur.ToList() },JsonRequestBehavior.AllowGet);
        }

        public ActionResult MethodePassage()
        {

            var url = Request.Url.GetLeftPart(UriPartial.Authority);
            //tu peux faire passer les parametres ici et les communiquer à la vue soit avec un viewbag ou un viewdata
            return Redirect(url + "/ListeParticipant");
        }
    }


     
    }
