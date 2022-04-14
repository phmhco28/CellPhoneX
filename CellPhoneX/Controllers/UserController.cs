using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Configuration;
using System.Web.Mvc;
using CellPhoneX.Models;


namespace CellPhoneX.Controllers
{
    public class UserController : Controller
    {
        CellPhoneDBDataContext data = new CellPhoneDBDataContext();
        // GET: User
        public ActionResult Index()
        {
            account acc = (account)Session["TaiKhoan"];
           
           if (acc == null || acc.role_id != 2 || acc.confirm == null)
            {
                return RedirectToAction("SignIn", "User");
            }
           
            customer customer = data.customers.SingleOrDefault(p => p.account_id == acc.account_id);
            return View(customer);
        }

        //EDIT thông tin khách hàng
        public ActionResult Edit(string id)
        {
            account acc = (account)Session["TaiKhoan"];

            if (acc == null || acc.role_id != 2 || acc.confirm == null)
            {
                return RedirectToAction("SignIn", "User");
            }
            /* account acc = (account)Session["TaiKhoan"];
             if (acc == null || acc.role_id != 2)
             {
                 return RedirectToAction("SignIn", "User");
             }*/
            customer customer = data.customers.SingleOrDefault(p => p.customer_id == id);
            return View(customer);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection collection, string id)
        {
            var name = collection["customer_name"];
            var birthday = collection["date_of_birth"];
            var sex = collection["sex"];
            var address = collection["address"];
            var mail = collection["mail"];
            var phone = collection["phone_number"];
            /*var pass = */ //CHANGE PASSWORD
            customer customer = data.customers.SingleOrDefault(p => p.customer_id == id);
            if (string.IsNullOrEmpty(name))
            {
                ViewData["Error"] = "Don't empty!";
                //ADD MORE
            }
            else
            {
                customer.customer_id = id;
                customer.customer_name = name;
                customer.date_of_birth = DateTime.Parse(birthday);
                customer.sex = sex;
                customer.address = address;
                customer.mail = mail;
                customer.phone_number = phone;
                UpdateModel(customer);
                data.SubmitChanges();
                return RedirectToAction("Index", "User");
            }
            return this.Edit(id);
        }

