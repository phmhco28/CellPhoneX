using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
            ViewBag.soluong = Session["soluong"];
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
                /*return RedirectToAction("Giohang");*/
            }
            Session["count"] = TongSOluong();
            return Content(Session["count"].ToString());
        }
        public ActionResult CapNhatgiohang(string id, string amount)
        {
            
            List<Giohang> lst = Laygiohang();
            Giohang sanpham = lst.FirstOrDefault(n => n.proId == id);
            
            
               
            if (sanpham != null)
            {
                product_version pro = dt.product_versions.FirstOrDefault(n => n.version_id == id);
                Session["soluong"] = pro.amount;                
                if (int.Parse(amount) > pro.amount)
                {
                    Session["Message"] = "Không đủ số lượng";
                }
                else
                {
                    sanpham.amount = int.Parse(amount);
                }
                
            }

            return RedirectToAction("GioHang");
        }
        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("SignIn", "User");
            }
            if (Session["Giohang"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            List<Giohang> lst = Laygiohang();
            ViewBag.Tongtien = Tongtien();
            ViewBag.messageEx = Session["MessageEx"];
            return View(lst);
        }
        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            invoice dh = new invoice();
            customer kh = (customer)Session["TaiKhoan"];

            product_version s = new product_version();
            List<Giohang> gh = Laygiohang();
            var ngaygiao = collection["NgayGiao"];
            dh.invoice_id = Nanoid.Nanoid.Generate(size: 10);
            dh.customer_id = kh.customer_id;
            dh.order_date = DateTime.Now;
            dh.deliver_date = DateTime.Parse(ngaygiao);
            if (dh.deliver_date < dh.order_date)
            {
                Session["MessageEx"] = "Ngay giao phải sau ngày đặt";
                return RedirectToAction("DatHang");
            }
            else
            {
                dt.invoices.InsertOnSubmit(dh);
                dt.SubmitChanges();
            }

            foreach (var item in gh)
            {
                invoice_detail ctdh = new invoice_detail();
                ctdh.invoice_id = dh.invoice_id;
                ctdh.version_id = item.proId;
                ctdh.amount = item.amount;
                ctdh.price = (decimal)item.special_price;
                s = dt.product_versions.Single(n => n.version_id == item.proId);
                s.amount -= ctdh.amount;

                dt.SubmitChanges();

                dt.invoice_details.InsertOnSubmit(ctdh);
            }
            dt.SubmitChanges();

            Session["Giohang"] = null;


            try
            {
                var senderEmail = new MailAddress("quoctupdn@gmail.com", "Nguyễn Quốc Tú");
                var receiverEmail = new MailAddress(kh.mail, "Receiver");
                var password = "";
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
        }
        public ActionResult XacnhanDonhang()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
        private int TongSOluong()
        {
            int tsl = 0;
            List<Giohang> lst = Session["Giohang"] as List<Giohang>;
            if (lst != null)
            {
                tsl = lst.Count();
            }
            return tsl;
        }

        [HttpPost]

        public ActionResult AddProductAjax(string id)
        {
            List<Giohang> lst = Laygiohang();
            Giohang sanpham = lst.Find(n => n.proId == id);
            
            
            if (sanpham == null)
            {
                sanpham = new Giohang(id);
                lst.Add(sanpham);
                
            }
            else
            {
                sanpham.amount++;
               
            }
            Session["count"] = TongSOluong();
            return Content(Session["count"].ToString());

        }
    }
}