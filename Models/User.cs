using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DRES.Models
{
    public enum userrole
    {
        admin,
        sitemanager,
        siteengineer
    }
    [Index(nameof(username), IsUnique = true)]
    public class User
    {

        public int id { get; set; }

        public required string username { get; set; }

        public required string passwordhash { get; set; }
        public required string phone { get; set; }

        public userrole role { get; set; }

        public int? siteid { get; set; }

        public bool IsActive { get; set; } = true;

        public int createdbyid { get; set; }

        public int updatedbyid { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual Site? Site { get; set; }
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

}
