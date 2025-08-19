using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Helpers
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
            var sheetName = Constants.SheetNameSignups.Replace("$", string.Empty);

            return $"""
                Error connecting to the Excel data. 
                Exception: {ex.Message}
                Requirements w.r. to the Excel file database:
                - The file name should be {Constants.ExcelFileName}
                - The file should contain a sheet named {sheetName}
                - The first row of {sheetName} should contain headers
                - Column {Constants.SignupsColumnIndexDeleted} should be named {nameof(Signup.Deleted_ý)}
                - Column {Constants.SignupsColumnIndexId} should be named {nameof(Signup.Id)}
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