using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class AddressModel
    {
        [Key]
        public int id { get; set; }
        public int? user_id { get; set; }
        public string adr_name { get; set; }
        public string full_address { get; set; }
        public string place_id { get; set; }
        public double adr_lat { get; set; }
        public double adr_lng { get; set; }
        public string created_date { get; set; }
        public int? created_user_id { get; set; }
        public string updated_date { get; set; }
        public int? updated_user_id { get; set; }
    }
}
