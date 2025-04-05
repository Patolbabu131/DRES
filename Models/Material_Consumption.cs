using System.ComponentModel.DataAnnotations;

namespace DRES.Models
{
    public class Material_Consumption
    {
  
        public int id { get; set; }

        public int site_id { get; set; }

        public int user_id { get; set; }
        public DateTime date { get; set; }


        public string? remark { get; set; }
        public int? confirmed_by { get; set; }

        public DateTime createdon {  get; set; } 
        public virtual ICollection<Material_Consumption_Item> Material_Consumption_Item { get; set; } = new List<Material_Consumption_Item>();

        public virtual Site Site { get; set; }

    }
}
