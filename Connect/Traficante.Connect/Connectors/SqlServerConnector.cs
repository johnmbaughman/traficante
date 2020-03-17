﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Connect.Connectors
{
    public class SqlServerConnector : Connector
    {
        public SqlServerConnectorConfig Config => (SqlServerConnectorConfig)base.Config;

        public SqlServerConnector(SqlServerConnectorConfig config)
        {
            base.Config = config;
        }

        public override Delegate GetTable(string name, string[] path)
        {
            Func<IDataReader> @delegate = () =>
            {
                SqlConnection sqlConnection = null;
                try
                {
                    string sqlPath = "";
                    if (path.Length > 1)
                        sqlPath = string.Join(".", path.Skip(1).Select(x => $"[{x}]")) + ".";
                    string sqlName = $"[{name}]";

                    sqlConnection = new SqlConnection(this.Config.ToConnectionString());
                    SqlCommand sqliteCommand = new SqlCommand();
                    sqliteCommand.Connection = sqlConnection;
                    sqliteCommand.CommandText = $"SELECT * FROM {sqlPath}{sqlName}";
                    sqlConnection.Open();
                    return sqliteCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch
                {
                    sqlConnection?.Dispose();
                    throw;
                }
            };
            return @delegate;
        }

        public async Task TryConnectAsync(CancellationToken ct)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                await sqlConnection.OpenAsync(ct);
            }
        }

        public void TryConnect(string databaseName)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(databaseName);
            }
        }

        public IEnumerable<object> Run(string text, Action<Type> returnTypeCreated = null)
        {
            using (var sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                //sqlConnection.ChangeDatabase(databaseName);
                using (var sqlCommand = new SqlCommand(text, sqlConnection))
                {
                    using (var sqlReader = sqlCommand.ExecuteReader())
                    {
                        List<(string name, Type type)> fields = new List<(string name, Type type)>();
                        for (int i = 0; i < sqlReader.FieldCount; i++)
                        {
                            var name = sqlReader.GetName(i);
                            var type = sqlReader.GetFieldType(i);
                            fields.Add((name, type));
                        }
                        Type returnType = new ExpressionHelper().CreateAnonymousType(fields);
                        returnTypeCreated?.Invoke(returnType);
                        while (sqlReader.Read())
                        {
                            var item = Activator.CreateInstance(returnType);
                            for (int i = 0; i < sqlReader.FieldCount; i++)
                            {
                                
                                var name = sqlReader.GetName(i);
                                var field = returnType.GetField(name);
                                var value = sqlReader.GetValue(i);
                                if (value is DBNull)
                                    value = GetDefaultValue(field.FieldType);
                                field.SetValue(item, value);
                            }
                            yield return item;
                        }
                    }
                }
            }
        }

        public object GetDefaultValue(Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

        // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections?view=netframework-4.8

        public List<string> GetDatabases()
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                var databasesNames = sqlConnection.GetSchema("Databases").Select().Select(s => s[0].ToString()).ToList();
                return databasesNames;
            }
        }

        public List<(string schema, string name)> GetTables(string database)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(database);
                var tables = sqlConnection.GetSchema("Tables")
                    .Select()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "BASE TABLE")
                    .Select(t => (t["TABLE_SCHEMA"]?.ToString(), t["TABLE_NAME"]?.ToString()))
                    .ToList();
                return tables;
            }
        }

        public List<(string schema, string name)> GetViews(string database)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(database);
                var views = sqlConnection.GetSchema("Tables")
                    .Select()
                    .Where(x => x["TABLE_TYPE"]?.ToString() == "VIEW")
                    .Select(t => (t["TABLE_SCHEMA"]?.ToString(), t["TABLE_NAME"]?.ToString()))
                    .ToList();
                return views;
            }
        }

        public List<(string name, string type, bool? notNull)> GetFields(string database, string tableOrView)
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = this.Config.ToConnectionString();
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(database);
                var fields = sqlConnection.GetSchema("Columns")
                    .Select()
                    .Where(x => x["TABLE_NAME"].ToString() == tableOrView)
                    .Select(x => (x["COLUMN_NAME"]?.ToString(), x["DATA_TYPE"]?.ToString(), x["IS_NULLABLE"] as Nullable<bool>))
                    .ToList();
                return fields;
            }
        }
    }

    public class SqlServerConnectorConfig  : ConnectorConfig
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public SqlServerAuthentication Authentication { get; set; } = SqlServerAuthentication.Windows;
        public string ToConnectionString()
        {
            if (Authentication == SqlServerAuthentication.Windows)
                return $"Server={Server};Database=master;Trusted_Connection=True;";
            else
                return $"Server={Server};Database=master;User Id={UserId};Password={Password};";
        }
    }

    public enum SqlServerAuthentication : int
    {
        Windows = 0,
        SqlServer = 1
    }
}