        public ActionResult SignIn()
        {
            Session["TaiKhoan"] = null;
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(FormCollection collection)
        {
            var username = collection["username"];
            var pass = collection["password"];
            customer acc = data.customers.SingleOrDefault(p => p.account.username == username && p.account.password == pass);
            if (acc != null)
            {

                Session["TaiKhoan"] = acc;
                if (acc.account.role_id == 1)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (acc.account.confirm == null)
                {
                    ViewBag.Notify = "Tài khoản không tồn tại !!!";
                    return this.SignIn();
                }
                else
                {

                    return RedirectToAction("Index", "Home");

                }
            }
            ViewBag.Notify = "Tên đăng nhập hoặc mật khẩu không đúng !!!";

            return this.SignIn();

        }

        public ActionResult ForgotPass()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ForgotPass(FormCollection collection)
        {
            var mail = collection["mail"];            
            if (mail != null || mail != "")
            {
                customer cus = data.customers.SingleOrDefault(p => p.mail == mail);
                send_mail gmail = data.send_mails.SingleOrDefault(m => m.id == "IZvaj2R3Q6");
                if (cus == null)
                {
                    ViewBag.Thongbao = "Không tìm thấy tài khoản của bạn !!!";
                    return this.ForgotPass();
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        var senderEmail = new MailAddress(gmail.mail, "CellphoneX");
                        var receiverEmail = new MailAddress(mail, "Receiver");
                        var password = gmail.pass; //lahcbhn
                        var sub = "XAC_NHAN_DOI_MAT_KHAU";
                        token token = new token();
                        token.Token1 = Nanoid.Nanoid.Generate(size: 10);
                        token.time1 = DateTime.Now;
                        token.time2 = DateTime.Now.AddMinutes(2);
                        data.tokens.InsertOnSubmit(token);
                        data.SubmitChanges();
                        var link = string.Format("{0}", Url.Action("ConfirmResetMail", "Home", new { Token = token.Token1, AccID = cus.account_id }, Request.Url.Scheme));
                        var body = "Xin chào: " + cus.customer_name + "\n" +
                                    "Vui lòng nhấn vào link này để XÁC NHẬN ĐỔI MẬT KHẨU: " + link + "\n" +
                                    "Xác nhận này chỉ có hiệu lực đến " + DateTime.Now.AddMinutes(2) + "\n" +
                                    "Xin cảm ơn quý khách !!!";

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
                    }
                }
            }
            return RedirectToAction("SignIn","User");
        }

        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(FormCollection collection)
        {
            account acc = new account();
            customer customer = new customer();
            var name = collection["name"];
            var username = collection["username"];
            var pass = collection["password"];
            var confirmPass = collection["confirm_password"];
            var email = collection["email"];
            var address = collection["address"];
            var phoneNumber = collection["phone"];
            var sex = collection["sex"];
            var dateOfBirth = string.Format("{0:MM/dd/yyyy}", collection["birthday"]);

            var acc_check_mail = data.customers.SingleOrDefault(s => s.mail == email);
            var acc_check_phone = data.customers.SingleOrDefault(s => s.phone_number == phoneNumber);
            var acc_check_username = data.accounts.SingleOrDefault(s => s.username == username);

            if (String.IsNullOrEmpty(confirmPass))
            {
                ViewData["Error_Null_ConfirmPass"] = "Vui lòng nhập mật khẩu xác nhận !!!";
                return this.SignUp();
            }
            else if (acc_check_mail != null && acc_check_phone != null && acc_check_username != null)
            {
                ViewData["Error_mail"] = "Email này đã được đăng ký !!!";
                ViewData["Error_phone"] = "Số điện thoại này đã được đăng ký !!!";
                ViewData["Error_username"] = "Tên đăng nhập đã tồn tại !!!";
                return this.SignUp();
            }
            else if (acc_check_mail != null)
            {
                ViewData["Error_mail"] = "Email này đã được đăng ký !!!";
                return this.SignUp();
            }
            else if (acc_check_phone != null)
            {
                ViewData["Error_phone"] = "Số điện thoại này đã được đăng ký !!!";
                return this.SignUp();
            }
            else if (acc_check_username != null)
            {
                ViewData["Error_username"] = "Tên đăng nhập đã tồn tại !!!";
                return this.SignUp();
            }
            else
            {
                if (!pass.Equals(confirmPass))
                {
                    ViewData["Error_Not_Same"] = "Mật khẩu không khớp !!!";
                    return this.SignUp();
                }
                else
                {
                    //ADD account
                    acc.account_id = Nanoid.Nanoid.Generate(size: 10);
                    acc.username = username;
                    acc.password = pass;
                    acc.role_id = 2; // 1: Admin 2: Khách Hàng
                    data.accounts.InsertOnSubmit(acc);
                    data.SubmitChanges();

                    //ADD customer
                    customer.customer_id = Nanoid.Nanoid.Generate(size: 10);
                    customer.customer_name = name;
                    customer.date_of_birth = DateTime.Parse(dateOfBirth);
                    customer.sex = sex;
                    customer.mail = email;
                    customer.phone_number = phoneNumber;
                    customer.address = address;
                    customer.registration_date = DateTime.Now;
                    customer.account_id = acc.account_id;
                    data.customers.InsertOnSubmit(customer);
                    data.SubmitChanges();
                    send_mail gmail = data.send_mails.SingleOrDefault(m => m.id == "IZvaj2R3Q6");
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            var senderEmail = new MailAddress(gmail.mail, "CellphoneX");
                            var receiverEmail = new MailAddress(email, "Receiver");
                            var password = gmail.pass;
                            var sub = "XAC_THUC_TAI_KHOAN";
                            token tk = new token();
                            tk.Token1 = Nanoid.Nanoid.Generate(size: 10);
                            tk.time1 = DateTime.Now;
                            tk.time2 = DateTime.Now.AddMinutes(2);
                            data.tokens.InsertOnSubmit(tk);
                            data.SubmitChanges();
                            var link = string.Format("{0}", Url.Action("ConfirmSignUp", "Home", new { Token = tk.Token1, id = acc.account_id }, Request.Url.Scheme));
                            var body = "Vui lòng click vào link để xác nhận tài khoản: " +link + "\n" +
                                        "Xác nhận này chỉ có hiệu lực đến " + DateTime.Now.AddMinutes(2) + "\n" + 
                                        "Xin cảm ơn quý khách !!!";

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
                                Body = body,

                            })
                            {
                                smtp.Send(mess);
                            }
                            return RedirectToAction("ConfirmSignup", "User");

                        }
                        catch (Exception)
                        {
                            ViewBag.Error = "Some Error";
                        }
                    }
                        
                }

                return RedirectToAction("ConfirmSignup", "User");
            }

            return RedirectToAction("Index", "User");
        }
        public ActionResult ConfirmSignup()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("SignIn", "User");
        }
    }
}