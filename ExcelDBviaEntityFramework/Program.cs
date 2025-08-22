using ExcelDBviaEntityFramework.Console;
using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Data.Stubs;
using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// TODO
// -Add unit tests (compare generated tests between ChatGPT and CoPilot)

namespace ExcelDBviaEntityFramework
{
    public class Program
    {
        static void Main(string[] args)
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
            .AddScoped<IAssemblyService, AssemblyService>()
            .AddScoped<ISignupService, SignupService>()
            .AddScoped<IExcelRepositoryFactory, ExcelRepositoryFactory>()
            .AddScoped<ISignupRepository, SignupRepositoryStub>() // TODO lw
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