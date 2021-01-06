﻿using System;
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
using System.Net.Http;
using System.Threading.Tasks;
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

                    if(currentEvent.ESTPAYANT == 1)
                    {
                        //Création du orderid
                        string orderid = null;
                        Random random = new Random();
                        var orders = random.Next(1000, 10000);
                        var dt = DateTime.Now.ToShortTimeString().Replace("/", "");
                        var id = orders.ToString() + dt.Replace(":", "");
                        orderid = "ASC_" + id;

                        var codePlateforme = "Asc00123";
                        if(Operateur == 2)
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
    }
}