using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class CategoryModel
    {
        [Key]
        public int id { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Name is too long.")]
        public string name { get; set; }
        public string img_path { get; set; }
        public int? status { get; set; }
        public int? touch_count { get; set; }
        public string created_date { get; set; }
        public int? created_user_id { get; set; }
        public string updated_date { get; set; }
        public int? updated_user_id { get; set; }
    }
}
