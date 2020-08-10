using ETicaret.Controllers;
using ETicaret.Web.DB;
using ETicaret.Web.Filter;
using ETicaret.Web.Models.i;
using ETicaret.Web.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ETicaret.Web.Controllers
{
    public class HomeController : BaseController
    {
       
        public HomeController()
        {

            ViewBag.MenuCategories = db.Categories.Where(x => x.Parent_Id == null).ToList();
        }


        // GET: Home
        public ActionResult Index(int id = 0)
        {
            IQueryable<Products> products = db.Products.OrderByDescending(x => x.AddedDate).Where(x => x.IsDeleted == false || x.IsDeleted == null);
            Categories category = null;
            if (id > 0)
            {
                category = db.Categories.FirstOrDefault(x => x.Id == id);
                var allCategories = GetChildCategories(category);
                allCategories.Add(category);

                var catIntList = allCategories.Select(x => x.Id).ToList();
                //select * from Product where Category_Id in (1,2,3,4)
                products = products.Where(x => catIntList.Contains(x.Category_Id));
            }
            var viewModel = new ETicaret.Web.Models.i.IndexModel()
            {
                Products = products.ToList(),
                Category = category
            };
            return View(viewModel);
        }
        public ActionResult CatProduct(int? id)
        {
            CatProduct model = new CatProduct();

            var modeltek = db.Categories.FirstOrDefault(x => x.Id == id);
            var Product = db.Products.FirstOrDefault(x => x.Id == id);
            var catmodel = db.Categories.ToList();
            
            var MenuCategories = db.Categories.Where(x => x.Parent_Id == null).ToList();
            var list = db.Products.Where(
                          x => x.Category_Id == id).OrderByDescending(
                          x => x.AddedDate).ToList();
            model.CatList = catmodel;
            model.ProductList = list;
            model.Category = modeltek;
            model.Product = Product;
            ViewBag.ModelCat = modeltek;
            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Shop()
        {
            return View();
        }
        public ActionResult ProductDetail(int? id)
        {

            CatProduct model = new CatProduct();


            var Product = db.Products.FirstOrDefault(x => x.Id == id);
            var catmodel = db.Categories.ToList();
            var MenuCategories = db.Categories.Where(x => x.Parent_Id == null).ToList();
            var list = db.Products.Where(
                          x => x.Category_Id == id).OrderByDescending(
                          x => x.AddedDate).ToList();
            model.CatList = catmodel;
            model.ProductList = list;

            model.Product = Product;

            return View(model);
        }

        #region Basket

        [HttpGet]
        public ActionResult AddBasket(int id, bool remove = false)
        {
            List<ETicaret.Web.ViewModel.Home.BasketModels> basket = null;
            if (Session["Basket"] == null)
            {
                basket = new List<ETicaret.Web.ViewModel.Home.BasketModels>();
            }
            else
            {
                basket = (List<ETicaret.Web.ViewModel.Home.BasketModels>)Session["Basket"];
            }

            if (basket.Any(x => x.Product.Id == id))
            {
                var pro = basket.FirstOrDefault(x => x.Product.Id == id);
                if (remove && pro.Count > 0)
                {
                    pro.Count -= 1;
                }
                else
                {
                    if (pro.Product.UnitsInStock > pro.Count)
                    {
                        pro.Count += 1;
                    }
                    else
                    {
                        TempData["MyError"] = "Yeterli Stok yok";
                    }
                }

            }
            else
            {
                var pro = db.Products.FirstOrDefault(x => x.Id == id);
                if (pro != null && pro.IsContinued && pro.UnitsInStock > 0)
                {
                    basket.Add(new ETicaret.Web.ViewModel.Home.BasketModels()
                    {
                        Count = 1,
                        Product = pro
                    });
                }
                else if (pro != null && pro.IsContinued == false)
                {
                    TempData["MyError"] = "Bu ürünün satışı durduruldu.";
                }
            }
            basket.RemoveAll(x => x.Count < 1);
            Session["Basket"] = basket;

            return RedirectToAction("Basket", "Home");
        }
        [HttpGet]
        public ActionResult Basket()
        {

            List<ETicaret.Web.ViewModel.Home.BasketModels> model = (List<ETicaret.Web.ViewModel.Home.BasketModels>)Session["Basket"];
            if (model == null)
            {
                model = new List<ETicaret.Web.ViewModel.Home.BasketModels>();
            }
            if (base.IsLogon())
            {
                int currentId = CurrentUserId();
                ViewBag.CurrentAddresses = db.Addresses
                                            .Where(x => x.Member_Id == currentId)
                                            .Select(x => new SelectListItem()
                                            {
                                                Text = x.Name,
                                                Value = x.Id.ToString()
                                            }).ToList();
            }
            ViewBag.TotalPrice = model.Select(x => x.Product.Price * x.Count).Sum();

            return View(model);
        }
        [HttpGet]
        public ActionResult RemoveBasket(int id)
        {
            List<ETicaret.Web.ViewModel.Home.BasketModels> basket = (List<ETicaret.Web.ViewModel.Home.BasketModels>)Session["Basket"];
            if (basket != null)
            {
                if (id > 0)
                {
                    basket.RemoveAll(x => x.Product.Id == id);
                }
                else if (id == 0)
                {
                    basket.Clear();
                }
                Session["Basket"] = basket;
            }
            return RedirectToAction("Basket", "Home");
        }
        [HttpPost]
        [MyAuthorization]
        public ActionResult Buy(string Address)
        {
            if (IsLogon())
            {
                try
                {
                    var basket = (List<ETicaret.Web.ViewModel.Home.BasketModels>)Session["Basket"];
                    var guid = new Guid(Address);
                    var _address = db.Addresses.FirstOrDefault(x => x.Id == guid);
                    //Sipariş Verildi = SV
                    //Ödeme Bildirimi = OB
                    //Ödeme Onaylandı = OO

                    var order = new Orders()
                    {
                        AddedDate = DateTime.Now,
                        Address = _address.AdresDescription,
                        Member_Id = CurrentUserId(),
                        Status = "SV",
                        Id = Guid.NewGuid()
                    };
                    //5
                    //ahmet 5
                    //mehmet 5
                    foreach (ETicaret.Web.ViewModel.Home.BasketModels item in basket)
                    {
                        var oDetail = new OrderDetails();
                        oDetail.AddedDate = DateTime.Now;
                        oDetail.Price = item.Product.Price * item.Count;
                        oDetail.Product_Id = item.Product.Id;
                        oDetail.Quantity = item.Count;
                        oDetail.Id = Guid.NewGuid();

                        order.OrderDetails.Add(oDetail);

                        var _product = db.Products.FirstOrDefault(x => x.Id == item.Product.Id);
                        if (_product != null && _product.UnitsInStock >= item.Count)
                        {
                            _product.UnitsInStock = _product.UnitsInStock - item.Count;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0} ürünü için yeterli stok yoktur veya silinmiş bir ürünü almaya çalışıyorsunuz.", item.Product.Name));
                        }
                    }
                    db.Orders.Add(order);
                    db.SaveChanges();
                    Session["Basket"] = null;
                }
                catch (Exception)
                {
                   
                }
                return RedirectToAction("Buy", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }
        [HttpGet]
        [MyAuthorization]
        public ActionResult Buy()
        {
            if (IsLogon())
            {
                var currentId = CurrentUserId();
                IQueryable<Orders> orders;
                if (((int)CurentUser().MemberType) > 8)
                {
                    orders = db.Orders.Where(x => x.Status == "OB");
                }
                else
                {
                    orders = db.Orders.Where(x => x.Member_Id == currentId);
                }

                List<BuyModels> model = new List<BuyModels>();
                foreach (var item in orders)
                {
                    var byModel = new BuyModels();
                    byModel.TotelPrice = item.OrderDetails.Sum(y => y.Price);
                    byModel.OrderName = string.Join(", ", item.OrderDetails.Select(y => y.Products.Name + "(" + y.Quantity + ")"));
                    byModel.OrderStatus = item.Status;
                    byModel.OrderId = item.Id.ToString();
                    byModel.Member = item.Members;
                    model.Add(byModel);
                }

                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }


        [HttpPost]
        [MyAuthorization]
        public JsonResult OrderNotification(OrderNotificationModel model)
        {
            if (string.IsNullOrEmpty(model.OrderId) == false)
            {
                var guid = new Guid(model.OrderId);
                var order = db.Orders.FirstOrDefault(x => x.Id == guid);
                if (order != null)
                {
                    order.Description = model.OrderDescription;
                    order.Status = "OB";
                    db.SaveChanges();
                }
            }
            return Json("");
        }

        [HttpGet]
        //[HttpPost]
        public JsonResult GetProductDes(int id)
        {
            var pro = db.Products.FirstOrDefault(x => x.Id == id);
            return Json(pro.ShortText, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetOrder(string id)
        {
            var guid = new Guid(id);
            var order = db.Orders.FirstOrDefault(x => x.Id == guid);
            return Json(new
            {
                Description = order.Description,
                Address = order.Address
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [MyAuthorization]
        public JsonResult OrderCompilete(string id, string text)
        {
            var guid = new Guid(id);
            var order = db.Orders.FirstOrDefault(x => x.Id == guid);
            order.Description = text;
            order.Status = "OO";
            db.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        #endregion




        #region Partial_Views


        public PartialViewResult _Navbar()
        {

            return PartialView();
        }
        public PartialViewResult _Foot()
        {

            return PartialView();
        }
        public PartialViewResult _NavMobile()
        {

            return PartialView();
        }
        public PartialViewResult _Header()
        {
            List<ViewModel.Home.BasketModels> model = (List<ViewModel.Home.BasketModels>)Session["Basket"];
            if (model == null)
            {
                model = new List<ViewModel.Home.BasketModels>();
            }

            ViewBag.TotalPrice = model.Select(x => x.Product.Price * x.Count).Sum();

            return PartialView(model);
        }

        public PartialViewResult _Search()
        {

            return PartialView();
        }
        public PartialViewResult _Footer()
        {

            return PartialView();
        }
        public PartialViewResult _Slider()
        {

            return PartialView();
        }
        public PartialViewResult _Categories()
        {

            return PartialView();
        }
        public PartialViewResult _LeftSlide()
        {

            return PartialView();
        }
        public PartialViewResult _SpecialProduct()
        {
            var models = db.Products.OrderByDescending(i => i.Price).Take(3).ToList();


            return PartialView(models);
        }
        public PartialViewResult _MostPopular()
        {

            var model = db.Products.Take(2).ToList();


            return PartialView(model);
        }
        public PartialViewResult _Bestseller()
        {

            var model = db.Products.OrderByDescending(i => i.AddedDate).Take(5).ToList();

            return PartialView(model);
        }
        public PartialViewResult _Banner()
        {

            return PartialView();
        }
        public PartialViewResult _NewProduct()
        {
            var model = db.Products.OrderByDescending(i => i.Category_Id).Take(8).ToList();
            return PartialView(model);
        }
        public PartialViewResult _Electronic()
        {
            var model = db.Products.Where(i => i.Categories.Name == "Elektronik").Take(5).ToList();
            return PartialView(model);
        }
        public PartialViewResult _Toys()
        {
            var modelss = db.Products.Where(i => i.Categories.Name == "Spor").Take(5).ToList();
            return PartialView(modelss);
        }
        public PartialViewResult _Cosmetic()
        {
            var modelc = db.Products.Where(i => i.Categories.Name == "Aksesuar").Take(5).ToList();
            return PartialView(modelc);
        }
        public PartialViewResult _Fashion()
        {
            var modelm = db.Products.Where(i => i.Categories.Name == "Moda").Take(5).ToList();
            return PartialView(modelm);
        }
        public PartialViewResult _SearchDetail()
        {
            var model = db.Categories.ToList();
            return PartialView(model);
        }
        public PartialViewResult _CategoriesDetail()
        {

            return PartialView();
        }
        public PartialViewResult _Brend()
        {

            return PartialView();
        }
        public PartialViewResult _ShopFilter()
        {

            return PartialView();
        }
        public PartialViewResult _ShopProduct()
        {

            return PartialView();
        }
        public PartialViewResult _ProductDetail()
        {



            return PartialView();
        }
        #endregion
    }
}
