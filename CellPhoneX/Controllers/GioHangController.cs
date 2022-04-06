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
                return RedirectToAction("ListProduct", "Product");
            }
            else
            {
                sanpham.amount++;
                return RedirectToAction("ListProduct", "Product");
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
        public ActionResult XoaGiohang(string id)
        {
            List<Giohang> lst = Laygiohang();
            Giohang sanpham = lst.SingleOrDefault(n => n.proId == id);
            if (sanpham != null)
            {
                lst.RemoveAll(n => n.proId == id);
                return RedirectToAction("Giohang");
            }
            return RedirectToAction("Giohang");
        }
        public ActionResult CapNhatgiohang(string id, FormCollection collection)
        {
            List<Giohang> lst = Laygiohang();
            Giohang sanpham = lst.SingleOrDefault(n => n.proId == id);

            if (sanpham != null)
            {
                product_version pro = dt.product_versions.FirstOrDefault(n => n.product_id == id);
                int amount = int.Parse(collection["txtSolg"].ToString());
                if (amount > pro.amount)
                {
                    Session["Message"] = "Không đủ số lượng";
                    return RedirectToAction("Giohang");
                }
                sanpham.amount = amount;
            }

            return RedirectToAction("Giohang");
        }
        /*  [HttpGet]*/
        /* public ActionResult DatHang()
         {
             if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
             {
                 return RedirectToAction("DangNhap", "NguoiDung");
             }
             if (Session["Giohang"] == null)
             {
                 return RedirectToAction("Index", "Home");
             }
             List<Giohang> lst = Laygiohang();
             ViewBag.Tongtien = Tongtien();
             return View(lst);
         }
         [HttpPost]
         public ActionResult DatHang(FormCollection collection)
         {
             DonHang dh = new DonHang();
             KhachHang kh = (KhachHang)Session["TaiKhoan"];
             Sach s = new Sach();
             List<Giohang> gh = Laygiohang();
             var ngaygiao = String.Format("{0:dd/MM/yyyy}", collection["NgayGiao"]);

             dh.makh = kh.makh;
             dh.ngaydat = DateTime.Now;
             dh.ngaygiao = DateTime.Parse(ngaygiao);
             dh.giaohang = false;
             dh.thanhtoan = false;
             if (dh.ngaygiao < dh.ngaydat)
             {
                 Session["MessageEx"] = "Ngay giao phải sau ngày đặt";
                 return RedirectToAction("DatHang");
             }
             else
             {
                 dt.DonHangs.InsertOnSubmit(dh);
                 dt.SubmitChanges();
             }

             foreach (var item in gh)
             {
                 ChiTietDonHang ctdh = new ChiTietDonHang();
                 ctdh.madon = dh.madon;
                 ctdh.masach = item.Masach;
                 ctdh.soluong = item.soluong;
                 ctdh.gia = (decimal)item.giaban;
                 s = dt.Saches.Single(n => n.masach == item.Masach);
                 s.soluongton -= ctdh.soluong;

                 dt.SubmitChanges();

                 dt.ChiTietDonHangs.InsertOnSubmit(ctdh);
             }
             dt.SubmitChanges();

             Session["Giohang"] = null;


             try
             {
                 var senderEmail = new MailAddress("quoctupdn@gmail.com", "Nguyễn Quốc Tú");
                 var receiverEmail = new MailAddress(kh.email, "Receiver");
                 var password = "QuocTu2907";
                 var sub = "Hello";
                 var body = "Đơn hàng đã được xác nhận";
                 var smtp = new SmtpClient
                 {
                     Host = "smtp.gmail.com",
                     Port = 587,
                     EnableSsl = true,
                     DeliveryMethod = SmtpDeliveryMethod.Network,
                     UseDefaultCredentials = false,
                     Credentials = new NetworkCredential(senderEmail.Address, password)
                 };
                 using (var mess = new MailMessage(senderEmail, receiverEmail)
                 {
                     Subject = sub,
                     Body = body
                 })
                 {
                     smtp.Send(mess);
                 }
                 return RedirectToAction("XacnhanDonhang", "GioHang");

             }
             catch (Exception)
             {
                 ViewBag.Error = "Some Error";
             }
             return RedirectToAction("XacnhanDonhang", "GioHang");
         }*/
        public ActionResult XacnhanDonhang()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }  
}