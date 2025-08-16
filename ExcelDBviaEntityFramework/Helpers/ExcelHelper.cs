using ClosedXML.Excel;
using ExcelDBviaEntityFramework;
using ExcelDBviaEntityFramework.Helpers;

public static class ExcelHelper
{
    public static void RemoveDeletedRow(string id)
    {
        var filePath = FileHelper.ResolveExcelPath(Constants.ExcelFileName);

        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheet(Constants.SheetName.Replace("$", string.Empty));

        // Iterate bottom-up (so row indices remain valid after deletes)
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int row = lastRow; row > 1; row--) // skip header
        {
            var cell = worksheet.Cell(row, Constants.ColumnIndexId);

            if (!cell.GetString().Equals(id))
                continue;

            cell = worksheet.Cell(row, Constants.ColumnIndexDeleted);
            var value = cell.GetString();

            if (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("1"))
            {
                worksheet.Row(row).Delete();
            }
        }

        workbook.Save();
    }
}