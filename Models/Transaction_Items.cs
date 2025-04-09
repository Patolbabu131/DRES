namespace DRES.Models
{
    public class Transaction_Items
    {

        public int id { get; set; }
        public int transaction_id { get; set; }
        public int material_id { get; set; }
        public int unit_type_id { get; set; } // map with unit type id
        public int quantity { get; set; } // quantity of stockpublic decimal unit_price { get; set; } // price of one unte without gst
        public decimal? unit_price { get; set; }
        public string? tex_type { get; set; } // Intra , Inter , No gst (no,inter,intra)
        public int? gst { get; set; } // percentage of gst
        public decimal? total { get; set; }

        public virtual Transaction Transaction { get; set; }
        public virtual Material Material { get; set; }
        public virtual Unit Unit { get; set; }
    }
}
