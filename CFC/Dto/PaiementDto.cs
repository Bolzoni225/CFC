using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class PaiementDto
    {
        public int ROWIDAUTO { get; set; }
        public decimal MONTANT { get; set; }
        public DateTime DATE_PAIEMENT { get; set; }
        public int STATUT_PAIEMENT { get; set; }
        public DateTime DATE_MODIFICATION { get; set; }
        public int ID_PARTICIPER { get; set; }
        public string ORDERID { get; set; }
	
    }
}