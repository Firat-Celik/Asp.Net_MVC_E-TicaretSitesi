using ETicaret.Web.DB;
using System.Collections.Generic;

namespace ETicaret.Web.ViewModel.Home
{
    public class IndexModel
    {
        public List<Products> Products { get; set; }
        public Categories Category { get; set; }
    }
}