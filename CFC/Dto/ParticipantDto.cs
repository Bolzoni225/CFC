using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class ParticipantDto
    {
        public int rowid { get; set; }
        public string NomPrenoms { get; set; }
        public string Titre { get; set; }
        public string Telephone { get; set; }
        public string email { get; set; }
        public string NomEts { get; set; }
        public string AnneeConstitution { get; set; }
        public int Id_Secteur { get; set; }
        public string Adressegeographique { get; set; }
        public DateTime DateCreation { get; set; }

    }
}