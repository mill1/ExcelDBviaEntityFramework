using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ExcelDBviaEntityFramework.Models
{
    [Table(Constants.SheetNameLogs)]
    public class Log
    {
        [Key]
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        [Column("Signup id")]
        public string SignupId { get; set; }
        public string Entry { get; set; }

        public override string? ToString()
        {
            return $"{Timestamp} User: {User} Entry: {Entry})";
        }
    }
}
