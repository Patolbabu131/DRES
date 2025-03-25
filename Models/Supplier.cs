using Microsoft.EntityFrameworkCore;

namespace DRES.Models
{
    [Index(nameof(gst), IsUnique = true)]
    public class Supplier
    {
        public int id { get; set; }
        public string company_name { get; set; }  // e.g., "Acme Corp"
        public string contact_name { get; set; }     // e.g., "John Doe"
        public string gst { get; set; }     // e.g., "John Doe"
       
        public string phone1 { get; set; }              // e.g., "contact@acme.com"
        public string phone2 { get; set; }              // e.g., "contact@acme.com"
        public string address { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
