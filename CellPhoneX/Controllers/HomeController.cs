using CellPhoneX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CellPhoneX.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            CellPhoneDBDataContext context = new CellPhoneDBDataContext();
            var phone = (from ss in context.product_versions select ss).OrderBy(m => m.amount);
            var phone2 = (from ss in context.phone_brands select ss).ToList();
            ViewBag.listPro = phone2;
            return View(phone);

        }


    }
}
