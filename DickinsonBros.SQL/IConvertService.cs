using System.Collections.Generic;
using System.Data;

namespace DickinsonBros.SQL
{
    public interface IConvertService
    {
        DataTable ToDataTable<T>(IEnumerable<T> enumerable, string tableName);
    }
}