using Humanizer;
using System.ComponentModel.DataAnnotations;

namespace DRES.Models
{
    public class Transaction
    {
        public int id { get; set; }

        public int material_id { get; set; } // map with parent id Material table Material.id

        public int unit_type_id { get; set; } 
        public int quantity { get; set; }
        public string? remark { get; set; }
        public int? form_supplier_id { get; set; } 
        public int? from_site_id { get; set; }   
        public int? from_userid { get; set; } 
        public int to_site_id { get; set; }    
        public int to_user_id { get; set; }   
        public DateTime transaction_date { get; set; } = DateTime.UtcNow;
        public int initiated_by_userid { get; set; } 
        public int received_by_user_id { get; set; }  
        public int createdby { get; set; }           
        public int updatedby { get; set; }


        public virtual Material Material { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual Site Site { get; set; }
        public virtual User user { get; set; }

    }
}
