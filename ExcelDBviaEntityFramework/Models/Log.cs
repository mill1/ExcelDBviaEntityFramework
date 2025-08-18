using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExcelDBviaEntityFramework.Models
{
    [Table(Constants.SheetNameLogs)]
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
