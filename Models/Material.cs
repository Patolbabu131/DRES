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

        public int unit_id { get; set; }

        public string remark { get; set; }

        public virtual Unit Unit { get; set; }  // Navigation property


        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
