using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Console
{
    public static class ConsoleFormatter
    {
        private static void WriteLine(string message, ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            System.Console.ForegroundColor = foreground;
            System.Console.BackgroundColor = background;

            System.Console.WriteLine(message);
        }

        // Output helpers
        public static void WriteError(string message) => WriteLine($"{message}", ConsoleColor.Red);
        public static void WriteWarning(string message) => WriteLine($"{message}", ConsoleColor.Magenta);
        public static void WriteInfo(string message) => WriteLine($"{message}", ConsoleColor.Cyan);
        public static void WriteSuccess(string message) => WriteLine($"{message}", ConsoleColor.Green);
        public static void WriteMenuOption(string message) => WriteLine($"{message}", ConsoleColor.Yellow);
        public static void WriteBanner(string message) => WriteLine($"{message}", ConsoleColor.White, ConsoleColor.DarkBlue);

        public static string GetUserInput(string prompt = "")
        {
            if (!string.IsNullOrWhiteSpace(prompt))
                WriteInfo(prompt);

            System.Console.ForegroundColor = ConsoleColor.White;
            return System.Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public static void ResetColor()
        {
            System.Console.ResetColor();
        }

        #region Error message builders
        public static string UnexpectedError(Exception ex) =>
           $"Unexpected error: {ex.Message}";

        public static string InvalidOption(string option) =>
            $"Invalid option: {option}";

        public static string DatabaseError(System.Data.OleDb.OleDbException ex)
        {
            var sheetSignups = Constants.SheetNameSignups.Replace("$", string.Empty);
            var sheetLogs = Constants.SheetNameLogs.Replace("$", string.Empty);

            return $"""
                Error connecting to the Excel data. 
                Exception: {ex.Message}
                Requirements w.r. to the Excel file database:
                - The file name should be {Constants.ExcelFileName}
                - The file should contain sheets named {sheetSignups} and {sheetLogs}
                - The first row of {sheetSignups} should contain headers and its fields should match the properties of the {nameof(Signup)} class.
                - The first row of {sheetLogs} should contain headers and its fields should match the properties of the {nameof(Log)} class.
                """;
        }

        public static string DualObjectNotFound(string errorMessageExcerpt) =>
            $"""
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
        #endregion
    }
}
