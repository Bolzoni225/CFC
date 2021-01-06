using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class ObjetRetour
    {
        public ObjetRetour()
        {
            Etat = false;
            Contenu = null;
            Message = string.Empty;
        }
        public bool Etat { get; set; }
        public object Contenu { get; set; }
        public string Message { get; set; }
    }
}