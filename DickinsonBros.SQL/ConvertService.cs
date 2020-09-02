using DickinsonBros.SQL.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DickinsonBros.SQL
{

    public class ConvertService : IConvertService
    {
        internal readonly IMemoryCache _memoryCache;

        public ConvertService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public DataTable ToDataTable<T>(IEnumerable<T> enumerable, string tableName)
        {
            if (enumerable is null)
            {
                throw new NullReferenceException("enumerable is null");
            }

            var dataTable = (DataTable)null;
            var assemblyQualifiedName = typeof(T).AssemblyQualifiedName;

            var dataTableWithPropertyInfo = (DataTableWithPropertyInfo)null;

            if (_memoryCache.TryGetValue(assemblyQualifiedName, out dataTableWithPropertyInfo))
            {
                dataTable = dataTableWithPropertyInfo.DataTable.Copy();
                FillDataTable(dataTable, dataTableWithPropertyInfo.Properties, enumerable);
            }
            //Note: Data tables for successul entry into SQL require a name to all columns.
            //      This will add set "item" for the column. 
            //      Example List<string> has no name for its column.
            else if (IsAnonymousType(typeof(T)))
            {
                var normalizedEnumerable = enumerable.Select(item => new AnonymousWarpper<T>() { Item = item });
                PropertyInfo[] properties = PublicProperties(typeof(AnonymousWarpper<T>));
                var dataTableTemplate = CreateDataTable(properties, tableName);

                _memoryCache.Set
                (
                    assemblyQualifiedName,
                    new DataTableWithPropertyInfo
                    {
                        DataTable = dataTableTemplate,
                        Properties = properties
                    },
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1))
                );

                dataTable = dataTableTemplate.Clone();
                FillDataTable(dataTable, properties, normalizedEnumerable);
            }
            else
            {
                PropertyInfo[] properties = PublicProperties(typeof(T));
                var dataTableTemplate = CreateDataTable(properties, tableName);

                _memoryCache.Set
                (
                    assemblyQualifiedName,
                    new DataTableWithPropertyInfo
                    {
                        DataTable = dataTableTemplate,
                        Properties = properties
                    },
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1))
                );

                dataTable = dataTableTemplate.Clone();
                FillDataTable(dataTable, properties, enumerable);
            }

            return dataTable;
        }

        internal DataTable CreateDataTable(PropertyInfo[] propertyInfos, string tableName)
        {
            DataTable dataTable = new DataTable(tableName);

            foreach (var propertyInfo in propertyInfos)
            {
                dataTable
                .Columns
                .Add
                (
                    propertyInfo.Name,
                    ToUnderlyingDataTableType(propertyInfo.PropertyType)
                );
            }

            return dataTable;
        }

        internal Type ToUnderlyingDataTableType(Type type)
        {
            if (type.IsEnum)
            {
                return Enum.GetUnderlyingType(type);
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                return Nullable.GetUnderlyingType(type);
            }

            return type;
        }

        internal void FillDataTable<T>(DataTable dataTable, PropertyInfo[] propertyInfos, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                DataRow row = dataTable.NewRow();
                foreach (var propertyInfo in propertyInfos)
                {
                    var value = propertyInfo.GetValue(item);
                    row[propertyInfo.Name] = ToUnderlyingDataTableValue(value, dataTable.Columns[propertyInfo.Name].DataType);
                }
                dataTable.Rows.Add(row);
            }
        }

        internal object ToUnderlyingDataTableValue(object value, Type type)
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            }
            else if (type.IsEnum)
            {
                object converted = Convert.ChangeType(value, Enum.GetUnderlyingType(type));
                if (!Enum.IsDefined(type, converted))
                {
                    throw new InvalidCastException($"Unable to convert {converted} to {type.Name} - invalid range");
                }
                else
                {
                    return converted;
                }
            }
            else
            {
                return Convert.ChangeType(value, type);
            }
        }

        internal PropertyInfo[] PublicProperties(Type type)
        {
            var propertyInfos = new List<PropertyInfo>();

            if (type.IsInterface)
            {
                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface))
                            continue;
                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }
                    var typeProperties = subType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                    var newPropertyInfos = typeProperties.Where(typeProperty => !propertyInfos.Contains(typeProperty));
                    propertyInfos.InsertRange(0, newPropertyInfos);
                }
            }
            else
            {
                propertyInfos.AddRange(type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance));
            }

            var primitivePropertyInfos =
            propertyInfos.
            Where
            (
                info =>
                {
                    bool isReadableProperty = info.GetGetMethod() != null;
                    bool isAllowedDataTableType = IsAllowedDataTableType(info.PropertyType);
                    bool isAllowedDataTableNullableType = (IsNullableType(info.PropertyType) && IsAllowedDataTableType(Nullable.GetUnderlyingType(info.PropertyType)));
                    return isReadableProperty && (isAllowedDataTableType || isAllowedDataTableNullableType);
                }
            ).
            ToArray();
            return primitivePropertyInfos;
        }

        internal bool IsAnonymousType(Type type)
        {
            return
                type.IsPrimitive ||
                type.IsValueType ||
                type == typeof(string);
        }

        internal bool IsAllowedDataTableType(Type type)
        {
            return
            type.IsPrimitive ||
            type.IsValueType ||
            type.IsEnum ||
            type == typeof(System.DateTime) ||
            type == typeof(Guid) ||
            type == typeof(string) ||
            type == typeof(TimeSpan) ||
            type == typeof(byte[]);
        }

        internal bool IsNullableType(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
