using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DRES.Models
{
    public class Material_Consumption_Item
    {

        public int id { get; set; }

     
        public int consumption_id { get; set; }

        public int material_id { get; set; }

        public int unit_id { get; set; }

        public int quantity { get; set; }

  
        public virtual Material_Consumption Material_Consumption { get; set; }

        public virtual Material Material { get; set; }
        public virtual Unit Unit { get; set; }
    }
}
