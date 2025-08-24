namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface IFileService
    {
        void EnsureFileNotLocked(string filePath);
        string ResolveExcelPath(string fileName);
    }
}