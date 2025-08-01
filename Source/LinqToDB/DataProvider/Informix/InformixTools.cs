﻿using System.Data.Common;

using LinqToDB.Data;
using LinqToDB.Internal.DataProvider.Informix;

namespace LinqToDB.DataProvider.Informix
{
	public static class InformixTools
	{
		internal static InformixProviderDetector ProviderDetector = new();

		public static bool AutoDetectProvider
		{
			get => ProviderDetector.AutoDetectProvider;
			set => ProviderDetector.AutoDetectProvider = value;
		}

		public static IDataProvider GetDataProvider(InformixProvider provider = InformixProvider.AutoDetect, string? connectionString = null, DbConnection? connection = null)
		{
			return ProviderDetector.GetDataProvider(new ConnectionOptions(ConnectionString: connectionString, DbConnection: connection), provider, default);
		}

		#region CreateDataConnection

		public static DataConnection CreateDataConnection(string connectionString, InformixProvider provider = InformixProvider.AutoDetect)
		{
			return new DataConnection(new DataOptions()
				.UseConnectionString(ProviderDetector.GetDataProvider(new ConnectionOptions(ConnectionString: connectionString), provider, default), connectionString));
		}

		public static DataConnection CreateDataConnection(DbConnection connection, InformixProvider provider = InformixProvider.AutoDetect)
		{
			return new DataConnection(new DataOptions()
				.UseConnection(ProviderDetector.GetDataProvider(new ConnectionOptions(DbConnection: connection), provider, default), connection));
		}

		public static DataConnection CreateDataConnection(DbTransaction transaction, InformixProvider provider = InformixProvider.AutoDetect)
		{
			return new DataConnection(new DataOptions()
				.UseTransaction(ProviderDetector.GetDataProvider(new ConnectionOptions(DbTransaction: transaction), provider, default), transaction));
		}

		#endregion
	}
}
