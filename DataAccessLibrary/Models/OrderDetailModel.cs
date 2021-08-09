using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class OrderDetailModel
    {
        [Key]
        public int id { get; set; }
        public int order_id { get; set; }
        public int? order_status_id { get; set; }
        public int? product_id { get; set; }
        public int? qty { get; set; }
        public double unit_price { get; set; }
        public double discount_amount { get; set; }
        public double discount_percent { get; set; }
        public double total_amount { get; set; }
        public DateTime created_date { get; set; }
        public int? created_user_id { get; set; }
        public DateTime updated_date { get; set; }
        public int? updated_user_id { get; set; }
        [Editable(false)]
        public string product_name { get; set; }
        [Editable(false)]
        public string product_img_path { get; set; }
        [Editable(false)]
        public string currency_symbol { get; set; }
        [Editable(false)]
        public string order_status_name { get; set; }
        [Editable(false)]
        public DateTime? accepted_date { get; set; }
        [Editable(false)]
        public DateTime? preparing_date { get; set; }
        [Editable(false)]
        public DateTime? arriving_date { get; set; }
        [Editable(false)]
        public DateTime? delivered_date { get; set; }
        [Editable(false)]
        public DateTime? cancelled_date { get; set; }
    }
}
