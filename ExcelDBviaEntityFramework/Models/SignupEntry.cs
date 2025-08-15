using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelDBviaEntityFramework.Models
{
    [Table("Sheet1$")]
    public class SignupEntry
    {
        [Key]
        public required string Id_ý { get; set; }

        public required bool Deleted_ý {  get; set; }

        public string? Name { get; set; }

        [Column("Phone number")]
        public string? PhoneNumber { get; set; }

        [Column("Party size")]
        public int PartySize { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if(ReferenceEquals(this, obj)) return true;

            var other = (SignupEntry)obj;

            return other.Id_ý == this.Id_ý &&
                other.Name == this.Name &&
                other.Deleted_ý == this.Deleted_ý &&
                other.PhoneNumber == this.PhoneNumber &&
                other.PartySize == this.PartySize;
        }

        public override string? ToString()
        {
            return $"{Id_ý}: {Name} {PhoneNumber} ({PartySize})";
        }
    }
}
