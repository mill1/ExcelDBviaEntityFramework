using ClosedXML.Excel;

namespace ExcelDBviaEntityFramework.Helpers
{
    public static class ExcelHelper
    {
        public static void RemoveDeletedRow(string id, bool cascadeDelete)
        {
            var filePath = FileHelper.ResolveExcelPath(Constants.ExcelFileName);

            using var workbook = new XLWorkbook(filePath);

            var signupsSheet = GetSignupsSheet(workbook, Constants.SheetNameSignups);
            var logsSheet = GetSignupsSheet(workbook, Constants.SheetNameLogs);

            var lastRow = GetLastRow(signupsSheet);

            for (int row = lastRow; row > 1; row--) // skip header
            {                
                if (signupsSheet.Cell(row, Constants.SignupsColumnIndexId).GetString() != id)
                    continue;                

                if (signupsSheet.Cell(row, Constants.SignupsColumnIndexDeleted).GetString().Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    signupsSheet.Row(row).Delete();

                    if(cascadeDelete)
                        RemoveLogsForSignup(id, logsSheet);
                }
            }

            workbook.Save();
        }

        private static void RemoveLogsForSignup(string signupId, IXLWorksheet logsSheet)
        {
            var lastRow = GetLastRow(logsSheet);

            for (int row = lastRow; row > 1; row--) // skip header
            {
                if (logsSheet.Cell(row, Constants.LogsColumnIndexSignupId).GetString() == signupId)
                {
                    logsSheet.Row(row).Delete();
                }
            }
        }

        private static int GetLastRow(IXLWorksheet sheet)
        {
            return sheet.LastRowUsed()?.RowNumber() ?? 1;
        }

        private static IXLWorksheet GetSignupsSheet(XLWorkbook workbook, string sheetName)
        {
            return workbook.Worksheet(sheetName.Replace("$", string.Empty));
        }
    }

}