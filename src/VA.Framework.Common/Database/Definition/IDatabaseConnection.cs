using System.Data;

namespace VA.Framework.Common.Database.Definition
{
    public interface IDatabaseConnection
    {
        IDbConnection GetDbConnection(string connectionString);
    }
}
