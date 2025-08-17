using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelDBviaEntityFramework.Models
{
    [Table(Constants.SheetNameLog)]
    public class Log
    {
        [Key]
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        [Column("Signup id")]
        public string SignupId { get; set; }
        public string Entry { get; set; }
    }
}
