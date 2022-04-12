using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CellPhoneX.Models;

namespace CellPhoneX.Controllers
{
    public class HomeController : Controller
    {
        CellPhoneDBDataContext data = new CellPhoneDBDataContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Confirm(string invoice, string Token)
        {
            token tokenConfirm = data.tokens.SingleOrDefault(p => p.Token1 == Token && DateTime.Now >= p.time1 && DateTime.Now <= p.time2);
            var inv = data.invoices.SingleOrDefault(p => p.invoice_id == invoice);
            if (tokenConfirm != null && inv != null)
            {
                inv.invoice_confirm = "Đã xác nhận";
                UpdateModel(inv);
                data.SubmitChanges();
            }
            return View(inv);
        }
    }
}