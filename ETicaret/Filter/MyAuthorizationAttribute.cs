using ETicaret.Web.DB;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Web.Filter
{
    public class MyAuthorizationAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public int ActionMemberType { get; private set; }
        public MyAuthorizationAttribute()
        {

        }
        /// <summary>
        /// veireln numara ve üzeri kontorl yapar
        /// </summary>
        /// <param name="_memberType">yetki numarası</param>
        public MyAuthorizationAttribute(int _memberType)
        {
            this.ActionMemberType = _memberType;
        }
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var member = (Members)HttpContext.Current.Session["LogonUser"];
            if (member == null)
            {
                filterContext.Result = new RedirectResult("/Home/Index");
            }
            else
            {
                var memberType = (int)member.MemberType;
                //0 = customer
                //10 = admin => ActionMemberType
                if (memberType < ActionMemberType)
                {
                    filterContext.Result = new RedirectResult("/Home/Index");
                }
            }
        }
    }
}