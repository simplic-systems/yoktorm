﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoktorm.Db;

namespace Yoktorm.SQLite
{
    public class SQLiteProvider : IDatabaseProvider
    {
        public IDbConnection Get(string connectionString)
        {
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public Type GetClrType(int dbType)
        {
            return GetClrType((DbType)dbType);
        }

        public Type GetClrType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.String:
                    return typeof(string);
                default:
                    return null;
            }
        }

        public Type GetClrTypeByName(string typeName)
        {
            if (typeName.ToLower().StartsWith("varchar"))
                return typeof(string);

            return null;
        }

        public DbType GetDbType(Type clrType)
        {
            if (clrType == typeof(string))
                return DbType.String;

            return DbType.AnsiString;
        }

        public DatabaseStructure GetStructure(IDynamicDbContext context)
        {
            var structure = new DatabaseStructure();

            var tables = context.Query("SELECT * FROM sqlite_master where type = 'table'");

            foreach (var table in tables)
            {
                var _table = new TableStructure()
                {
                    Name = table.name
                };

                // cid, name, type, notnull, dflt_value, pk
                var columns = context.Query($"PRAGMA table_info({table.name})");
                foreach (var column in columns)
                {
                    var _column = new ColumnStructure()
                    {
                        Name = column.name,
                        DefaultValue = column.dflt_value,
                        IsPrimaryKey = Convert.ToBoolean(column.pk),
                        NotNull = Convert.ToBoolean(column.notnull),
                        ColumnType = GetClrTypeByName(column.type)
                    };

                    _table.Columns.Add(_column);
                }

                structure.Tables.Add(_table);
            }

            return structure;
        }
    }
}