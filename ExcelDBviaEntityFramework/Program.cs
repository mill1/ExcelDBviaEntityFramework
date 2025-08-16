using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Services;
using ExcelDBviaEntityFramework.Console;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// TODO
// unit tests
// log sheet???
// on close: delete all xl rows physically
// IRepository interface
// central lock chk

namespace ExcelDBviaEntityFramework
{
    // https://www.bricelam.net/2024/03/12/ef-xlsx.html

    public class Program
    {
        static void Main()
        {
            var services = BuildServiceCollection();
            using ServiceProvider sp = services.BuildServiceProvider();
            var service = sp.GetRequiredService<ConsoleUI>();

            service.Run();
        }

        private static IServiceCollection BuildServiceCollection()
        {
            IServiceCollection services = new ServiceCollection();

            services
            .AddSingleton<ConsoleUI>()
            .AddScoped<ISignupService, SignupService>()
            .AddDbContextFactory<ExcelDbContext>(
                options => options.UseJet($"""
                    Provider=Microsoft.ACE.OLEDB.12.0;
                    Data Source={FileHelper.ResolveExcelPath(Constants.ExcelFileName)};
                    Extended Properties='Excel 12.0 Xml;HDR=YES';
                """));

            return services;
        }
    }
}