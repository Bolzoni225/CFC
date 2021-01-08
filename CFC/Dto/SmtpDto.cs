using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class SmtpDto
    {
        public string NomServeur { get; set; }
        public string NomUtilisateur { get; set; }
        public string Port { get; set; }
        public string Mdp { get; set; }
        public string ReceiverMail { get; set; }
    }
}