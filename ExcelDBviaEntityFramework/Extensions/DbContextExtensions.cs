using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        public static int SaveChangesWithGateway(this ExcelDbContext ctx)
        {
            var gateway = new ExcelDataGateway(ctx.Database.GetDbConnection());
            return ctx.SaveChanges(gateway);
        }
    }
}