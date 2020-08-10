using ETicaret.Web.DB;
using System.Collections.Generic;

namespace ETicaret.Web.Models.i
{
    public class CommentModels
    {
        public List< Comments> Comments { get; set; }
        public  Products Product { get; set; }
    }
}