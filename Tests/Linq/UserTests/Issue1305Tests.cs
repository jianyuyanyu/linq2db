﻿using System;
using System.Linq;

using LinqToDB;
using LinqToDB.Mapping;

using NUnit.Framework;

namespace Tests.UserTests
{
	/// <summary>
	/// Test fixes to Issue #1305.
	/// Before fix fields in derived tables were added first in the column order by <see cref="DataExtensions.CreateTable{T}(IDataContext, string?, string?, string?, string?, string?, LinqToDB.SqlQuery.DefaultNullable, string?, TableOptions)"/>.
	/// </summary>
	[TestFixture]
	public class Issue1305Tests : TestBase
	{
		public class FluentMapping
		{
			public int       RecordID       { get; set; }
			public DateTime? EffectiveEnd   { get; set; }
			public DateTime  EffectiveStart { get; set; }
			public int       Key            { get; set; }
			public int       Audit2ID       { get; set; }
			public int       Audit1ID       { get; set; }
			public int       Unordered1     { get; set; }
			public int       Unordered2     { get; set; }
		}

		/// <summary>
		/// Base table class with column order specified.
		/// </summary>
		public abstract class VersionedRecord
		{
			[Column(IsPrimaryKey = true, SkipOnInsert = true, Order = 1)]
			public int RecordID             { get; set; }
			[Column(Order = 3)]
			public DateTime? EffectiveEnd   { get; set; }
			[Column(Order = 2)]
			public DateTime EffectiveStart  { get; set; }
			[Column(Order = 4)]
			public int Key                  { get; set; }
			[Column(Order = -1)]
			public int Audit2ID             { get; set; }
			[Column(Order = -10)]
			public int Audit1ID             { get; set; }
		}

		/// <summary>
		/// Derived table class, column order not specified.
		/// </summary>
		[Table("ColumnOrderTest")]
		public class ColumnOrderTest : VersionedRecord
		{
			[Column]
			public string? Name { get; set; }
			[Column]
			public string? Code { get; set; }
		}

		/// <summary>
		/// Confirm that tables creation uses the <see cref="ColumnAttribute.Order"/> field correctly.
		/// </summary>
		/// <param name="context">Configuration string for test context.</param>
		[Test]
		public void TestAttributeMapping([DataSources(false, ProviderName.SQLiteMS)] string context)
		{
			using (var db = GetDataConnection(context))
			using (var __ = db.CreateLocalTable<ColumnOrderTest>())
			{
				// Get table schema
				var sp = db.DataProvider.GetSchemaProvider();
				var s = sp.GetSchema(db);
				var table = s.Tables.FirstOrDefault(_ => _.TableName!.Equals("ColumnOrderTest", StringComparison.OrdinalIgnoreCase))!;
				Assert.That(table, Is.Not.Null);
				using (Assert.EnterMultipleScope())
				{
					// Confirm order of specified fields only
					Assert.That(table.Columns[0].ColumnName.ToLowerInvariant(), Is.EqualTo("recordid"));
					Assert.That(table.Columns[1].ColumnName.ToLowerInvariant(), Is.EqualTo("effectivestart"));
					Assert.That(table.Columns[2].ColumnName.ToLowerInvariant(), Is.EqualTo("effectiveend"));
					Assert.That(table.Columns[3].ColumnName.ToLowerInvariant(), Is.EqualTo("key"));
					Assert.That(table.Columns[6].ColumnName.ToLowerInvariant(), Is.EqualTo("audit1id"));
					Assert.That(table.Columns[7].ColumnName.ToLowerInvariant(), Is.EqualTo("audit2id"));
				}

				// Confirm that unordered fields are in the right range of positions
				string[] unordered = new[] { "name", "code" };
				Assert.That(unordered, Does.Contain(table.Columns[4].ColumnName.ToLowerInvariant()));
				Assert.That(unordered, Does.Contain(table.Columns[5].ColumnName.ToLowerInvariant()));
			}
		}

		/// <summary>
		/// Confirm that tables creation uses the <see cref="ColumnAttribute.Order"/> field correctly.
		/// </summary>
		/// <param name="context">Configuration string for test context.</param>
		[Test]
		public void TestFluentMapping([DataSources(false, ProviderName.SQLiteMS)] string context)
		{
			var ms = new MappingSchema();

			new FluentMappingBuilder(ms)
				.Entity<FluentMapping>()
				.Property(t => t.Audit1ID).HasOrder(-10)
				.Property(t => t.Audit2ID).HasOrder(-1)
				.Property(t => t.RecordID).HasOrder(1)
				.Property(t => t.EffectiveEnd).HasOrder(3)
				.Property(t => t.EffectiveStart).HasOrder(2)
				.Property(t => t.Key).HasOrder(4)
				.Property(t => t.Unordered1)
				.Property(t => t.Unordered2)
				.Build();

			using (var db = GetDataConnection(context, ms))
			{
				using (var tbl = db.CreateLocalTable<FluentMapping>())
				{
					// Get table schema
					var sp = db.DataProvider.GetSchemaProvider();
					var s = sp.GetSchema(db);
					var table = s.Tables.FirstOrDefault(_ => _.TableName!.Equals(nameof(FluentMapping), StringComparison.OrdinalIgnoreCase))!;
					Assert.That(table, Is.Not.Null);
					using (Assert.EnterMultipleScope())
					{
						// Confirm order of specified fields only
						Assert.That(table.Columns[0].ColumnName.ToLowerInvariant(), Is.EqualTo("recordid"));
						Assert.That(table.Columns[1].ColumnName.ToLowerInvariant(), Is.EqualTo("effectivestart"));
						Assert.That(table.Columns[2].ColumnName.ToLowerInvariant(), Is.EqualTo("effectiveend"));
						Assert.That(table.Columns[3].ColumnName.ToLowerInvariant(), Is.EqualTo("key"));
						Assert.That(table.Columns[6].ColumnName.ToLowerInvariant(), Is.EqualTo("audit1id"));
						Assert.That(table.Columns[7].ColumnName.ToLowerInvariant(), Is.EqualTo("audit2id"));
					}

					// Confirm that unordered fields are in the right range of positions
					string[] unordered = new[] { "unordered1", "unordered2" };
					Assert.That(unordered, Does.Contain(table.Columns[4].ColumnName.ToLowerInvariant()));
					Assert.That(unordered, Does.Contain(table.Columns[5].ColumnName.ToLowerInvariant()));
				}
			}
		}
	}
}
