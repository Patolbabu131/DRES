using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DRES.Models
{
  
    public class Material_Request
    {
      
        public int id { get; set; }

        public int site_id { get; set; }
        public DateTime request_date { get; set; }
        public int requested_by { get; set; }
        public string? remark { get; set; }
        public int? approved_by { get; set; }
        public DateTime? approval_date { get; set; }
        public bool forwarded_to_ho { get; set; }

        public virtual Site Site { get; set; }
        public virtual ICollection<Material_Request_Item> Material_Request_Item { get; set; } = new List<Material_Request_Item>();
    }

}
