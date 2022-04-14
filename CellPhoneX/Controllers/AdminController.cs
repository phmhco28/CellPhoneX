using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using CellPhoneX.Models;
using PagedList;
using PagedList.Mvc;
using System.Net.Mail;
using System.Net;
using System.Configuration;

namespace CellPhoneX.Controllers
{
    public class AdminController : Controller
    {
        CellPhoneDBDataContext data = new CellPhoneDBDataContext();
        // GET: Admin
        public ActionResult Index()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            employee employee = data.employees.SingleOrDefault(p => p.account_id == acc.account_id);
            var get_all_invoice = data.invoices.Where(p => p.active_status == null || p.active_status == true).Where(p => p.invoice_confirm == null || p.invoice_confirm == "" || p.invoice_confirm == "Chưa xác nhận").ToList().OrderBy(p => p.order_date);
            ViewBag.AdminName = employee.name;
            decimal totalDay = 0;
            decimal totalMonth = 0;
            var invList = data.invoices.Where(p => p.active_status == null || p.active_status == true ).Where(p => p.invoice_status_pay == "Đã thanh toán").ToList();
            DateTime date = DateTime.Now;
            foreach (var item in invList)
            {
                string day = item.order_date.ToShortDateString();
                if (day == date.ToShortDateString())
                {
                    totalDay += data.invoice_details.Where(p => p.invoice_id == item.invoice_id).Sum(s => s.price * s.amount);
                }
            }

