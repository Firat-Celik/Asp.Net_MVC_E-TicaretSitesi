using ETicaret.Web.DB;

namespace ETicaret.Web.Models.i
{
    public class BuyModels
    {
        public string OrderId { get; set; }
        public string OrderName { get; set; }
        public decimal TotelPrice { get; set; }
        public string OrderStatus { get; set; }
        public  Members Member { get; set; }
    }
}