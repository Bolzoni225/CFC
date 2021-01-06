using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class RdvDto
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
        public DateTime DateRDV {get;set;}
       public int RowidAuto { get; set; }
        public int idSecteur { get; set; }
        
	
    }
}