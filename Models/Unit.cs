using Microsoft.EntityFrameworkCore;

namespace DRES.Models
{
    [Index(nameof(unitname), IsUnique = true)]
    [Index(nameof(unitsymbol), IsUnique = true)]
    public class Unit
    {
        public int Id { get; set; }

        public string unitname { get; set; }      // e.g., "Kilogram" or "Gram"

        public string unitsymbol { get; set; }

        public virtual ICollection<Stock> Stocks   { get; set; } = new List<Stock>();
        public virtual ICollection<Transaction_Items> Transaction_Items { get; set; } = new List<Transaction_Items>();
        public virtual ICollection<Material_Request_Item> Material_Request_Item { get; set; } = new List<Material_Request_Item>();
        public virtual ICollection<Material_Consumption_Item> Material_Consumption_Item { get; set; } = new List<Material_Consumption_Item>();
    }

}
