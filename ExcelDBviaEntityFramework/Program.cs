using ExcelDBviaEntityFramework.Console;
using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// TODO
// - unit tests
// - Excel: Add logs for signup data toevoegen

namespace ExcelDBviaEntityFramework
{
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
            .AddSingleton<IUIActions, UIActions>()
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