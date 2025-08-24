using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Data.Infrastructure;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            .AddScoped<IFileService, FileService>()
            .AddScoped<IAssemblyService, AssemblyService>()
            .AddScoped<ISignupService, SignupService>()
            .AddScoped<IExcelDataGatewayFactory, ExcelDataGatewayFactory>()
            .AddScoped<ISignupRepository, SignupRepository>()            
            .AddDbContextFactory<ExcelDbContext>(
                options => options.UseJet($"""
                    Provider=Microsoft.ACE.OLEDB.12.0;
                    Data Source={new FileService().ResolveExcelPath(Constants.ExcelFileName)};
                    Extended Properties='Excel 12.0 Xml;HDR=YES';
                """));

            return services;
        }
    }
}