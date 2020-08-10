using ETicaret.Web.DB;
using System.Collections.Generic;

namespace ETicaret.Web.ViewModel.Home
{
    public class ProductModels
    {
        public Products Product { get; set; }
        public List<Comments> Comments { get; set; }
    }
}