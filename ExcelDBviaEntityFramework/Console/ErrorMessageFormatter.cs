using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Console
{
    public static class ErrorMessageFormatter
    {
        public static string UnexpectedError(Exception ex)
        {
            return $"Unexpected error: {ex.Message}";
        }

        public static string InvalidOption(string option)
        {
            return $"Invalid option: {option}";
        }

        public static string DatabaseError(System.Data.OleDb.OleDbException ex)
        {
            var sheetSignups = Constants.SheetNameSignups.Replace("$", string.Empty);
            var sheetLogs = Constants.SheetNameLogs.Replace("$", string.Empty);

            return $"""
                Error connecting to the Excel data. 
                Exception: {ex.Message}
                Requirements w.r. to the Excel file database:
                - The file name should be {Constants.ExcelFileName}
                - The file should contain sheets named {sheetSignups} en {sheetLogs}
                - The first row of {sheetSignups} should contain headers and its fields should match the properties of the {nameof(Signup)} class.
                - The first row of {sheetLogs} should contain headers and its fields should match the properties of the {nameof(Log)} class.
                """;
        }

        public static string DualObjectNotFound(string errorMessageExcerpt)
        {
            return $"""
            The Jet provider has issued a .Dual probe when EF asked it for metadata w.r. to the query.
            Since Excel is not a relational database this causes next error:
            {errorMessageExcerpt}
            There are two workarounds for this:
            1. Force EF to skip relational translation paths by using no-tracking raw SQL instead of LINQ:
                return ctx.Signups
                    .FromSqlRaw($"SELECT * FROM [SheetName]")
                    .AsNoTracking()
                    .ToList();
            2. Switch to client-side evaluation
               If you must keep LINQ, call .AsEnumerable() before filtering so EF doesn’t try to translate.
               For instance use the `AsEnumerable()` method on the DbSet, e.g.:
                return ctx.Signups
                    .AsNoTracking()
                    .AsEnumerable()   // forces client-side LINQ
                    .Where(s => !string.IsNullOrEmpty(s.Id));
            """;
        }
    }
}