using CellPhoneX.Models;
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
        CellPhoneDBDataContext context = new CellPhoneDBDataContext();
        public ActionResult Index()
        {
            
            var phone = (from ss in context.product_versions select ss).Take(10).OrderBy(m => m.amount);
            var phone2 = (from ss in context.phone_brands select ss).ToList();
            ViewBag.listPro = phone2;
            return View(phone);

        }   
        public ActionResult BrandIndex(string id)
        {
            var phone = context.product_versions.Where(p => p.product.phone_brand_id == id).ToList();
            return View(phone);
        }
        public ActionResult Member()
        {
            return View();
        }
        public ActionResult Cus()
        {
            //lay id account
            customer acc = (customer)Session["TaiKhoan"];
            
            if ( acc == null )
            {
                return RedirectToAction("SignIn", "User");
            }
            else
            {
                var D_cus = context.customers.FirstOrDefault(p => p.account_id == acc.account_id);
                return View(D_cus);
            }    
           
            
        }
        
        public ActionResult Edit()
        {
            customer acc = (customer)Session["TaiKhoan"];
            
            if (acc == null)
            {
                return RedirectToAction("SignIn", "User");
            }
            else
            {
                var D_cus = context.customers.FirstOrDefault(p => p.account_id == acc.account_id);
                return View(D_cus);
            }
        }
        [HttpPost]
        public ActionResult Edit(FormCollection collection)
        {
            customer acc = (customer)Session["TaiKhoan"];
           
            if (acc == null)
            {
                return RedirectToAction("SignIn", "User");
            }
            else
            {
                var E_cus = context.customers.FirstOrDefault(p => p.account_id == acc.account_id);
                var Hoten = collection["customerName"];
                var SDT = collection["PhoneNumber"];
                var mail = collection["mail"];
                var sex = collection["sex"];
                var birthday = Convert.ToDateTime(collection["birthday"]);
                var address = collection["address"];

                E_cus.customer_name = Hoten;
                E_cus.phone_number = SDT;
                E_cus.mail = mail;
                E_cus.sex = sex;
                E_cus.date_of_birth= birthday;
                E_cus.address = address;

                UpdateModel(E_cus);
                context.SubmitChanges();

                return RedirectToAction("Cus","Home", new { id = acc.account_id });
            }
            
        }
        public ActionResult History()
        {
            customer acc = (customer)Session["TaiKhoan"];
            var list = context.invoices.Where(p => p.customer.account_id == acc.account_id).ToList();
            ViewBag.listIn = list;
            return View(list);
        }
        public ActionResult Details(string id)
        {
            var list = context.invoice_details.Where(p => p.invoice_id == id).ToList();
            ViewBag.listDetails = list;
            return View(list);
        }

        public ActionResult Confirm(string invoice, string Token)
        {
            token tokenConfirm = context.tokens.SingleOrDefault(p => p.Token1 == Token && DateTime.Now >= p.time1 && DateTime.Now <= p.time2);
            var inv = context.invoices.SingleOrDefault(p => p.invoice_id == invoice);
            if (tokenConfirm != null && inv != null)
            {
                inv.invoice_confirm = "Đã xác nhận";
                UpdateModel(inv);
                context.SubmitChanges();
            }
            return View(inv);
        }
        [HttpPost]
        public ActionResult ConfirmSignUp(string account, string Token)
        {
            token tokenConfirm = context.tokens.SingleOrDefault(p => p.Token1 == Token && DateTime.Now >= p.time1 && DateTime.Now <= p.time2);
            account acc = context.accounts.SingleOrDefault(p => p.account_id == account);
            if (tokenConfirm != null && acc != null)
            {
                acc.confirm = "Đã xác nhận";
                UpdateModel(acc);
                context.SubmitChanges();
            }
            return View(acc);
        }
    }
}
