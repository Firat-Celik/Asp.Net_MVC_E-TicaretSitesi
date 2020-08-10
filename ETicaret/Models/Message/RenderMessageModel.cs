using ETicaret.Web.DB;
using System.Collections.Generic;

namespace ETicaret.Web.Models.Message
{
    public class RenderMessageModel
    {
        public List< Messages> Messages { get; set; }
        public int Count { get; set; }
    }
}