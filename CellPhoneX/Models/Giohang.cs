using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CellPhoneX.Models;

namespace CellPhoneX.Models
{
    
    public class Giohang
    {
        CellPhoneDBDataContext context = new CellPhoneDBDataContext();
        public string proId { get; set; }
        
        public string namePro { get; set; }
       
        public string image { get; set; }
       
        public Double price { get; set; }

        public Double special_price { get; set; }
        
        public int amount { get; set; }
        
        public Double thanhtien
        {
            get { return amount * special_price; }
        }
        public Giohang(string id)
        {
            proId = id;
            product_version pro = context.product_versions.Single(n => n.version_id == proId);
            namePro = pro.product.product_name +"-"+pro.memory_internal;
            image = pro.image;
            price = double.Parse(pro.price.ToString());
            special_price = double.Parse(pro.special_price.ToString());
            amount = 1;
        }
    }
}