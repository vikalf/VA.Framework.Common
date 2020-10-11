using System.Data;
using System.Data.SqlClient;
using VA.Framework.Common.Database.Definition;

namespace VA.Framework.Common.Database.Implementation
{
    public class DatabaseConnection : IDatabaseConnection
    {
        public IDbConnection GetDbConnection(string connectionString) =>
            new SqlConnection(connectionString);
    }
}
