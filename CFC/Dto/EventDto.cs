using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class EventDto
    {
        public int ROWID { get; set; }
        public string NOM { get; set; }
        public string TITRE { get; set; }
        public string DESCRIPTION { get; set; }
        public byte ESTPAYANT { get; set; }
        public decimal TARIF { get; set; }
        public byte ESTPUBLIER { get; set; }
        public int TYPE { get; set; }
        public string LIEU { get; set; }
        public DateTime DATE_HEURE { get; set; }
        public string URLIMAGE { get; set; }
	
    }
}