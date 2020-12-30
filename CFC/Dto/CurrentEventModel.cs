using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class CurrentEventModel
    {
        public int ROWID { get; set; }
        public string NOM { get; set; }
        public string TITRE { get; set; }
        public byte ESTPAYANT { get; set; }
        public decimal TARIF { get; set; }
	
    }
}