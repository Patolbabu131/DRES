using Humanizer;
using System.ComponentModel.DataAnnotations;

namespace DRES.Models
{
    public class Transaction
    {
        public int id { get; set; }
        public string? invoice_number { get; set;}
        public string transaction_type { get; set; } // purchase, transfer, issue, return, received, damage, loss, scrap, other     
        public DateTime transaction_date { get; set; } // user select
        public int? site_id { get; set; } // site id where transaction is done


        public int? request_id { get; set; }
        public string? remark { get; set; }
        public int? form_supplier_id { get; set; } // suppler id 
        public int? from_site_id { get; set; } // 0 if transfer form suppler to site 

        //public int? from_userid { get; set; } // site manager id
        public int? to_site_id { get; set; }    
        public int? to_user_id { get; set; }  // not null if transfering to site enginner 


        public string? status { get; set; } // 'pending', 'approved', 'rejected', 'completed', 'cancelled'
        public int? received_by_user_id { get; set; }  
        public DateTime? recived_datetime { get; set; } // site manager recive 


        public int createdby { get; set; }           
        public DateTime createdat { get; set; }
        public int updatedby { get; set; }
        public DateTime updatedat { get; set; }


      
        public virtual Supplier Supplier { get; set; }
        public virtual Site Site { get; set; }
        public virtual User user { get; set; }
        public virtual ICollection<Transaction_Items> TransactionItems { get; set; }

    }
}
