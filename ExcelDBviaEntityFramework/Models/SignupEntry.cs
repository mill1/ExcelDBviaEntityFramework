using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelDBviaEntityFramework.Models
{
    [Keyless, Table("Sheet1$")]
    public class SignupEntry
    {
        public string Name { get; set; }

        [Column("Phone number")]
        public string PhoneNumber { get; set; }

        [Column("Party size")]
        public double PartySize { get; set; }
    }
}
