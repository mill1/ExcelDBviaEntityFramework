using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Services;
using ExcelDBviaEntityFramework.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// TODO
// unit tests
// log sheet???
// on close: delete all xl rows physically
// other todo's
// update call


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
                .AddDbContext<ExcelDbContext>(options =>
                {
                    options.UseJet($"""
                        Provider=Microsoft.ACE.OLEDB.12.0;
                        Data Source={FileHelper.ResolveExcelPath(Constants.ExcelFileName)};
                        Extended Properties='Excel 12.0 Xml;HDR=YES';
                    """);
                });

            return services;
        }
    }
}