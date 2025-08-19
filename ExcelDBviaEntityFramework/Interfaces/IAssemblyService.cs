using System.Reflection;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface IAssemblyService
    {
        AssemblyName GetAssemblyName();
        string GetAssemblyValue(string propertyName, AssemblyName assemblyName);
    }
}
