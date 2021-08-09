using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class CartModel
    {
        [Key]
        public int id { get; set; }
        public int? user_id { get; set; }
        public int? product_id { get; set; }
        public int? qty { get; set; }
        public string created_date { get; set; }
        public int? created_user_id { get; set; }
        public string updated_date { get; set; }
        public int? updated_user_id { get; set; }
        [Editable(false)]
        public string product_name { get; set; }
        [Editable(false)]
        public string product_img_path { get; set; }
        [Editable(false)]
        public string currency_symbol { get; set; }
        [Editable(false)]
        public double total_amount { get; set; }
    }
}
