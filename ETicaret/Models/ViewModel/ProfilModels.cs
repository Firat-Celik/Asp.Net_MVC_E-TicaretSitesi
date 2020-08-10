using ETicaret.Web.DB;
using System.Collections.Generic;

namespace ETicaret.Web.Models.ViewModel
{
    public class ProfilModels
    {
        public Members Members { get; set; }
        public List<Addresses> Addresses { get; set; }
        public Addresses CurrentAddress { get; set; }
    }
}