using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class MemberModel
    {
        [Key]
        public int id { get; set; }
        public string user_name { get; set; }
        public string user_email { get; set; }
        public string user_phone { get; set; }
        public string user_password { get; set; }
        public string user_about_me { get; set; }
        public string img_path { get; set; }
        public int? status { get; set; }
        public int? verified { get; set; }
        public string facebook_id { get; set; }
        public string google_id { get; set; }
        public string apple_id { get; set; }
        public string device_type { get; set; }
        public string device_code { get; set; }
        public string device_token { get; set; }
        public string created_date { get; set; }
        public int? created_user_id { get; set; }
        public string updated_date { get; set; }
        public int? updated_user_id { get; set; }
    }
}
