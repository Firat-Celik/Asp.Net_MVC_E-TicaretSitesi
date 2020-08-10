using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ETicaret.Web.DB;
using ETicaret.Web.Filter;

namespace ETicaret.Controllers
{
    [MyAuthorization(_memberType: 10)]
    public class ProductController : BaseController
    {
       
        // GET: Product
        public ActionResult i()
        {
            var products =  db.Products.Where(x => x.IsDeleted == false || x.IsDeleted == null).ToList();
            return View(products.OrderByDescending(x => x.AddedDate).ToList());
        }
        
        public ActionResult Edit(int id = 0)
        {
            var product =  db.Products.FirstOrDefault(x => x.Id == id);
            var categories =  db.Categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            ViewBag.Categories = categories;
            return View(product);
        }
        [HttpPost]
        public ActionResult Edit( Products product)
        {
            var productImagePath = string.Empty;
            if (Request.Files != null && Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file.ContentLength > 0)
                {
                    var folder = Server.MapPath("~/Content/img");
                    var fileName = Guid.NewGuid() + ".jpg";
                    file.SaveAs(Path.Combine(folder, fileName));

                    var filePath = "Content/img/" + fileName;
                    productImagePath = filePath;
                }
            }
            if (product.Id > 0)
            {
                var dbProduct =  db.Products.FirstOrDefault(x => x.Id == product.Id);
                dbProduct.Category_Id = product.Category_Id;
                dbProduct.ModifiedDate = DateTime.Now;
                dbProduct.Description = product.Description;
                dbProduct.IsContinued = product.IsContinued;
                dbProduct.Name = product.Name;
                dbProduct.Price = product.Price;
                dbProduct.UnitsInStock = product.UnitsInStock;
                dbProduct.IsDeleted = false;
                if (string.IsNullOrEmpty(productImagePath) == false)
                {
                    dbProduct.ProductImageName = productImagePath;
                }
            }
            else
            {
                product.AddedDate = DateTime.Now;
                product.IsDeleted = false;
                product.ProductImageName = productImagePath;
                 db.Entry(product).State = System.Data.Entity.EntityState.Added;

            }

             db.SaveChanges();

            return RedirectToAction("i","Product");
        }
        public ActionResult Delete(int id)
        {
            var pro =  db.Products.FirstOrDefault(x => x.Id == id);
            pro.IsDeleted = true;
             db.SaveChanges();
            return RedirectToAction("i", "Product");
        }
    }
}