using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CellPhoneX.Models;

namespace CellPhoneX.Controllers
{
    public class ProductController : Controller
    {
        CellPhoneDBDataContext data = new CellPhoneDBDataContext();
        // GET: Product
        public ActionResult ListProduct()
        {
            var all_pro = data.product_versions.ToList();

            return View(all_pro);

        }
        public ActionResult Detail(string id)
        {
            var D_Phone = data.product_versions.Where(m => m.version_id == id).First();
            var list = data.product_versions.Where(p=>p.product_id == D_Phone.product_id).ToList();
            var list2 = data.customer_comments.Where(p => p.version_id == id).ToList();
            
            ViewBag.listCom = list2;
            
            ViewBag.listProDetail = list;
            return View(D_Phone);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(FormCollection collection, product_version vs)
        {
            var C_namePro = collection["namePro"];
            var C_proID = collection["proID"];
            var C_colorID = collection["colorID"];
            var C_Memory_ram = collection["MemoryRam"];
            var C_Memory_intern = collection["MemoryIntern"];
            var C_Price = Convert.ToDecimal(collection["price"]);
            var C_special_price = Convert.ToDecimal(collection["specialPrice"]);
            var C_imgae = collection["imgae"];
            var C_amount = Convert.ToInt32(collection["amount"]);

            if (string.IsNullOrEmpty(C_namePro))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                vs.version_id = Nanoid.Nanoid.Generate(size: 10);
                vs.product_id = C_proID;
                vs.color_id = C_colorID;
                vs.memory_ram = C_Memory_ram;
                vs.memory_internal = C_Memory_intern;
                vs.price = C_Price;
                vs.special_price = C_special_price;
                vs.image = C_imgae;
                vs.amount = C_amount;
                data.product_versions.InsertOnSubmit(vs);
                data.SubmitChanges();
                return RedirectToAction("ListProduct");
            }
            return this.Create();
        }
        public ActionResult CreateBrand()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateBrand(FormCollection collection, phone_brand br)
        {
            var C_nameBrand = collection["nameBrand"];


            if (string.IsNullOrEmpty(C_nameBrand))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                br.phone_brand_id = Nanoid.Nanoid.Generate(size: 10);

                br.phone_brand_name = C_nameBrand;
                data.phone_brands.InsertOnSubmit(br);
                data.SubmitChanges();
                return RedirectToAction("Create");
            }
            return this.CreateBrand();
        }
        public ActionResult CreateSpec()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateSpec(FormCollection collection, spec sp)
        {
            var C_size = collection["size"];
            var C_type = collection["type"];
            var C_resolute = collection["resolute"];
            var C_feature = collection["feature"];
            var C_camera = collection["camera"];
            var C_camera_video = collection["camera_video"];
            var C_camera_selfie = collection["selfie"];
            var C_camera_feature = collection["camera_video"];
            var C_platform_os = collection["platform_os"];
            var C_platform_chip = collection["platform_chip"];
            var C_platform_cpu = collection["platform_cpu"];
            var C_cpmms_wlan = collection["cpmms_wl"];
            var C_cpmms_sound = collection["cpmms_sound"];
            var C_cpmms_bluetooth = collection["cpmms_bluetooth"];
            var C_cpmms_usb = collection["cpmms_usb"];
            var C_cpmms_gps = collection["cpmms_gps"];
            var C_body_size = collection["body_size"];
            var C_body_weight = collection["body_weight"];
            var C_body_sim = collection["body_sim"];
            var C_body_build = collection["body_build"];
            var C_battery_type = collection["battery_type"];
            var C_battery_charging = collection["battery_charging"];
            var C_feature_special = collection["featureSpecial"];

            sp.specs_id = Nanoid.Nanoid.Generate(size: 10);
            sp.display_size = C_size;
            sp.display_type = C_type;
            sp.display_resolution = C_resolute;
            sp.display_feature = C_feature;
            sp.main_camera = C_camera;
            sp.main_camera_video = C_camera_video;
            sp.selfie_camera = C_camera_selfie;
            sp.camera_feature = C_camera_feature;
            sp.platform_os = C_platform_os;
            sp.platform_chipset = C_platform_chip;
            sp.platform_cpu = C_platform_cpu;
            sp.comms_wlan = C_cpmms_wlan;
            sp.comms_sound_jack = C_cpmms_sound;
            sp.comms_bluetooth = C_cpmms_bluetooth;
            sp.comms_usb = C_cpmms_usb;
            sp.comms_gps = C_cpmms_gps;
            sp.body_size = C_body_size;
            sp.body_weight = C_body_weight;
            sp.body_sim = C_body_sim;
            sp.body_build = C_body_build;
            sp.battery_type = C_battery_type;
            sp.battery_charging = C_battery_charging;
            sp.feature_special = C_feature_special;
            data.specs.InsertOnSubmit(sp);
            data.SubmitChanges();
            return RedirectToAction("Create");


        }


        [HttpPost]
        public JsonResult SearchPhone(String keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return Json(null);
            }
            var result = new List<Object>();
            /*List<string> b = new List<string>();*/
            var listResult = data.products.Where(p => p.product_name.Contains(keyword.ToLower())).ToList();
            if (listResult == null)
            {
                return Json(null);
            }
            else
            {
                listResult = listResult.OrderByDescending(p => p.release_date).Take(5).ToList();
                foreach (var item in listResult)
                {
                    var resultPhone_version = data.product_versions.Where(p => p.product_id == item.product_id)
                                                                                   .OrderByDescending(p => p.amount)
                                                                                   .Take(1).Select(a => new {
                                                                                       /*id = a.version_id,*/
                                                                                       phone_name = a.product.product_name,
                                                                                       rom = a.memory_internal,
                                                                                       price = string.Format("{0:0.00}", a.price),
                                                                                       special_price = a.special_price,
                                                                                       image = a.image
                                                                                   });
                    /*b.Add(resultPhone_version.product.product_name);*/
                    result.Add(resultPhone_version);
                }
            }
            return Json(result);
        }

        public ActionResult ChooseColor(string id)
        {
            var colorPhone = data.product_versions.FirstOrDefault(p => p.color_id == id);
            return Content("");
        }
        public ActionResult AddComment(string comment, string vsId)
        {
            customer kh = (customer)Session["TaiKhoan"];
            if (kh == null)
            {
                return RedirectToAction("Login", "User");
            }
            customer_comment cm = new customer_comment();
            cm.customer_comment_id = Nanoid.Nanoid.Generate(size: 10);
            cm.version_id = vsId;
            cm.customer_id = kh.customer_id;

            cm.content = comment;
            cm.comment_date = DateTime.Now;
            data.customer_comments.InsertOnSubmit(cm);
            data.SubmitChanges();
            return RedirectToAction("Detail", new { id = vsId });
        }
    }
}