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
            var all_pro = from ss in data.product_versions select ss;
            return View(all_pro);
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

    }
}