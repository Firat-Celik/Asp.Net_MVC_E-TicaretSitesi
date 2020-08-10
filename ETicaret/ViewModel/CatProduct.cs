using ETicaret.Web.DB;
using System.Collections.Generic;

namespace ETicaret.Web.ViewModel
{
    public class CatProduct
    {
        public List<Products> ProductList { get; set; }
        public List<Categories> CatList { get; set; }       
        public Categories Category { get; set; }
        public Products Product { get; set; }
    }
}