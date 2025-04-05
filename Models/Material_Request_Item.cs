using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DRES.Models
{
    public class Material_Request_Item
    {
  
        public int id { get; set; }

        public int request_id { get; set; } // FK to Material_Request.id
        public int material_id { get; set; }
        public int unit_id { get; set; }
        public int quantity { get; set; }
        public int? issued_quantity { get; set; }
        public string status { get; set; }

        public virtual Unit Unit { get; set; }
        public virtual Material Material { get; set; }
        public virtual Material_Request material_request { get; set; }
    }
}
