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

        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
        public virtual ICollection<Stock> Stocks   { get; set; } = new List<Stock>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

}
