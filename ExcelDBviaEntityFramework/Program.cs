using ExcelDBviaEntityFramework.Services;
using ExcelDBviaEntityFramework.UI;

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
            new ConsoleUI(new SignupService()).Run();
        }
    }
}