            foreach(var item in invList)
            {
                string day = string.Format("{0:MM-yyyy}", item.order_date);
                if (day == string.Format("{0:MM-yyyy}", date))
                {
                    totalMonth += data.invoice_details.Where(p => p.invoice_id == item.invoice_id).Sum(s => s.price * s.amount);
                }
            }
            ViewBag.DoanhThuNgay = string.Format("{0:000,000}",totalDay);
            ViewBag.DoanhThuThang = string.Format("{0:000,000}", totalMonth);
            return View(get_all_invoice);
        }
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(FormCollection collection)
        {
            var username = collection["username"];
            var pass = collection["password"];
            if (String.IsNullOrEmpty(username) && String.IsNullOrEmpty(pass))
            {
                ViewData["Error_null_username"] = "Vui lòng điền tên đăng nhập";
                ViewData["Error_null_pass"] = "Vui lòng nhập mật khẩu";
            }
            else if (String.IsNullOrEmpty(username))
            {
                ViewData["Error_null_username"] = "Vui lòng điền tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(pass))
            {
                ViewData["Error_null_pass"] = "Vui lòng nhập mật khẩu";
            }
            else
            {
                account acc = data.accounts.SingleOrDefault(p => p.username == username && p.password == pass);
                if (acc != null)
                {
                    Session["TaiKhoanAdmin"] = acc;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return this.SignIn();
        }

        //DONE-------------------------------Quản lý Tài khoản -------------------------------------------------------------------
        public ActionResult Account()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_account = data.accounts.ToList();
            return View(get_all_account);
        }

        public ActionResult CreateAccount()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            ViewBag.Role = new SelectList(data.roles.ToList(), "role_id", "role_name");
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(FormCollection collection)
        {
            account newAcc = new account();
            newAcc.account_id = Nanoid.Nanoid.Generate(size: 10);
            newAcc.username = collection["username"];
            newAcc.password = collection["password"];
            newAcc.role_id = int.Parse(collection["role"]);
            data.accounts.InsertOnSubmit(newAcc);
            data.SubmitChanges();
            return RedirectToAction("Account", "Admin");
        }

        [HttpGet]
        public ActionResult DeleteAccount(string id)
        {
            account accSigin = (account)Session["TaiKhoanAdmin"];
            if (accSigin == null || accSigin.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            account acc = data.accounts.SingleOrDefault(p => p.account_id == id);
            return View(acc);
        }

        [HttpPost]
        public ActionResult DeleteAccount(string id, FormCollection collection)
        {
            account acc = data.accounts.SingleOrDefault(p => p.account_id == id);
            employee employeeAdmin = data.employees.SingleOrDefault(p => p.account_id == id);
            if (employeeAdmin != null)
            {
                employeeAdmin.account_id = null;
                UpdateModel(employeeAdmin);
                data.SubmitChanges();
            }
            data.accounts.DeleteOnSubmit(acc);
            data.SubmitChanges();
            return RedirectToAction("Account", "Admin");
        }

        public ActionResult EditAccount(string id)
        {
            account accSign = (account)Session["TaiKhoanAdmin"];
            if (accSign == null || accSign.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            account acc = data.accounts.SingleOrDefault(p => p.account_id == id);
            return View(acc);
        }

        [HttpPost]
        public ActionResult EditAccount(string id, FormCollection collection)
        {
            account acc = data.accounts.SingleOrDefault(p => p.account_id == id);
            acc.password = collection["password"];
            UpdateModel(acc);
            data.SubmitChanges();
            return RedirectToAction("Account", "Admin");
        }
        //DONE--------------------------------------Ket thuc Quan ly Account -------------------------------

        //DONE-------------------------------------Quan ly Phone Brand --------------------------------------
        public ActionResult PhoneBrand()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_brand = data.phone_brands.ToList().OrderBy(p => p.active_status);
            return View(get_all_brand);
        }

        public ActionResult CreatePhoneBrand()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            return View();
        }

        [HttpPost]
        public ActionResult CreatePhoneBrand(FormCollection collection)
        {
            phone_brand pb = new phone_brand();
            pb.phone_brand_id = Nanoid.Nanoid.Generate(size: 10);
            pb.phone_brand_name = collection["phone_brand_name"];
            data.phone_brands.InsertOnSubmit(pb);
            data.SubmitChanges();
            return RedirectToAction("PhoneBrand", "Admin");
        }

        public ActionResult DeletePhoneBrand(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            phone_brand pb = data.phone_brands.SingleOrDefault(p => p.phone_brand_id == id);
            return View(pb);
        }

        [HttpPost]
        public ActionResult DeletePhoneBrand(string id, FormCollection collection)
        {
            phone_brand pb = data.phone_brands.SingleOrDefault(p => p.phone_brand_id == id);
            pb.active_status = false;
            UpdateModel(pb);
            data.SubmitChanges();
            return RedirectToAction("PhoneBrand", "Admin");
        }

        public ActionResult EditPhoneBrand(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            phone_brand pb = data.phone_brands.SingleOrDefault(p => p.phone_brand_id == id);
            return View(pb);
        }

        [HttpPost]
        public ActionResult EditPhoneBrand(string id, FormCollection collection)
        {
            phone_brand pb = data.phone_brands.SingleOrDefault(p => p.phone_brand_id == id);
            pb.phone_brand_name = collection["phone_brand_name"];
            UpdateModel(pb);
            data.SubmitChanges();
            return RedirectToAction("PhoneBrand", "Admin");
        }
        //DONE-----------------------------------Ket thuc Quan ly Phone Brand -----------------------------------------

        //DONE-----------------------------------Quan ly khach hang -----------------------------
        public ActionResult Customer()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_customer = data.customers.ToList().OrderBy(p => p.customer_name);
            return View(get_all_customer);
        }


        public ActionResult DeleteCustomer(string id)
        {
            customer customer = data.customers.SingleOrDefault(p => p.customer_id == id);
            return View(customer);
        }

        [HttpPost]
        public ActionResult DeleteCustomer(string id, FormCollection collection)
        {
            customer customer = data.customers.SingleOrDefault(p => p.customer_id == id);
            customer.active_status = false;
            UpdateModel(customer);
            data.SubmitChanges();
            return RedirectToAction("Customer", "Admin");
        }
        //DONE-----------------------------Ket thuc quan ly khach hang--------------------------

        //DONE---------------------------- Quan ly nhan vien ----------------------------
        public ActionResult Employee()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_employee = data.employees.ToList().OrderBy(p => p.name);
            return View(get_all_employee);
        }

