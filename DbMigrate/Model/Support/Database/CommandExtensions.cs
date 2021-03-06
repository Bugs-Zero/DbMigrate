using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DbMigrate.Model.Support.Database
{
	internal static class CommandExtensions
	{
		public static Task<int> ExecuteNonQueryAnync(this DbCommand command)
		{
			var sqlCommand = command as SqlCommand;
			if (sqlCommand != null)
			{
				return Task<int>.Factory.FromAsync(sqlCommand.BeginExecuteNonQuery(),
					sqlCommand.EndExecuteNonQuery);
			}
			return Task<int>.Factory.StartNew(command.ExecuteNonQuery);
		}

		public static Task<DbDataReader> ExecuteReaderAsync(this DbCommand command)
		{
			var sqlCommand = command as SqlCommand;
			if (sqlCommand != null)
			{
				return Task<DbDataReader>.Factory.FromAsync(
					sqlCommand.BeginExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow),
					sqlCommand.EndExecuteReader);
			}
			return Task<DbDataReader>.Factory.StartNew(command.ExecuteReader);
		}
	}
}