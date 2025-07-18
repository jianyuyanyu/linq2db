﻿using System.Data.Common;
using System.Reflection;

using LinqToDB.Data;
using LinqToDB.Internal.DataProvider;
using LinqToDB.Internal.DataProvider.Sybase;

namespace LinqToDB.DataProvider.Sybase
{
	public static class SybaseTools
	{
		internal static SybaseProviderDetector ProviderDetector = new();

		public static bool AutoDetectProvider
		{
			get => ProviderDetector.AutoDetectProvider;
			set => ProviderDetector.AutoDetectProvider = value;
		}

		public static IDataProvider GetDataProvider(SybaseProvider provider = SybaseProvider.AutoDetect, string? connectionString = null, DbConnection? connection = null)
		{
			return ProviderDetector.GetDataProvider(new ConnectionOptions(ConnectionString: connectionString, DbConnection: connection), provider, default);
		}

		public static void ResolveSybase(string path, string? assemblyName = null)
		{
			_ = new AssemblyResolver(path, assemblyName ?? SybaseProviderAdapter.ManagedAssemblyName);
		}

		public static void ResolveSybase(Assembly assembly)
		{
			_ = new AssemblyResolver(assembly, assembly.FullName!);
		}

		#region CreateDataConnection

		public static DataConnection CreateDataConnection(string connectionString, SybaseProvider provider = SybaseProvider.AutoDetect)
		{
			return new DataConnection(new DataOptions()
				.UseConnectionString(ProviderDetector.GetDataProvider(new ConnectionOptions(ConnectionString: connectionString), provider, default), connectionString));
		}

		public static DataConnection CreateDataConnection(DbConnection connection, SybaseProvider provider = SybaseProvider.AutoDetect)
		{
			return new DataConnection(new DataOptions()
				.UseConnection(ProviderDetector.GetDataProvider(new ConnectionOptions(DbConnection: connection), provider, default), connection));
		}

		public static DataConnection CreateDataConnection(DbTransaction transaction, SybaseProvider provider = SybaseProvider.AutoDetect)
		{
			return new DataConnection(new DataOptions()
				.UseTransaction(ProviderDetector.GetDataProvider(new ConnectionOptions(DbTransaction: transaction), provider, default), transaction));
		}

		#endregion

	}
}
