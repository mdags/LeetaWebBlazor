using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class OrderModel
    {
        [Key]
        public int id { get; set; }
        public int? user_id { get; set; }
        public double sub_total_amount { get; set; }
        public double discount_amount { get; set; }
        public double tax_amount { get; set; }
        public double tax_percent { get; set; }
        public double shipping_amount { get; set; }
        public double total_amount { get; set; }
        public int? total_item_count { get; set; }
        public string contact_name { get; set; }
        public string contact_phone { get; set; }
        public string contact_email { get; set; }
        public int? user_address_id { get; set; }
        public string contact_address { get; set; }
        public string delivery_type { get; set; }
        public string payment_method { get; set; }
        public int? payment_status { get; set; }
        public string order_note { get; set; }
        public DateTime created_date { get; set; }
        public int? created_user_id { get; set; }
        public DateTime updated_date { get; set; }
        public int? updated_user_id { get; set; }
        public List<OrderDetailModel> order_details { get; set; }
        [Editable(false)]
        public string currency_symbol { get; set; }
    }
}
