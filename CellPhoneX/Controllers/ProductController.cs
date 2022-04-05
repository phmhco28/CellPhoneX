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
        public JsonResult SearchPhone(string keyword)
        {
            var listResult = data.products.Where(p => p.product_name.Contains(keyword.ToLower())).ToList();
            var result = new List<product_version>();
            if (listResult == null)
            {
                return Json(result);
            }
            else
            {
                listResult = listResult.OrderByDescending(p => p.release_date).Take(5).ToList();
                foreach (var item in listResult)
                {
                    List<product_version> listPhone_version = data.product_versions.Where(p => p.product_id == item.product_id)
                                                                                   .OrderByDescending(p => p.amount)
                                                                                   .Take(2).ToList();
                    product_version resultPhone_version = listPhone_version.First();
                    result.Add(resultPhone_version);
                }
            }
            return Json(result);
        }

    }
}