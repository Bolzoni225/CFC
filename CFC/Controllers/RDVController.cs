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

                throw;
            }
            return View();
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
            Sql sql = new Sql("SELECT NomDemandeur,PrenomsDemandeur,Fonction,Telephone,Email,NomEntreprise,AnneeConstitution,ObjetRDV,ChiffreAffaire,DescriptionMotif,DateRDV,RowidAuto FROM TB_RDV where rowidauto="+id);
            var liste = _db.Fetch<RdvDto>(sql);
            Sql sqlMotif = new Sql("SELECT  LibelleMotif from TB_MOTIF WHERE IdRDV="+id);
            var ListeMotif = _db.Fetch<MotifDto>(sqlMotif);
            return Json(new { ok = true, liste = liste.ToList(), ListMotif = ListeMotif }, JsonRequestBehavior.AllowGet);
        }
    }
}