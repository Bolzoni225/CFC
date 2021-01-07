using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
	public class RdvModel
	{
        public string NomDemandeur { get; set; }
        public string PrenomsDemandeur { get; set; }
        public string Fonction { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string NomEntreprise { get; set; }
        public string AnneeConstitution { get; set; }
        public string ObjetRDV { get; set; }
        public string ChiffreAffaire { get; set; }
        public string MotifDemande { get; set; }
        public string DescriptionMotif { get; set; }
        public string DateRDV { get; set; }
        public string timeRDV { get; set; }
        public List<string> ListeMotif { get; set; }
        public string idSecteur { get; set; }
    }
}