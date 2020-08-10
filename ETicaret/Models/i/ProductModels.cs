using ETicaret.Web.DB;
using System.Collections.Generic;

namespace ETicaret.Web.Models.i
{
    public class ProductModels
    {
        public  Products Product { get; set; }
        public List< Comments> Comments { get; set; }
    }
}