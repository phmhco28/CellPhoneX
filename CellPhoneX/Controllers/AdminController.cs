using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CellPhoneX.Models;

namespace CellPhoneX.Controllers
{
    public class AdminController : Controller
    {
        CellPhoneDBDataContext data = new CellPhoneDBDataContext();
        // GET: Admin
        public ActionResult Index()
        {
            account acc = (account)Session["TaiKhoan"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "User");
            }
            employee employee = data.employees.SingleOrDefault(p => p.account_id == acc.account_id);
            ViewBag.AdminName = employee.name;
            return View(employee);
        }

        public ActionResult Customer()
        {
            return View();
        }

        public ActionResult ProductAdmin()
        {
            return View();
        }
    }
}