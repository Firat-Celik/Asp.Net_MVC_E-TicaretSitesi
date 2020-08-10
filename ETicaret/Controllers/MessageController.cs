using ETicaret.Controllers;
using ETicaret.Web.DB;
using ETicaret.Web.Filter;
using ETicaret.Web.Models.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ETicaret.Web.Controllers
{
    [MyAuthorization]
    public class MessageController : BaseController
    {

        

        // GET: Message
        [HttpGet]
        public ActionResult i()
        {
            if (IsLogon() == false) return RedirectToAction("Index", "Home");
            var currentId = CurrentUserId();
            Models.Message.iModels model = new Models.Message.iModels();
            #region - Select List Item -
            model.Users = new List<SelectListItem>();
            var users = db.Members.Where(x => ((int)x.MemberType) > 0 && x.Id != currentId).ToList();
            model.Users = users.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = string.Format("{0} {1} ({2})", x.Name, x.Surname, x.MemberType.ToString())
            }).ToList();
            #endregion
            #region - Mesaj Listesi -
            var mList = db.Messages.Where(x => x.ToMemberId == currentId || x.MessageReplies.Any(y => y.Member_Id == currentId)).ToList();
            model.Messages = mList;
            #endregion
            return View(model);
        }
        [HttpPost]
        public ActionResult SendMessage(Models.Message.SendMessageModel message)
        {
            if (IsLogon() == false) return RedirectToAction("Index", "Home");

            Messages mesaj = new Messages()
            {
                Id = Guid.NewGuid(),
                AddedDate = DateTime.Now.ToString(),
                IsRead = false,
                Subject = message.Subject,
                ToMemberId = message.ToUserId
            };
            var mRep = new MessageReplies()
            {
                Id = Guid.NewGuid(),
                AddedDate = DateTime.Now,
                Member_Id = CurrentUserId(),
                Text = message.MessageBody
            };
            mesaj.MessageReplies.Add(mRep);
            db.Messages.Add(mesaj);
            db.SaveChanges();
            return RedirectToAction("Home", "Message");
        }
        [HttpGet]
        public ActionResult MessageReplies(string id)
        {
            if (IsLogon() == false) return RedirectToAction("Index", "Home");
            var currentId = CurrentUserId();
            var guid = new Guid(id);
            Messages message = db.Messages.FirstOrDefault(x => x.Id == guid);
            if (message.ToMemberId == currentId)
            {
                message.IsRead = true;
                db.SaveChanges();
            }

            MessageRepliesModel model = new MessageRepliesModel();
            model.MReplies = db.MessageReplies.Where(x => x.MessageId == guid).OrderBy(x => x.AddedDate).ToList();
            return View(model);
        }
        [HttpPost]
        public ActionResult MessageReplies(MessageReplies message)
        {
            if (IsLogon() == false) return RedirectToAction("Index", "Home");

            message.AddedDate = DateTime.Now;
            message.Id = Guid.NewGuid();
            message.Member_Id = CurrentUserId();
            db.MessageReplies.Add(message);
            db.SaveChanges();
            return RedirectToAction("MessageReplies", "Message", new { id = message.MessageId });
        }

        [HttpGet]
        public ActionResult RenderMessage()
        {
            RenderMessageModel model = new RenderMessageModel();
            var currentId = CurrentUserId();
            var mList = db.Messages
                        .Where(x => x.ToMemberId == currentId || x.MessageReplies.Any(y => y.Member_Id == currentId))
                        .OrderByDescending(x => x.AddedDate);
            model.Messages = mList.Take(4).ToList();
            model.Count = mList.Count();

            return PartialView("_Message", model);
        }

        public ActionResult RemoveMessageReplies(string id)
        {
            var guid = new Guid(id);
            //mesja cepaları silindi
            var mReplies = db.MessageReplies.Where(x => x.MessageId == guid);
            db.MessageReplies.RemoveRange(mReplies);
            //mesajın kendisi silindi.
            var message = db.Messages.FirstOrDefault(x => x.Id == guid);
            db.Messages.Remove(message);

            db.SaveChanges();

            return RedirectToAction("Home", "Message");
        }
    }
}