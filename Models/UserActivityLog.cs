namespace DRES.Models
{
    public class UserActivityLog
    {
        public int id { get; set; }
        public int userid { get; set; }
        public string ipaddress { get; set; }
        public string devicetype { get; set; }
        public string location { get; set; }
        public DateTime createdat { get; set; }
        public string jwttoken { get; set; }



    }
}
