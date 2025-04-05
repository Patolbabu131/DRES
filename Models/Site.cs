using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DRES.Models
{
    
        public enum SiteStatus
        {
            Active,
            Completed,
            OnHold
        }

        [Index(nameof(sitename), IsUnique = true)]
        public class Site
        {
            public int id { get; set; }
            public required string sitename { get; set; }
            public string? description { get; set; }
            public SiteStatus status { get; set; } = SiteStatus.Active;
            public required string siteaddress { get; set; }
            public required string state { get; set; }
            public int createdbyid { get; set; }
            public int updatedbyid { get; set; }
            public DateTime createdat { get; set; } 
            public DateTime updatedat { get; set; }

            public virtual ICollection<User> Users { get; set; } = new List<User>();
            public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

            public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
            public virtual ICollection<Material_Request> Material_Request { get; set; } = new List<Material_Request>();
            public virtual ICollection<Material_Consumption> Material_Consumption { get; set; } = new List<Material_Consumption>();

    }
    
}
