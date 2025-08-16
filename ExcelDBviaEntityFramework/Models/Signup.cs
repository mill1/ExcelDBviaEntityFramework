using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelDBviaEntityFramework.Models
{
    [Table(Constants.SheetName)]
    public class Signup
    {
        [Key]
        public required string Id_ý { get; set; }

        public required bool Deleted_ý {  get; set; }

        public string Id { get; set; }

        public string? Name { get; set; }

        [Column("Phone number")]
        public string? PhoneNumber { get; set; }

        [Column("Party size")]
        public int PartySize { get; set; }

        public override string? ToString()
        {
            return $"EF Id: {Id_ý}, Id: {Id}, {Name} {PhoneNumber} ({PartySize})";
        }
    }
}
