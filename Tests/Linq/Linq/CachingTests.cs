﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.SqlQuery;
using LinqToDB.Mapping;

using NUnit.Framework;

using Tests.DataProvider;

using Tests.Model;

namespace Tests.Linq
{
	public class CachingTests: TestBase
	{
		sealed class AggregateFuncBuilder : Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				builder.AddFragment("funcName",  builder.GetValue<string>("funcName"));
				builder.AddFragment("fieldName", builder.GetValue<string>("fieldName"));
			}
		}

		[Sql.Extension("{funcName}({fieldName})", BuilderType = typeof(AggregateFuncBuilder), ServerSideOnly = true)]
		static double AggregateFunc([SqlQueryDependent] string funcName, [SqlQueryDependent] string fieldName)
		{
			throw new NotImplementedException();
		}

		[Test]
		public void TestSqlQueryDependent(
			[IncludeDataSources(ProviderName.SQLiteClassic)] string context,
			[Values(
				"MIN",
				"MAX",
				"AVG",
				"COUNT"
			)] string funcName,
			[Values(
				nameof(ALLTYPE.ID),
				nameof(ALLTYPE.BIGINTDATATYPE),
				nameof(ALLTYPE.SMALLINTDATATYPE),
				nameof(ALLTYPE.DECIMALDATATYPE),
				nameof(ALLTYPE.INTDATATYPE),
				nameof(ALLTYPE.REALDATATYPE)
			)] string fieldName)
		{
			using (var db = GetDataContext(context))
			{
				var query =
					from t in db.GetTable<ALLTYPE>()
					from c in db.GetTable<Child>()
					select new
					{
						Aggregate = AggregateFunc(funcName, fieldName)
					};

				query.ToArray();
				var sql = query.ToSqlQuery().Sql;

				Assert.That(sql, Contains.Substring(funcName).And.Contains(fieldName));
			}
		}

		[Test]
		public void TestSqlQueryDependent_DecFloat(
			[IncludeDataSources(TestProvName.AllDB2)] string context,
			[Values(
				"MIN",
				"MAX",
				"AVG",
				"COUNT"
			)] string funcName,
			[Values(
				nameof(ALLTYPE.DECFLOATDATATYPE)
			)] string fieldName)
		{
			using (var db = GetDataContext(context))
			{
				var query =
					from t in db.GetTable<ALLTYPE>()
					from c in db.GetTable<Child>()
					select new
					{
						Aggregate = AggregateFunc(funcName, fieldName)
					};

				query.ToArray();
				var sql = query.ToSqlQuery().Sql;

				Assert.That(sql, Contains.Substring(funcName).And.Contains(fieldName));
			}
		}

		[Test]
		public void TestSqlQueryDependent_Time(
			[IncludeDataSources(TestProvName.AllSqlServer2008Plus)] string context,
			[Values(
				"MIN",
				"MAX",
				// SQL Server doesn't support AVG(time)
				//"AVG",
				"COUNT"
			)] string funcName,
			[Values(
				nameof(ALLTYPE.TIMEDATATYPE)
			)] string fieldName)
		{
			using (var db = GetDataContext(context))
			{
				var query =
					from t in db.GetTable<ALLTYPE>()
					from c in db.GetTable<Child>()
					select new
					{
						Aggregate = AggregateFunc(funcName, fieldName)
					};

				query.ToArray();
				var sql = query.ToSqlQuery().Sql;

				Assert.That(sql, Contains.Substring(funcName).And.Contains(fieldName));
			}
		}

		static IQueryable<T> GetTestTable<T>(IDataContext context,
			string tableName,
			string databaseName,
			string schemaName)
		where T : class
		{
			return context.GetTable<T>().DatabaseName(databaseName).SchemaName(schemaName)
				.TableName(tableName);
		}

		static int CountOccurrences(string source, string subString)
		{
			var count = 0;
			var n     = 0;

			if (subString.Length != 0)
			{
				while ((n = source.IndexOf(subString, n, StringComparison.Ordinal)) != -1)
				{
					n += subString.Length;
					++count;
				}
			}

			return count;
		}

		[Test]
		public void TestByCall(
			[IncludeDataSources(TestProvName.AllSqlServer)] string context,
			[Values("tableName1", "tableName2")] string tableName,
			[Values("database1",  "database2")]  string databaseName,
			[Values("schema1",    "schema2")]    string schemaName
		)
		{
			using (var db = GetDataContext(context))
			{
				var query =
					from c in db.Child
					from cc in (
						from c1 in GetTestTable<Child>(db, tableName, databaseName, schemaName)
						from c2 in GetTestTable<Child>(db, tableName, databaseName, schemaName)
						select new {c1, c2}
					)
					select cc;

				var sql = query.ToSqlQuery().Sql;
				BaselinesManager.LogQuery(sql);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(CountOccurrences(sql, tableName), Is.EqualTo(2));
					Assert.That(CountOccurrences(sql, databaseName), Is.EqualTo(2));
					Assert.That(CountOccurrences(sql, schemaName), Is.EqualTo(2));
				}
			}
		}

		[Test]
		public void TestInlined(
			[IncludeDataSources(TestProvName.AllSqlServer)] string context,
			[Values("tableName1", "tableName2")] string tableName,
			[Values("database1",  "database2")]  string databaseName,
			[Values("schema1",    "schema2")]    string schemaName
		)
		{
			using (var db = GetDataContext(context))
			{
				var query =
					from c in db.Child
					from cc in
					(
						from c1 in db.Child.DatabaseName(databaseName).SchemaName(schemaName).TableName(tableName)
						from c2 in db.Child.DatabaseName(databaseName).SchemaName(schemaName).TableName(tableName)
						select new {c1, c2}
					)
					select cc;

				var sql = query.ToSqlQuery().Sql;
				BaselinesManager.LogQuery(sql);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(CountOccurrences(sql, tableName), Is.EqualTo(2));
					Assert.That(CountOccurrences(sql, databaseName), Is.EqualTo(2));
					Assert.That(CountOccurrences(sql, schemaName), Is.EqualTo(2));
				}
			}
		}

		[Test]
		public void TakeHint(
			[IncludeDataSources(TestProvName.AllSqlServer, TestProvName.AllClickHouse)] string context,
			[Values(TakeHints.Percent, TakeHints.WithTies, TakeHints.Percent | TakeHints.WithTies)] TakeHints takeHint)
		{
			if (takeHint.HasFlag(TakeHints.Percent) && context.IsAnyOf(TestProvName.AllClickHouse))
				Assert.Inconclusive($"ClickHouse doesn't support '{takeHint}' hint");

			using (var db = GetDataContext(context))
			{
				var query =
					from c1 in db.Child
					from c2 in db.Child.OrderBy(r => r.ParentID).Take(10, takeHint)
					select new {c1, c2};

				query.ToArray();
				var sql = query.ToSqlQuery().Sql;

				if (takeHint.HasFlag(TakeHints.Percent))
					Assert.That(sql, Contains.Substring("PERCENT"));

				if (takeHint.HasFlag(TakeHints.WithTies))
					Assert.That(sql, Contains.Substring("WITH TIES"));
			}
		}

		[ActiveIssue]
		[Test(Description = "https://github.com/linq2db/linq2db/issues/4266")]
		public void TestExtensionCollectionParameterSameQuery([IncludeDataSources(TestProvName.AllSqlServer2008Plus)] string context)
		{
			using var db = GetDataConnection(context);

			db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			db.Execute("CREATE TYPE IntTableType AS TABLE(Id INT)");

			try
			{
				var persons = new List<int>() { 1, 2 };
				var query = from p in db.GetTable<Person>()
							where InExt(p.ID, persons)
							orderby p.ID
							select p.ID;

				var currentMiss = query.GetCacheMissCount();

				var result =  query.ToList();
				AreEqual(persons, result);
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 1));

				persons.AddRange(new int[] { 3, 4 });

				result = query.ToList();

				AreEqual(persons, result);
				// cache miss
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 2));
			}
			finally
			{
				db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			}
		}

		[ActiveIssue]
		[Test(Description = "https://github.com/linq2db/linq2db/issues/4266")]
		public void TestExtensionCollectionParameterEqualQuery([IncludeDataSources(TestProvName.AllSqlServer2008Plus)] string context)
		{
			using var db = GetDataConnection(context);

			db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			db.Execute("CREATE TYPE IntTableType AS TABLE(Id INT)");

			try
			{
				var persons = new List<int>() { 1, 2 };
				var query = from p in db.GetTable<Person>()
							where InExt(p.ID, persons)
							orderby p.ID
							select p.ID;

				var currentMiss = query.GetCacheMissCount();

				var result =  query.ToList();

				AreEqual(persons, result);
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 1));

				persons.AddRange(new int[] { 3, 4 });

				query = from p in db.GetTable<Person>()
						where InExt(p.ID, persons)
						orderby p.ID
						select p.ID;

				result = query.ToList();

				AreEqual(persons, result);
				// cache miss
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 2));
			}
			finally
			{
				db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			}
		}

		[Sql.Extension("{field} IN (select * from {values})", IsPredicate = true, BuilderType = typeof(InExtExpressionItemBuilder), ServerSideOnly = true)]
		private static bool InExt<T>([ExprParameter] T field, [SqlQueryDependent] IEnumerable<T> values) where T : struct, IEquatable<int>
		{
			throw new NotImplementedException();
		}

		public sealed class InExtExpressionItemBuilder : Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				var parameterName = (builder.Arguments[1] as MemberExpression)?.Member.Name ?? "p";

				var values = builder.GetValue<System.Collections.IEnumerable>("values")?.OfType<int>().ToArray();

				if (values == null)
				{
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
					throw new ArgumentNullException("values", "Values for \"In/Any\" operation should not be empty");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
				}

				using var dataTable = new DataTable("IntTableType");
				dataTable.Columns.Add("Id", typeof(int));

				foreach (var x in values.Distinct())
				{
					var newRow = dataTable.Rows.Add();
					newRow[0] = x;
				}

				dataTable.AcceptChanges();

				var param = new SqlParameter(new DbDataType(dataTable.GetType() ?? typeof(object), "IntTableType"), parameterName, dataTable);

				builder.AddParameter("values", param);
			}
		}

		[Sql.Extension("{field} IN (select * from {values})", IsPredicate = true, ServerSideOnly = true)]
		private static bool InExtClass<T>([ExprParameter] T field, [ExprParameter] IntArrayClass values) where T : struct, IEquatable<int> => throw new NotImplementedException();

		[Sql.Extension("{field} IN (select * from {values})", IsPredicate = true, ServerSideOnly = true)]
		private static bool InExtStruct<T>([ExprParameter] T field, [ExprParameter] IntArrayStruct values) where T : struct, IEquatable<int> => throw new NotImplementedException();

		[Test(Description = "https://github.com/linq2db/linq2db/issues/4266")]
		public void Issue4266Test_Class([IncludeDataSources(TestProvName.AllSqlServer2008Plus)] string context)
		{
			var ms = new MappingSchema();
			ms.SetConverter<IntArrayClass, DataParameter>(v => v.CreateParameter());
			ms.AddScalarType(typeof(IntArrayClass), new SqlDataType(IntArrayClass.Type));

			using var db = GetDataConnection(context, ms);

			db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			db.Execute("CREATE TYPE IntTableType AS TABLE(Id INT)");

			try
			{
				var persons = new List<int>() { 1, 2 };
				var query = from p in db.GetTable<Person>()
							where InExtClass(p.ID, persons) && InExtClass(p.ID, new IntArrayClass(persons))
							orderby p.ID
							select p.ID;

				var currentMiss = query.GetCacheMissCount();

				var result =  query.ToList();
				AreEqual(persons, result);
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 1));

				persons.AddRange(new int[] { 3, 4 });

				result = query.ToList();

				AreEqual(persons, result);
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 1));
			}
			finally
			{
				db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			}
		}

		[Test(Description = "https://github.com/linq2db/linq2db/issues/4266")]
		public void Issue4266Test_Struct([IncludeDataSources(TestProvName.AllSqlServer2008Plus)] string context)
		{
			var ms = new MappingSchema();
			ms.SetConverter<IntArrayStruct, DataParameter>(v => v.CreateParameter());
			ms.SetDataType(typeof(IntArrayStruct), new SqlDataType(IntArrayStruct.Type));

			using var db = GetDataConnection(context, ms);

			db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			db.Execute("CREATE TYPE IntTableType AS TABLE(Id INT)");

			try
			{
				var persons = new List<int>() { 1, 2 };
				var query = from p in db.GetTable<Person>()
							where InExtStruct(p.ID, persons) && InExtStruct(p.ID, new IntArrayStruct(persons))
							orderby p.ID
							select p.ID;

				var currentMiss = query.GetCacheMissCount();

				var result =  query.ToList();
				AreEqual(persons, result);
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 1));

				persons.AddRange(new int[] { 3, 4 });

				result = query.ToList();

				AreEqual(persons, result);
				Assert.That(query.GetCacheMissCount(), Is.EqualTo(currentMiss + 1));
			}
			finally
			{
				db.Execute("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntTableType') DROP TYPE IntTableType");
			}
		}

		sealed class IntArrayClass(IEnumerable<int> values)
		{
			public static readonly DbDataType Type = new DbDataType(typeof(DataTable), "IntTableType");

			public DataParameter CreateParameter()
			{
				using var dataTable = new DataTable("IntTableType");
				dataTable.Columns.Add("Id", typeof(int));

				foreach (var x in values.Distinct())
				{
					var newRow = dataTable.Rows.Add();
					newRow[0] = x;
				}

				dataTable.AcceptChanges();

				return new DataParameter(null, dataTable);
			}

			public static implicit operator IntArrayClass(List<int> values) => new(values);
		}

		struct IntArrayStruct(IEnumerable<int> values)
		{
			public static readonly DbDataType Type = new DbDataType(typeof(DataTable), "IntTableType");

			public DataParameter CreateParameter()
			{
				using var dataTable = new DataTable("IntTableType");
				dataTable.Columns.Add("Id", typeof(int));

				foreach (var x in values.Distinct())
				{
					var newRow = dataTable.Rows.Add();
					newRow[0] = x;
				}

				dataTable.AcceptChanges();

				return new DataParameter(null, dataTable);
			}

			public static implicit operator IntArrayStruct(List<int> values) => new(values);
		}
	}
}
