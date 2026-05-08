using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class ProductSearchCriteria
    {
        public string ProductKeyword { get; set; }//關鍵字搜尋

        public int? MinPrice { get; set; }//價格區間

        public int? MaxPrice { get; set; }//價格區間

        public bool? IsActive { get; set; }

        public int? TemperatureType { get; set; }
        // 下拉選單資料
        public List<SelectListItem> IsActiveOptions { get; set; }

        public List<SelectListItem> TemperatureTypeOptions { get; set; }
        public List<ProductViewModel> Products { get; set; }
    }
}