        public ActionResult CreateEmployee()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            return View();
        }

        [HttpPost]
        public ActionResult CreateEmployee(FormCollection collection)
        {
            employee emp = new employee();
            emp.employee_id = Nanoid.Nanoid.Generate(size: 10);
            emp.name = collection["name"];
            emp.date_of_birth = DateTime.Parse(collection["date_of_birth"]);
            emp.sex = collection["sex"];
            emp.address = collection["address"];
            emp.id_card = collection["id_card"];
            emp.mail = collection["mail"];
            emp.phone_number = collection["phone_number"];
            emp.registration_date = DateTime.Now;
            var accountID = collection["account_id"];
            var checkL = data.accounts.Where(p => p.account_id == accountID).ToList();
            if (accountID == null || accountID == "" || checkL.Count == 0)
            {
                emp.account_id = null;
            }
            else
            {
                account check = data.accounts.FirstOrDefault(p => p.account_id == accountID);
                if (check == null)
                {
                    ViewData["Error_not_found_acc"] = "Không tìm thấy tài khoản";
                    return this.CreateAccount();
                }
                else
                {
                    emp.account_id = accountID;
                }
            }
            data.employees.InsertOnSubmit(emp);
            data.SubmitChanges();
            return RedirectToAction("Employee", "Admin");
        }

        public ActionResult DetailEmployee(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            employee emp = data.employees.SingleOrDefault(p => p.employee_id == id);
            return View(emp);
        }

        public ActionResult DeleteEmployee(string id)
        {
            employee emp = data.employees.SingleOrDefault(p => p.employee_id == id);
            return View(emp);
        }

        [HttpPost]
        public ActionResult DeleteEmployee(string id, FormCollection collection)
        {
            employee emp = data.employees.SingleOrDefault(p => p.employee_id == id);
            emp.active_status = false;
            UpdateModel(emp);
            data.SubmitChanges();
            return RedirectToAction("Employee", "Admin");
        }

        public ActionResult EditEmployee(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            employee emp = data.employees.SingleOrDefault(p => p.employee_id == id);
            return View(emp);
        }

        [HttpPost]
        public ActionResult EditEmployee(string id, FormCollection collection)
        {
            employee emp = data.employees.SingleOrDefault(p => p.employee_id == id);
            emp.name = collection["name"];
            emp.date_of_birth = DateTime.Parse(collection["date_of_birth"]);
            emp.sex = collection["sex"];
            emp.address = collection["address"];
            emp.id_card = collection["id_card"];
            emp.mail = collection["mail"];
            emp.phone_number = collection["phone_number"];
            emp.account_id = collection["account_id"]; /**********/
            UpdateModel(emp);
            data.SubmitChanges();
            return RedirectToAction("Employee", "Admin");
        }
        //DONE---------------------------------------Ket thuc quan ly nhan vien ----------------------------

        //DONE----------------------------Quan ly san pham ----------------------------
        public ActionResult Product()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_product = data.products.ToList().OrderBy(p => p.active_status);
            return View(get_all_product);
        }

        public ActionResult CreateProduct()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            ViewBag.phoneBrand = new SelectList(data.phone_brands.Where(s => s.active_status == null || s.active_status ==true).ToList().OrderBy(p => p.phone_brand_name), "phone_brand_id", "phone_brand_name");
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            return View();
        }

        [HttpPost]
        public ActionResult CreateProduct(FormCollection collection)
        {
            ViewBag.phoneBrand = new SelectList(data.phone_brands.Where(s => s.active_status == null || s.active_status == true).ToList().OrderBy(p => p.phone_brand_name), "phone_brand_id", "phone_brand_name");
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            product product = new product();
            product.product_id = Nanoid.Nanoid.Generate(size: 10);
            product.product_name = collection["product_name"];
            product.release_date = DateTime.Parse(collection["release_date"]);
            product.specs_id = collection["specs_id"];
            product.phone_brand_id = collection["phoneBrand"];
            product.description = collection["description"];
            var promotionID = collection["promotion"];
            var check = data.promotions.Where(p => p.promotion_id == promotionID).ToList();
            if (check.Count == 0)
            {
                product.promotion_id = null;
            }
            else
                product.promotion_id = promotionID;
            data.products.InsertOnSubmit(product);
            data.SubmitChanges();
            return RedirectToAction("Product", "Admin");
        }

        public ActionResult DeleteProduct(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            product product = data.products.SingleOrDefault(p => p.product_id == id);
            return View(product);
        }

        [HttpPost]
        public ActionResult DeleteProduct(string id, FormCollection collection)
        {
            product product = data.products.SingleOrDefault(p => p.product_id == id);
            product.active_status = false;
            UpdateModel(product);
            data.SubmitChanges();
            return RedirectToAction("Product", "Admin");
        }

        public ActionResult EditProduct(string id)
        {
            ViewBag.phoneBrand = new SelectList(data.phone_brands.Where(s => s.active_status == null || s.active_status == true).ToList().OrderBy(p => p.phone_brand_name), "phone_brand_id", "phone_brand_name");
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
          
            product product = data.products.SingleOrDefault(p => p.product_id == id);
            return View(product);
        }

        [HttpPost]
        public ActionResult EditProduct(string id, FormCollection collection)
        {
            ViewBag.phoneBrand = new SelectList(data.phone_brands.Where(s => s.active_status == null || s.active_status == true).ToList().OrderBy(p => p.phone_brand_name), "phone_brand_id", "phone_brand_name");
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            product product = data.products.SingleOrDefault(p => p.product_id == id);
            product.product_name = collection["product_name"];
            product.specs_id = collection["specs_id"];
            product.phone_brand_id = collection["phoneBrand"];
            product.description = collection["description"];
            var promotionID = collection["promotion"];
            var check = data.promotions.Where(p => p.promotion_id == promotionID).ToList();
            if (check.Count == 0)
            {
                product.promotion_id = null;
            }
            else
                product.promotion_id = promotionID;
            UpdateModel(product);
            data.SubmitChanges();
            return RedirectToAction("Product", "Admin");
        }
        //DONE----------------------------Ket thuc Quan ly san pham ---------------------------

        //DONE-----------------------------Quan ly các phien ban san pham ----------------------
        public ActionResult ProductVersion(int? page)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 3;
            var get_all_version = data.product_versions.ToList();
            return View(get_all_version.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult CreateProductVersion()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            ViewBag.product = new SelectList(data.products.Where(p => p.active_status == true || p.active_status == null).ToList().OrderBy(p => p.product_name), "product_id", "product_name");
            ViewBag.color = new SelectList(data.colors.OrderBy(d => d.color_name).Where(p => (p.active_status == true || p.active_status == null)).ToList(), "color_id", "color_name");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateProductVersion(FormCollection collection, HttpPostedFileBase file)
        {
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            ViewBag.product = new SelectList(data.products.Where(p => (p.active_status == true || p.active_status == null)).ToList().OrderBy(p => p.product_name), "product_id", "product_name");
            ViewBag.color = new SelectList(data.colors.OrderBy(d => d.color_name).Where(p => (p.active_status == true || p.active_status == null)).ToList(), "color_id", "color_name");
            if (file == null)
            {
                ViewBag.Thongbao = "Vui lòng chọn ảnh !!!";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Thongbao = "Hình đã tồn tại";
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                    product_version version = new product_version();
                    version.version_id = Nanoid.Nanoid.Generate(size: 10);
                    version.product_id = collection["product"];
                    version.color_id = collection["color"];
                    version.memory_ram = collection["memory_ram"];
                    version.memory_internal = collection["memory_internal"];
                    if (collection["price"] == null || collection["price"] == "")
                    {
                        version.price = 0;
                    }
                    else
                        version.price = decimal.Parse(collection["price"]);
                    if (collection["special_price"] == null || collection["special_price"] == "")
                    {
                        version.special_price = 0;
                    }
                    else
                        version.special_price = decimal.Parse(collection["special_price"]);
                    version.image = fileName;
                    if (collection["amount"] == null || collection["amount"] == "")
                    {
                        version.amount = 0;
                    }
                    else
                        version.amount = int.Parse(collection["amount"]);
                    version.active_status = true;
                    var promotionID = collection["promotion_id"];
                    var check = data.promotions.Where(p => p.promotion_id == promotionID).ToList();
                    if (check.Count == 0)
                    {
                        version.promotion_id = null;
                    }
                    else
                        version.promotion_id = promotionID;
                    data.product_versions.InsertOnSubmit(version);
                    data.SubmitChanges();

                }
            }
            return RedirectToAction("ProductVersion", "Admin");
        }

        public ActionResult DeleteProductVersion(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            product_version product = data.product_versions.SingleOrDefault(p => p.version_id == id);
            return View(product);
        }

        [HttpPost]
        public ActionResult DeleteProductVersion(string id, FormCollection collection)
        {
            product_version product = data.product_versions.SingleOrDefault(p => p.version_id == id);
            product.active_status = false;
            UpdateModel(product);
            data.SubmitChanges();
            return RedirectToAction("ProductVersion", "Admin");
        }

        public ActionResult EditProductVersion(FormCollection collection, HttpPostedFileBase file, string id)
        {
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            ViewBag.product = new SelectList(data.products.Where(p => p.active_status == true || p.active_status == null).ToList().OrderBy(p => p.product_name), "product_id", "product_name");
            ViewBag.color = new SelectList(data.colors.OrderBy(d => d.color_name).Where(p => (p.active_status == true || p.active_status == null)).ToList(), "color_id", "color_name");
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            product_version version = data.product_versions.SingleOrDefault(p => p.version_id == id);
            return View(version);
        }

        [HttpPost]
        public ActionResult EditProductVersion(string id, FormCollection collection)
        {
            ViewBag.promotion = new SelectList(data.promotions.Where(p => p.end_date >= DateTime.Now).OrderBy(d => d.end_date).ToList(), "promotion_id", "promotion_id");
            ViewBag.product = new SelectList(data.products.Where(p => p.active_status == true || p.active_status == null).ToList().OrderBy(p => p.product_name), "product_id", "product_name");
            ViewBag.color = new SelectList(data.colors.OrderBy(d => d.color_name).Where(p => (p.active_status == true || p.active_status == null)).ToList(), "color_id", "color_name");
            product_version version = data.product_versions.SingleOrDefault(p => p.version_id == id);
            if (collection["price"] == null || collection["price"] == "")
                {
                    version.price = 0;
                }
                else
                    version.price = decimal.Parse(collection["price"]);
                if (collection["special_price"] == null || collection["special_price"] == "")
                {
                    version.special_price = 0;
                }
                else
                    version.special_price = decimal.Parse(collection["special_price"]);
                if (collection["amount"] == null || collection["amount"] == "")
                {
                    version.amount = 0;
                }
                else
                    version.amount = int.Parse(collection["amount"]);
                var promotionID = collection["promotion_id"];
                var check = data.promotions.Where(p => p.promotion_id == promotionID).ToList();
                if (check.Count == 0)
                {
                    version.promotion_id = null;
                }
                else
                    version.promotion_id = promotionID;
                UpdateModel(version);
                data.SubmitChanges();
            return RedirectToAction("ProductVersion", "Admin");
        }
        //DONE-----------------------------Ket thuc Quan ly các phien ban san pham ----------------------

        //DONE-----------------------------Quan ly thong so ky thuat ---------------------------------
        public ActionResult Spec()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_specs = data.specs.ToList().OrderBy(p => p.active_status);
            return View(get_all_specs);
        }

        public ActionResult DetailSpec(string id)
        {
            spec spec = data.specs.SingleOrDefault(p => p.specs_id == id);
            List<product> productList = data.products.Where(p => p.specs_id == id).ToList();
            string phoneName = "";
            foreach (var item in productList)
            {
                phoneName += item.product_name + " | ";
            }
            ViewBag.Product = phoneName;
            return View(spec);
        }

        public ActionResult CreateSpec()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateSpec(FormCollection collection)
        {
            spec sp = new spec();
            sp.specs_id = Nanoid.Nanoid.Generate(size: 10);
            sp.display_size = collection["display_size"];
            sp.display_type = collection["display_type"];
            sp.display_resolution = collection["display_resolution"];
            sp.display_feature = collection["display_resolution"];
            sp.main_camera = collection["main_camera"];
            sp.main_camera_video = collection["main_camera_video"];
            sp.selfie_camera = collection["selfie_camera"];
            sp.camera_feature = collection["camera_feature"];
            sp.platform_os = collection["platform_os"];
            sp.platform_chipset = collection["platform_chipset"];
            sp.platform_cpu = collection["platform_cpu"];
            sp.platform_gpu = collection["platform_gpu"];
            sp.comms_wlan = collection["comms_wlan"];
            sp.comms_sound_jack = collection["comms_sound_jack"];
            sp.comms_bluetooth = collection["comms_bluetooth"];
            sp.comms_usb = collection["comms_usb"];
            sp.comms_gps = collection["comms_gps"];
            sp.body_size = collection["body_size"];
            sp.body_weight = collection["body_weight"];
            sp.body_sim = collection["body_sim"];
            sp.body_build = collection["body_build"];
            sp.battery_type = collection["battery_type"];
            sp.battery_charging = collection["battery_charging"];
            sp.feature_special = collection["feature_special"];
            data.specs.InsertOnSubmit(sp);
            data.SubmitChanges();
            return RedirectToAction("Spec", "Admin");
        }

        public ActionResult DeleteSpec(string id)
        {
            spec spec = data.specs.SingleOrDefault(p => p.specs_id == id);
            List<product> productList = data.products.Where(p => p.specs_id == id).ToList();
            string phoneName = "";
            foreach (var item in productList)
            {
                phoneName += item.product_name + " | ";
            }
            ViewBag.Product = phoneName;
            return View(spec);
        }

        [HttpPost]
        public ActionResult DeleteSpec(string id, FormCollection collection)
        {
            spec sp = data.specs.SingleOrDefault(p => p.specs_id == id);
            sp.active_status = false;
            UpdateModel(sp);
            data.SubmitChanges();
            return RedirectToAction("Spec", "Admin");
        }

        public ActionResult EditSpec(string id)
        {
            spec spec = data.specs.SingleOrDefault(p => p.specs_id == id);
            List<product> productList = data.products.Where(p => p.specs_id == id).ToList();
            string phoneName = "";
            foreach (var item in productList)
            {
                phoneName += item.product_name + " | ";
            }
            ViewBag.Product = phoneName;
            return View(spec);
        }

        [HttpPost]
        public ActionResult EditSpec(string id, FormCollection collection)
        {
            spec sp = data.specs.SingleOrDefault(p => p.specs_id == id);
            sp.display_size = collection["display_size"];
            sp.display_type = collection["display_type"];
            sp.display_resolution = collection["display_resolution"];
            sp.display_feature = collection["display_resolution"];
            sp.main_camera = collection["main_camera"];
            sp.main_camera_video = collection["main_camera_video"];
            sp.selfie_camera = collection["selfie_camera"];
            sp.camera_feature = collection["camera_feature"];
            sp.platform_os = collection["platform_os"];
            sp.platform_chipset = collection["platform_chipset"];
            sp.platform_cpu = collection["platform_cpu"];
            sp.platform_gpu = collection["platform_gpu"];
            sp.comms_wlan = collection["comms_wlan"];
            sp.comms_sound_jack = collection["comms_sound_jack"];
            sp.comms_bluetooth = collection["comms_bluetooth"];
            sp.comms_usb = collection["comms_usb"];
            sp.comms_gps = collection["comms_gps"];
            sp.body_size = collection["body_size"];
            sp.body_weight = collection["body_weight"];
            sp.body_sim = collection["body_sim"];
            sp.body_build = collection["body_build"];
            sp.battery_type = collection["battery_type"];
            sp.battery_charging = collection["battery_charging"];
            sp.feature_special = collection["feature_special"];
            UpdateModel(sp);
            data.SubmitChanges();
            return RedirectToAction("Spec", "Admin");
        }
        //DONE----------------------------------------Ket thuc quan ly thong so ky thuat -----------------------------

        //DONE-------------------------------------Quan ly hoa don---------------------------------------
        public ActionResult Invoice()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_invoice = data.invoices.ToList().OrderBy(p => p.order_date);
            return View(get_all_invoice);
        }

        public ActionResult DetailInvoice(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var invD = data.invoice_details.Where(p => p.invoice_id == id).ToList();
            var invoice = data.invoices.SingleOrDefault(p => p.invoice_id == id);
            //send to View
            ViewBag.InvoiceID = invoice.invoice_id;
            ViewBag.OrderDate = invoice.order_date.ToShortDateString();
            ViewBag.DeliverDate = invoice.deliver_date.ToString();
            ViewBag.Note = invoice.note;
            ViewBag.Customer = invoice.customer.customer_name;
            if (invoice.active_status == null || invoice.active_status == true)
            {
                ViewBag.ActiveStatus = "Còn hiệu lực";
            }
            else
            {
                ViewBag.ActiveStatus = "Đã xóa";
            }
            if (invoice.invoice_confirm == null || invoice.invoice_confirm == "")
            {
                ViewBag.InvoiceConfirm = "Chưa xác nhận";
            }
            else
            {
                ViewBag.InvoiceConfirm = "Đã xác nhận";
            }
            if (invoice.invoice_status_pay == null || invoice.invoice_status_pay == "")
            {
                ViewBag.InvoicePay = "Chưa thanh toán";
            }
            else
            {
                ViewBag.InvoicePay = "Đã thanh toán";
            }
            return View(invD);
        }

        public ActionResult DeleteInvoice(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var invD = data.invoice_details.Where(p => p.invoice_id == id).ToList();
            var invoice = data.invoices.SingleOrDefault(p => p.invoice_id == id);
            //send to View
            ViewBag.InvoiceID = invoice.invoice_id;
            ViewBag.OrderDate = invoice.order_date.ToShortDateString();
            ViewBag.DeliverDate = invoice.deliver_date.ToString();
            ViewBag.Note = invoice.note;
            ViewBag.Customer = invoice.customer.customer_name;
            if (invoice.active_status == null || invoice.active_status == true)
            {
                ViewBag.ActiveStatus = "Còn hiệu lực";
            }
            else
            {
                ViewBag.ActiveStatus = "Đã xóa";
            }
            if (invoice.invoice_confirm == null || invoice.invoice_confirm == "")
            {
                ViewBag.InvoiceConfirm = "Chưa xác nhận";
            }
            else
            {
                ViewBag.InvoiceConfirm = "Đã xác nhận";
            }
            if (invoice.invoice_status_pay == null || invoice.invoice_status_pay == "")
            {
                ViewBag.InvoicePay = "Chưa thanh toán";
            }
            else
            {
                ViewBag.InvoicePay = "Đã thanh toán";
            }
            return View(invD);
        }

        [HttpPost]
        public ActionResult DeleteInvoice(string id, FormCollection collection)
        {
            invoice inv = data.invoices.SingleOrDefault(p => p.invoice_id == id);
            inv.active_status = false;
            UpdateModel(inv);
            data.SubmitChanges();
            return RedirectToAction("Invoice", "Admin");
        }

        public ActionResult EditInvoice(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var invD = data.invoice_details.Where(p => p.invoice_id == id).ToList();
            var invoice = data.invoices.SingleOrDefault(p => p.invoice_id == id);
            //send to View
            if (invoice.active_status == null || invoice.active_status == true)
            {
                ViewBag.ActiveStatus = "Còn hiệu lực";
            }
            else
            {
                ViewBag.ActiveStatus = "Đã xóa";
            }
            if (invoice.invoice_confirm == null || invoice.invoice_confirm == "")
            {
                ViewBag.InvoiceConfirm = "Chưa xác nhận";
            }
            else
            {
                ViewBag.InvoiceConfirm = "Đã xác nhận";
            }
            if (invoice.invoice_status_pay == null || invoice.invoice_status_pay == "")
            {
                ViewBag.InvoicePay = "Chưa thanh toán";
            }
            else
            {
                ViewBag.InvoicePay = "Đã thanh toán";
            }
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "Đã thanh toán", Value = "Đã thanh toán"},
                new SelectListItem{Text = "Chưa thanh toán", Value = null}
            };
            ViewData["Pay"] = items;

            return View(invoice);
        }

        [HttpPost]
        public ActionResult EditInvoice(string id, FormCollection collection)
        {
            invoice inv = data.invoices.SingleOrDefault(p => p.invoice_id == id);
            inv.deliver_date = DateTime.Parse(collection["deliver_date"]);
            inv.note = collection["note"];
            var status_pay = collection["Pay"];
            if (inv.invoice_confirm == null || inv.invoice_confirm == "")
            {
                if (status_pay != null || status_pay != "")
                {
                    ViewBag.Notify = "Đon hàng chưa được xác nhận ! Vui lòng kiểm tra lại !!!";
                    return this.EditInvoice(id);
                }
            }
            inv.invoice_status_pay = status_pay;
            UpdateModel(inv);
            data.SubmitChanges();
            return RedirectToAction("Invoice", "Admin");
        }

        public ActionResult ConfirmInvoice(string id)
        {
            invoice inv = data.invoices.SingleOrDefault(p => p.invoice_id == id);
            var phoneInfo = "";
            decimal total = 0;
            foreach (var item in data.invoice_details.Where(p => p.invoice_id == id).ToList())
            {
                phoneInfo += item.product_version.product.product_name + " "
                            + item.product_version.memory_internal + " RAM "
                            + item.product_version.memory_internal + " bộ nhớ trong"
                            + " Màu " + item.product_version.color.color_name
                            + " có giá: " + item.price + " | Số lượng: "+item.amount +"\n";
                total += item.amount * item.price;
            }
            send_mail mail = data.send_mails.SingleOrDefault(m => m.id == "IZvaj2R3Q6");
            customer cus = data.customers.FirstOrDefault(p => p.customer_id == inv.customer_id);
           if (ModelState.IsValid)
            {
                var senderEmail = new MailAddress(mail.mail, "CellphoneX");
                var receiverEmail = new MailAddress(cus.mail, "Receiver");
                var password = mail.pass; 
                var sub = "XAC_NHAN_DON_HANG";
                token token = new token();
                token.Token1 = Nanoid.Nanoid.Generate(size: 10);
                token.time1 = DateTime.Now;
                token.time2 = DateTime.Now.AddMinutes(10);
                data.tokens.InsertOnSubmit(token);
                data.SubmitChanges();
                var link = string.Format("{0}", Url.Action("Confirm", "Home", new { Token = token.Token1, invoice = inv.invoice_id }, Request.Url.Scheme));
                var body = "Tên khách hàng: " + cus.customer_name + "\n" +
                            "Địa chỉ: " + cus.address + "\n" +
                            "Số điện thoại: " + cus.phone_number + "\n" +
                            "Ngày đặt hàng: " + inv.order_date + "\n" +
                            "Ngày giao hàng (dự kiến): " + inv.deliver_date + "\n" +
                            "Cụ thể: " + phoneInfo + "\n" +
                            "Tổng tiền hàng: " + total.ToString() + " VNĐ" + "\n" +
                            "Vui lòng nhấn vào link này để XÁC NHẬN ĐƠN HÀNG:" + link + "\n" +
                            "Xác nhận này chỉ có hiệu lực đến " + DateTime.Now.AddMinutes(10) + "\n" +
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
            return RedirectToAction("Invoice","Admin");
        }

        //DONE------------------------------------Quan ly HOa don-------------------------------------

        //DONE-----------------------------------------Quan ly mau sac dien thoai ---------------------------
        public ActionResult Color()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            var get_all_color = data.colors.ToList().OrderBy(p =>p.active_status);
            return View(get_all_color);
        }

        public ActionResult CreateColor()
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            return View();
        }

        [HttpPost]
        public ActionResult CreateColor(FormCollection collection)
        {
            color color = new color();
            color.color_id = Nanoid.Nanoid.Generate(size: 10);
            color.color_name = collection["color_name"];
            color.description = collection["description"];
            
            if (collection["color_name"] == null || collection["color_name"] == "")
            {
                ViewBag.ThongBao = "Vui lòng nhập tên màu !!!";
                return this.CreateColor();
            }
            data.colors.InsertOnSubmit(color);
            data.SubmitChanges();
            return RedirectToAction("Color", "Admin");
        }

        public ActionResult EditColor(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            color color = data.colors.SingleOrDefault(p => p.color_id == id);
            return View(color);
        }

        [HttpPost]
        public ActionResult EditColor(string id, FormCollection collection)
        {
            color color = data.colors.SingleOrDefault(p => p.color_id == id);
            color.description = collection["description"];
            UpdateModel(color);
            data.SubmitChanges();
            return RedirectToAction("Color", "Admin");
        }

        public ActionResult DeleteColor(string id)
        {
            account acc = (account)Session["TaiKhoanAdmin"];
            if (acc == null || acc.role_id != 1)
            {
                return RedirectToAction("SignIn", "Admin");
            }
            color color = data.colors.SingleOrDefault(p => p.color_id == id);
            return View(color);
        }

        [HttpPost]
        public ActionResult DeleteColor(string id, FormCollection collection)
        {
            color color = data.colors.SingleOrDefault(p => p.color_id == id);
            color.active_status = false;
            UpdateModel(color);
            data.SubmitChanges();
            return RedirectToAction("Color","Admin");
        }
        //DONE-----------------------------------------------------Color ----------------------------------------
    }
}