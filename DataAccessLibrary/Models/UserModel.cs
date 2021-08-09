using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class UserModel
    {
        [Key]
        public int id { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Name is too long.")]
        public string user_name { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Name is too long.")]
        public string user_password { get; set; }
        [EmailAddress]
        [StringLength(255, ErrorMessage = "Name is too long.")]
        public string user_email { get; set; }
        [Phone]
        [StringLength(255, ErrorMessage = "Name is too long.")]
        public string user_phone { get; set; }
        public int? status { get; set; }
        [Required]
        public string role { get; set; }
        public string created_date { get; set; }
        public int? created_user_id { get; set; }
        public string updated_date { get; set; }
        public int? updated_user_id { get; set; }
    }
}
