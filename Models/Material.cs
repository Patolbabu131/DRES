using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DRES.Models
{
    [Index(nameof(material_name), IsUnique = true)]
    public class Material
    {
        public int id { get; set; }

        [Required]
        public string material_name { get; set; }

        public string remark { get; set; }



        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        
        public virtual ICollection<Transaction_Items> Transaction_Items { get; set; } = new List<Transaction_Items>();
        public virtual ICollection<Material_Request_Item> Material_Request_Item { get; set; } = new List<Material_Request_Item>();
        public virtual ICollection<Material_Consumption_Item> Material_Consumption_Item { get; set; } = new List<Material_Consumption_Item>();
    }
}
