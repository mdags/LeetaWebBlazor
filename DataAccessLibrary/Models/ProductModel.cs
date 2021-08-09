using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class ProductModel
    {
        [Key]
        public int id { get; set; }
        public int? cat_id { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Name is too long.")]
        public string name { get; set; }
        public string description { get; set; }
        public string product_unit { get; set; }
        public double unit_price { get; set; }
        public double original_price { get; set; }
        public string img_path { get; set; }
        public string search_tag { get; set; }
        public double discount { get; set; }
        public double overall_rating { get; set; }
        public int? touch_count { get; set; }
        public int? favourite_count { get; set; }
        public int? status { get; set; }
        public string created_date { get; set; }
        public int? created_user_id { get; set; }
        public string updated_date { get; set; }
        public int? updated_user_id { get; set; }
        [Editable(false)]
        public string currency_symbol { get; set; }
        [Editable(false)]
        public int? is_favourite { get; set; }
        [Editable(false)]
        public int? cart_count { get; set; }
        [Editable(false)]
        public string cat_name { get; set; }
    }
}
