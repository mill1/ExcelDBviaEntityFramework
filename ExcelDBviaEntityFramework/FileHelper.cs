using System.Data;

namespace ExcelDBviaEntityFramework
{
    public static class FileHelper
    {
        public static string ResolveExcelPath(string fileName)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var fullPath = Path.Combine(baseDir, "Excel files", fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Excel file not found: {fullPath}");

            return fullPath;
        }

        public static void EnsureFileNotLocked(string filePath)
        {
            try
            {
                // Try opening with exclusive access
                using var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.None); // No sharing allowed
            }
            catch (IOException ex)
            {
                // TODO: After first EF SaveChanges() some io process locks the file although Excel is not opened. Investigate.
                //throw new DBConcurrencyException($"The Excel file is currently in use by another process. Check if the file is opened in Excel.\r\nPath: {filePath}");
            }
        }

    }
}
