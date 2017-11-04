﻿using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbMigrate.Model.Support.Database
{
	public class DbTranection : ITranection
	{
		private const string SqlServerProvider = "System.Data.SqlClient";
		private const string SqlLiteProvider = "System.Data.SQLite";
		private DbConnection _connection;
		private DbTransaction _transaction;
		private readonly DbProviderFactory _dbTypeToUse;

		public DbTranection(string connectionString)
		{
			ConnectionString = connectionString + "Asynchronous Processing=True;MultipleActiveResultSets=true;";
			_dbTypeToUse = DbProviderFactories.GetFactory(SqlServerProvider);
		}

		public void Dispose()
		{
			_transaction?.Rollback();
			_transaction = null;
			_connection?.Close();
			_connection = null;
		}

		public bool IsOpen => _connection != null;

		public string ConnectionString { get; }

		public Task<T> ExecuteScalar<T>(string sql)
		{
			var command = GetCommand(sql);
			return command.ExecuteReaderAsync()
				.ContinueWith(t =>
				{
					command.Dispose(); // before attempting to check the result, in case there was an exception.
					return ReadScalar<T>(t.Result);
				});
		}

		public Task<int> ExecuteNonQuery(string sql)
		{
			var command = GetCommand(sql);
			return command.ExecuteNonQueryAnync()
				.ContinueWith(numRows =>
				{
					command.Dispose();
					return numRows.Result;
				});
		}

		public void Commit()
		{
			if (!IsOpen) return;
			_transaction.Commit();
			_transaction = _connection.BeginTransaction(IsolationLevel.Serializable);
		}

		private void EnsureIsOpen()
		{
			if (IsOpen) return;
			_connection = _dbTypeToUse.CreateConnection();
			if (_connection == null)
				throw new InvalidOperationException(
					$"Failed to connect to database using provider {SqlServerProvider} and connection string {ConnectionString}");
			_connection.ConnectionString = ConnectionString;
			_connection.Open();
			_transaction = _connection.BeginTransaction(IsolationLevel.Serializable);
		}

		private DbCommand GetCommand(string sql)
		{
			EnsureIsOpen();
			var command = _connection.CreateCommand();
			command.CommandText = sql;
			command.CommandType = CommandType.Text;
			command.Transaction = _transaction;
			return command;
		}

		private static T ReadScalar<T>(IDataReader reader)
		{
			reader.Read();
			var result = (T) reader.GetValue(0);
			reader.Close();
			return result;
		}
	}
}