namespace ExcelDBviaEntityFramework
{
    public static class Constants
    {
        // 'Conventions'
        public const string ExcelFileName = "Signups_v2.xlsx";
        public const string SheetNameSignups = "Signups$";
        public const string SheetNameLogs = "Log$";
        public const string ColumnNameDeleted = "Deleted";
        public const int SignupsColumnIndexDeleted = 1;
        public const int SignupsColumnIndexId = 2;
        public const int LogsColumnIndexDeleted = 1;
        public const int LogsColumnIndexSignupId = 5;        
    }
}
