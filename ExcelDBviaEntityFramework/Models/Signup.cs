using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelDBviaEntityFramework.Models
{
    [Table(Constants.SheetNameSignups)]
    public class Signup
    {
        public required bool Deleted_ý {  get; set; }

        [Key]
        public string Id { get; set; }

        public string? Name { get; set; }

        [Column("Phone number")]
        public string? PhoneNumber { get; set; }

        [Column("Party size")]
        public int PartySize { get; set; }

        public override string? ToString()
        {
            return $"Id {Id}: {Name} {PhoneNumber} ({PartySize})";
        }
    }
}
