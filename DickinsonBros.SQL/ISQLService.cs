using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DickinsonBros.SQL
{
    public interface ISQLService
    {
        Task ExecuteAsync(string connectionString, string sql, object param = null, CommandType? commandType = null);
        Task<T> QueryFirstAsync<T>(string connectionString, string sql, object param = null, CommandType? commandType = null);
        Task<T> QueryFirstOrDefaultAsync<T>(string connectionString, string sql, object param = null, CommandType? commandType = null);
    }
}
