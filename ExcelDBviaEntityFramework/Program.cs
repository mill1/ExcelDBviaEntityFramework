using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Models;

// TODO
// unit tests
// log sheet???
// on close: delete all xl rows physically


namespace ExcelDBviaEntityFramework
{
    // https://www.bricelam.net/2024/03/12/ef-xlsx.html

    public class Program
    {
        static void Main(string[] args)
        {
            var fullPathExcel = FileHelper.ResolveExcelPath("Signups.xlsx");

            if (FileHelper.IsExcelFileInUse(fullPathExcel))
            {
                Console.WriteLine("Excel file is currently in use. Please close it and try again.");
                return;
            }

            new Runner().Run();
        }
    }
}
