namespace ExcelDBviaEntityFramework.Models
{
    public class SignupUpsert
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public int? PartySize { get; set; }

        public override string? ToString()
        {
            return $"Name: {Name ?? "[unchanged]"}, Phone: {PhoneNumber ?? "[unchanged]"}, Party Size: {(PartySize.HasValue ? PartySize : "[unchanged]")}";
        }
    }
}