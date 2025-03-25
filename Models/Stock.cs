using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DRES.Models
{

    public class Stock
    {
        public int Id { get; set; }

        public int material_id { get; set; }

        public int site_id { get; set; }  // Null = Admin warehouse

        public int unit_type_id { get; set; }  // Null = Admin warehouse

        public int user_id { get; set; }  // Null = Site-level stock

        public int quantity { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual Material Material { get; set; }
        public virtual Site Site { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual User User { get; set; }





    }
}