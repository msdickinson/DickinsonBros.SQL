using System.Data;
using System.Reflection;

namespace DickinsonBros.SQL.Models
{
    public class DataTableWithPropertyInfo
    {
        public DataTable DataTable { get; set; }
        public PropertyInfo[] Properties { get; set; }
    }
}
