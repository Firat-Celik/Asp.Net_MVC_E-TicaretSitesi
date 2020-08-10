using ETicaret.Controllers;
using ETicaret.Web.Filter;
using ETicaret.Web.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ETicaret.Web.DB;

namespace ETicaret.Web.Controllers
{
    public class AccountController : BaseController
    {
        
        // GET: Account
        public ActionResult Register()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Register(ETicaret.Web.Models.ViewModel.RegisterModels user)
        {
            try
            {
                if (user.rePassword != user.Member.Password)
                {
                    throw new Exception("Şifreler aynı değildir");
                }
                if ( db.Members.Any(x => x.Email == user.Member.Email))
                {
                    throw new Exception("Zaten bu e-posta adresi kayıtlıdır.");
                }
                user.Member.MemberType = MemberTypes.Customer;
                user.Member.AddedDate = DateTime.Now;
                 db.Members.Add(user.Member);
                 db.SaveChanges();
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.ReError = ex.Message;
                return View();
            }

        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(ETicaret.Web.Models.ViewModel.LoginModels model)
        {
            try
            {
                var user =  db.Members.FirstOrDefault(x => x.Password == model.Member.Password && x.Email == model.Member.Email);
                if (user != null)
                {
                    Session["LogonUser"] = user;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ReError = "Kullanici Bilgileriniz yanlış";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ReError = ex.Message;
                return View();
            }
        }
        public ActionResult Logout()
        {
            Session["LogonUser"] = null;
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public ActionResult Profil(int id = 0, string ad = "")
        {
            List<Addresses> addresses = null;
            Addresses currentAddress = new Addresses();
            if (id == 0)
            {
                id = base.CurrentUserId();
                addresses =  db.Addresses.Where(x => x.Member_Id == id).ToList();
                if (!string.IsNullOrEmpty(ad))
                {
                    var guild = new Guid(ad);
                    currentAddress =  db.Addresses.FirstOrDefault(x => x.Id == guild);
                }
            }
            var user =  db.Members.FirstOrDefault(x => x.Id == id);
            if (user == null) return RedirectToAction("Index", "Home");
            ProfilModels model = new ProfilModels()
            {
                Members = user,
                Addresses = addresses,
                CurrentAddress = currentAddress
            };
            return View(model);
        }
        [HttpGet]
        [MyAuthorization]
        public ActionResult ProfilEdit()
        {
            int id = base.CurrentUserId();
            var user =  db.Members.FirstOrDefault(x => x.Id == id);
            if (user == null) return RedirectToAction("Index", "Home");
            ProfilModels model = new ProfilModels()
            {
                Members = user
            };
            return View(model);
        }
        [HttpPost]
        [MyAuthorization]
        public ActionResult ProfilEdit(ProfilModels model)
        {

            try
            {
                int id = CurrentUserId();
                var updateMember =  db.Members.FirstOrDefault(x => x.Id == id);
                updateMember.ModifiedDate = DateTime.Now;
                updateMember.Bio = model.Members.Bio;
                updateMember.Name = model.Members.Name;
                updateMember.Surname = model.Members.Surname;

                if (string.IsNullOrEmpty(model.Members.Password) == false)
                {
                    updateMember.Password = model.Members.Password;
                }
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file.ContentLength > 0)
                    {
                        var folder = Server.MapPath("~/Content/img");
                        var fileName = Guid.NewGuid() + ".jpg";
                        file.SaveAs(Path.Combine(folder, fileName));

                        var filePath = "Content/img/" + fileName;
                        updateMember.ProfileImageName = filePath;
                    }
                }
                 db.SaveChanges();

                return RedirectToAction("Profil", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.MyError = ex.Message;
                int id = CurrentUserId();
                var viewModel = new ETicaret.Web.Models.ViewModel.ProfilModels()
                {
                    Members =  db.Members.FirstOrDefault(x => x.Id == id)
                };
                return View(viewModel);
            }
        }
        [HttpPost]
        [MyAuthorization]
        public ActionResult Address(Addresses address)
        {
            Addresses _address = null;
            if (address.Id == Guid.Empty)
            {
                address.Id = Guid.NewGuid();
                address.AddedDate = DateTime.Now;
                address.Member_Id = base.CurrentUserId();
                 db.Addresses.Add(address);
            }
            else
            {
                _address =  db.Addresses.FirstOrDefault(x => x.Id == address.Id);
                _address.ModifiedDate = DateTime.Now;
                _address.Name = address.Name;
                _address.AdresDescription = address.AdresDescription;
            }
             db.SaveChanges();
            return RedirectToAction("Profil", "Account");
        }

        [HttpGet]
        [MyAuthorization]
        public ActionResult RemoveAddress(string id)
        {
            var guid = new Guid(id);
            var address =  db.Addresses.FirstOrDefault(x => x.Id == guid);
             db.Addresses.Remove(address);
             db.SaveChanges();
            return RedirectToAction("Profil", "Account");
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var member =  db.Members.FirstOrDefault(x => x.Email == email);
            if (member == null)
            {
                ViewBag.MyError = "Böyle bir hesap bulunamadı";
                return View();
            }
            else
            {
                var body = "Şifreniz : " + member.Password;
                MyMail mail = new MyMail(member.Email, "Şifremi Unuttum", body);
                mail.SendMail();
                TempData["Info"] = email + " mail adresinize şifreniz gönderilmiştir.";
                return RedirectToAction("Login");
            }

        }

    }
}
