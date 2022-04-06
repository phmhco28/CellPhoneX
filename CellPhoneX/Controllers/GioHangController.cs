using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CellPhoneX.Models;

namespace CellPhoneX.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        CellPhoneDBDataContext dt = new CellPhoneDBDataContext();
        public List<Giohang> Laygiohang()
        {
            List<Giohang> lst = Session["Giohang"] as List<Giohang>;
            if (lst == null)
            {
                lst = new List<Giohang>();
                Session["Giohang"] = lst;
            }
            return lst;
        }
        public ActionResult ThemGioHang(string id, string strURL)
        {
            List<Giohang> lst = Laygiohang();
            Giohang sanpham = lst.Find(n => n.proId == id);
            if (sanpham == null)
            {
                sanpham = new Giohang(id);
                lst.Add(sanpham);
                return Redirect(strURL);
            }
            else
            {
                sanpham.amount++;
                return Redirect(strURL);
            }
           
        }
        private double Tongtien()
        {
            double tt = 0;
            List<Giohang> lst = Session["Giohang"] as List<Giohang>;
            if (lst != null)
            {
                tt = lst.Sum(n => n.thanhtien);
            }
            return tt;
        }
        public ActionResult GioHang()
        {
            List<Giohang> lst = Laygiohang();
            ViewBag.Tongtien = Tongtien();

            return View(lst);
        }
        public ActionResult GioHangPartial()
        {
            
            ViewBag.Tongtien = Tongtien();
            
            return PartialView();
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}