﻿using System;
using System.Linq;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.Mapping;

using NUnit.Framework;

using Shouldly;

namespace Tests.xUpdate
{
	public partial class MergeTests
	{
		#region Insert<TEntity>() + different source/match combinations
		[Test]
		public void SameSourceInsertFromTable([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db))
					.OnTargetKey()
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[2], result[4], null, null);
				AssertRow(InitialSourceData[3], result[5], null, 216);
			}
		}

		[Test]
		public void SameSourceInsertFromQuery([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db).Where(_ => _.Id == 5))
					.OnTargetKey()
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[2], result[4], null, null);
			}
		}

		[Test]
		public void SameSourceInsertFromQueryWithSelect([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db).Select(_ => new TestMapping1()
					{
						Id     = _.Id,
						Field1 = _.Field1,
						Field2 = _.Field2,
						Field3 = _.Id + _.Id,
						Field4 = _.Id + _.Id,
						Field5 = _.Field5
					}))
					.OnTargetKey()
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[2], result[4], null, 10);
				AssertRow(InitialSourceData[3], result[5], null, 12);
			}
		}

		// DB2, SAPHANA: match condition matches multiple target records
		[Test]
		public void SameSourceInsertFromTableWithMatch([MergeDataContextSource(
			ProviderName.DB2, TestProvName.AllSapHana)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db))
					.On((t, s) => t.Id == s.Id || s.Field1 != null)
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		[Test]
		public void SameSourceInsertFromTableWithMatchAlternative([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db))
					.On((t, s) => t.Id == s.Id - 1)
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		// DB2, SAPHANA: match condition matches multiple target records
		[Test]
		public void SameSourceInsertFromQueryWithSelectAndMatch([MergeDataContextSource(
			ProviderName.DB2, TestProvName.AllSapHana)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db).Select(_ => new TestMapping1()
					{
						Id     = _.Id,
						Field1 = _.Field3,
						Field2 = _.Field4,
						Field3 = _.Id + _.Id,
						Field4 = _.Id + _.Id + _.Id,
						Field5 = _.Field5
					}))
					.On((t, s) => t.Id == s.Id || s.Field2 != null)
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id));
					Assert.That(result[4].Field1, Is.Null);
					Assert.That(result[4].Field2, Is.Null);
					Assert.That(result[4].Field3, Is.Null);
					Assert.That(result[4].Field4, Is.EqualTo(15));
					Assert.That(result[4].Field5, Is.Null);
				}
			}
		}

		[Test]
		public void SameSourceInsertFromQueryWithSelectAndMatchAlternative([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db).Select(_ => new TestMapping1()
					{
						Id     = _.Id,
						Field1 = _.Field3,
						Field2 = _.Field4,
						Field3 = _.Id + _.Id,
						Field4 = _.Id + _.Id + _.Id,
						Field5 = _.Field5
					}))
					.On((t, s) => t.Id == s.Id - 1)
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[3].Id));
					Assert.That(result[4].Field1, Is.Null);
					Assert.That(result[4].Field2, Is.EqualTo(216));
					Assert.That(result[4].Field3, Is.Null);
					Assert.That(result[4].Field4, Is.EqualTo(18));
					Assert.That(result[4].Field5, Is.Null);
				}
			}
		}

		[Test]
		public void SameSourceInsertFromCollection([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(InitialSourceData)
					.OnTargetKey()
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[2], result[4], null, null);
				AssertRow(InitialSourceData[3], result[5], null, 216);
			}
		}

		[Test]
		public void SameSourceInsertFromEmptyCollection([MergeDataContextSource(TestProvName.AllOracle)] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(Array.Empty<TestMapping1>())
					.OnTargetKey()
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();
				using (Assert.EnterMultipleScope())
				{
					Assert.That(rows, Is.Zero);

					Assert.That(result, Has.Count.EqualTo(4));
				}

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
			}
		}

		// DB2, SAPHANA: match condition matches multiple target records
		[Test]
		public void SameSourceInsertFromCollectionWithMatch([MergeDataContextSource(
			ProviderName.DB2, TestProvName.AllSapHana)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(InitialSourceData)
					.On((t, s) => t.Id == s.Id || s.Field1 != null)
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		[Test]
		public void SameSourceInsertFromCollectionWithMatchAlternative([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(InitialSourceData)
					.On((t, s) => t.Id == s.Id - 1)
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		[Test]
		public void SameSourceInsertFromEmptyCollectionWithMatch([MergeDataContextSource(TestProvName.AllOracle)] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(Array.Empty<TestMapping1>())
					.On((t, s) => t.Id == s.Id && s.Field3 != null)
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();
				using (Assert.EnterMultipleScope())
				{
					Assert.That(rows, Is.Zero);

					Assert.That(result, Has.Count.EqualTo(4));
				}

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
			}
		}

		[Test]
		public void InsertFromCrossJoinedSourceQuery2Workaround([MergeDataContextSource(false, ProviderName.DB2)] string context)
		{
			using (var db = (DataConnection)GetDataConnection(context))
			{
				PrepareData(db);

				var source = from t1 in db.GetTable<TestMapping1>().TableName("TestMerge1")
							 from t2 in db.GetTable<TestMapping1>().TableName("TestMerge2")
							 select new TestMapping1()
							 {
								 Id     = t1.Id,
								 // this is workaround
								 //Fake = t2.Fake,
								 Field1 = t1.Field1,
								 Field2 = t2.Field2,
								 Field3 = t1.Field3,
								 Field4 = t2.Field4,
								 Field5 = t1.Field5
							 };

				var results = source.ToList();

				var selectQuery = source.GetSelectQuery();
				selectQuery.Select.Columns.Count.ShouldBe(6);

				if (db.DataProvider.SqlProviderFlags.IsCrossJoinSupported)
				{
					selectQuery.Select.From.Tables.Count.ShouldBe(1);
					selectQuery.Select.From.Tables[0].Joins.Count.ShouldBe(1);
					selectQuery.Select.From.Tables[0].Joins[0].JoinType.ShouldBe(JoinType.Cross);
				}
				else
				{
					selectQuery.Select.From.Tables.Count.ShouldBe(2);
					selectQuery.Select.From.Tables[0].Joins.Count.ShouldBe(0);
					selectQuery.Select.From.Tables[1].Joins.Count.ShouldBe(0);
				}

				results.Count.ShouldBe(16);
			}
		}

		// Probably wrong test
		[ActiveIssue]
		[Test]
		public void InsertFromCrossJoinedSourceQuery2([MergeDataContextSource(false)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				PrepareData(db);

				var source = from t1 in db.GetTable<TestMapping1>().TableName("TestMerge1")
							 from t2 in db.GetTable<TestMapping1>().TableName("TestMerge2")
							 select new TestMapping1()
							 {
								 Id     = t1.Id,
								 Fake   = t2.Fake,
								 Field1 = t1.Field1,
								 Field2 = t2.Field2,
								 Field3 = t1.Field3,
								 Field4 = t2.Field4,
								 Field5 = t1.Field5
							 };

				var results = source.ToList();

				var selectQuery = source.GetSelectQuery();
				selectQuery.Select.Columns.Count.ShouldBe(6);

				results.Count.ShouldBe(16);
			}
		}

		[Table("Parent")]
		public class CrossJoinLeft
		{
			[Column("ParentID")]
			public int Id { get; set; }
		}

		[Table("Child")]
		public class CrossJoinRight
		{
			[Column("ChildID")]
			public int Id { get; set; }
		}

		[Table("GrandChild")]
		public class CrossJoinResult
		{
			[Column("GrandChildID")]
			public int Id { get; set; }

			[Column("ParentID")]
			public int LeftId { get; set; }

			[Column("ChildID")]
			public int RightId { get; set; }
		}

		[Test]
		public void InsertFromCrossJoinedSourceQuery([MergeDataContextSource(false)] string context)
		{
			using (var db = GetDataConnection(context))
			using (db.BeginTransaction())
			{
				// prepare test data
				db.GetTable<CrossJoinLeft>().Delete();
				db.GetTable<CrossJoinRight>().Delete();
				db.GetTable<CrossJoinResult>().Delete();

				db.Insert(new CrossJoinLeft() { Id = 1 });
				db.Insert(new CrossJoinLeft() { Id = 2 });
				db.Insert(new CrossJoinRight() { Id = 10 });
				db.Insert(new CrossJoinRight() { Id = 20 });
				db.Insert(new CrossJoinResult() { Id = 11, LeftId = 100, RightId = 200 });

				var source = from t1 in db.GetTable<CrossJoinLeft>()
							 from t2 in db.GetTable<CrossJoinRight>()
							 select new
							 {
								 LeftId   = t1.Id,
								 RightId  = t2.Id,
								 ResultId = t1.Id + t2.Id
							 };

				var rows = db.GetTable<CrossJoinResult>()
					.Merge()
					.Using(source)
					.On((t, s) => t.Id == s.ResultId)
					.InsertWhenNotMatched(s => new CrossJoinResult()
					{
						Id      = s.ResultId,
						LeftId  = s.LeftId,
						RightId = s.RightId
					})
					.Merge();

				var result = db.GetTable<CrossJoinResult>().OrderBy(_ => _.Id).ToList();

				AssertRowCount(3, rows, context);

				Assert.That(result, Has.Count.EqualTo(4));
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[0].Id, Is.EqualTo(11));
					Assert.That(result[0].LeftId, Is.EqualTo(100));
					Assert.That(result[0].RightId, Is.EqualTo(200));

					Assert.That(result[1].Id, Is.EqualTo(12));
					Assert.That(result[1].LeftId, Is.EqualTo(2));
					Assert.That(result[1].RightId, Is.EqualTo(10));

					Assert.That(result[2].Id, Is.EqualTo(21));
					Assert.That(result[2].LeftId, Is.EqualTo(1));
					Assert.That(result[2].RightId, Is.EqualTo(20));

					Assert.That(result[3].Id, Is.EqualTo(22));
					Assert.That(result[3].LeftId, Is.EqualTo(2));
					Assert.That(result[3].RightId, Is.EqualTo(20));
				}
			}
		}

		[Test]
		public void InsertFromCrossJoinedSourceQuery3([MergeDataContextSource(ProviderName.DB2, TestProvName.AllSapHana)] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var source = from t1 in db.GetTable<TestMapping1>().TableName("TestMerge1")
							 from t2 in db.GetTable<TestMapping1>().TableName("TestMerge2")
							 select new TestMapping1()
							 {
								 Id     = t1.Id,
								 Fake   = t2.Fake,
								 Field1 = t1.Field1,
								 Field2 = t2.Field2,
								 Field3 = t1.Field3,
								 Field4 = t2.Field4,
								 Field5 = t1.Field5
							 };

				var rows = table
					.Merge()
					.Using(source)
					.OnTargetKey()
					.InsertWhenNotMatched()
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(0, rows, context);
			}
		}

		[Test]
		public void InsertFromSelectManySourceQuery([MergeDataContextSource(false)] string context)
		{
			using (var db = GetDataConnection(context))
			using (db.BeginTransaction())
			{
				// prepare test data
				db.GetTable<CrossJoinLeft>().Delete();
				db.GetTable<CrossJoinRight>().Delete();
				db.GetTable<CrossJoinResult>().Delete();

				db.Insert(new CrossJoinLeft()   { Id = 1 });
				db.Insert(new CrossJoinLeft()   { Id = 2 });
				db.Insert(new CrossJoinRight()  { Id = 10 });
				db.Insert(new CrossJoinRight()  { Id = 20 });
				db.Insert(new CrossJoinResult() { Id = 11, LeftId = 100, RightId = 200 });

				var source = db.GetTable<CrossJoinLeft>()
					.SelectMany(
						r => db.GetTable<CrossJoinRight>(),
						(t1, t2) =>
						 new
							 {
								 LeftId   = t1.Id,
								 RightId  = t2.Id,
								 ResultId = t1.Id + t2.Id
							 });

				var rows = db.GetTable<CrossJoinResult>()
					.Merge()
					.Using(source)
					.On((t, s) => t.Id == s.ResultId)
					.InsertWhenNotMatched(s => new CrossJoinResult()
					{
						Id      = s.ResultId,
						LeftId  = s.LeftId,
						RightId = s.RightId
					})
					.Merge();

				var result = db.GetTable<CrossJoinResult>().OrderBy(_ => _.Id).ToList();

				AssertRowCount(3, rows, context);

				Assert.That(result, Has.Count.EqualTo(4));
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[0].Id, Is.EqualTo(11));
					Assert.That(result[0].LeftId, Is.EqualTo(100));
					Assert.That(result[0].RightId, Is.EqualTo(200));

					Assert.That(result[1].Id, Is.EqualTo(12));
					Assert.That(result[1].LeftId, Is.EqualTo(2));
					Assert.That(result[1].RightId, Is.EqualTo(10));

					Assert.That(result[2].Id, Is.EqualTo(21));
					Assert.That(result[2].LeftId, Is.EqualTo(1));
					Assert.That(result[2].RightId, Is.EqualTo(20));

					Assert.That(result[3].Id, Is.EqualTo(22));
					Assert.That(result[3].LeftId, Is.EqualTo(2));
					Assert.That(result[3].RightId, Is.EqualTo(20));
				}
			}
		}

		[Test]
		public void InsertFromPartialSourceProjection_UnknownFieldInDefaultSetter([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var exception = Assert.Catch(
					() => table
						.Merge()
						.Using(table.Select(_ => new TestMapping1() { Id = _.Id, Field1 = _.Field1 }))
						.OnTargetKey()
						.InsertWhenNotMatched()
						.Merge())!;

				Assert.That(exception, Is.InstanceOf<LinqToDBException>());
				Assert.That(exception.Message,  Does.EndWith("'source.Field2' could not be converted to SQL."));
			}
		}

		[Test]
		public void InsertFromPartialSourceProjection_UnknownFieldInSetter([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var exception = Assert.Catch(
					() => table
						.Merge()
						.Using(table.Select(_ => new TestMapping1() { Id = _.Id }))
						.OnTargetKey()
						.InsertWhenNotMatched(s => new TestMapping1()
						{
							Id     = s.Id,
							Field1 = s.Field3
						})
						.Merge())!;

				Assert.That(exception, Is.InstanceOf<LinqToDBException>());

				Assert.That(exception.Message,  Does.EndWith("s.Field3' could not be converted to SQL."));
			}
		}
		#endregion

		#region Insert<TEntity>(predicate)
		[Test]
		public void SameSourceInsertWithPredicate([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db).Select(_ => new TestMapping1()
					{
						Id     = _.Id,
						Field1 = _.Field3,
						Field2 = _.Field4,
						Field3 = _.Id,
						Field4 = _.Id + _.Id + _.Id,
						Field5 = _.Field4
					}))
					.OnTargetKey()
					.InsertWhenNotMatchedAnd(source => source.Field5 != null)
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[3].Id));
					Assert.That(result[4].Field1, Is.Null);
					Assert.That(result[4].Field2, Is.EqualTo(216));
					Assert.That(result[4].Field3, Is.Null);
					Assert.That(result[4].Field4, Is.EqualTo(18));
					Assert.That(result[4].Field5, Is.Null);
				}
			}
		}
		#endregion

		#region Insert<TEntity>(create)
		[Test]
		public void SameSourceInsertWithCreate([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db))
					.OnTargetKey()
					.InsertWhenNotMatched(_ => new TestMapping1()
					{
						Id     = 10 + _.Id,
						Field1 = 123,
						Field2 = _.Field1,
						Field3 = _.Field2,
						Field4 = 999,
						Field5 = 888
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id + 10));
					Assert.That(result[4].Field1, Is.EqualTo(123));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field1));
					Assert.That(result[4].Field3, Is.EqualTo(4));
					Assert.That(result[4].Field4, Is.EqualTo(999));
					Assert.That(result[4].Field5, Is.EqualTo(888));

					Assert.That(result[5].Id, Is.EqualTo(InitialSourceData[3].Id + 10));
					Assert.That(result[5].Field1, Is.EqualTo(123));
					Assert.That(result[5].Field2, Is.EqualTo(InitialSourceData[3].Field1));
					Assert.That(result[5].Field3, Is.Null);
					Assert.That(result[5].Field4, Is.EqualTo(999));
					Assert.That(result[5].Field5, Is.EqualTo(888));
				}
			}
		}

		[Test]
		public void SameSourceInsertWithComplexSetter([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var name = "test";
				var idx  = 6;
				var rows = table
					.Merge()
					.Using(GetSource1(db))
					.OnTargetKey()
					.InsertWhenNotMatched(_ => new TestMapping1()
					{
						Id     = 10 + _.Id,
						Field1 = 123,
						Field2 = Sql.AsSql(name).Length + idx,
						Field3 = _.Field2,
						Field4 = 999,
						Field5 = 888
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id + 10));
					Assert.That(result[4].Field1, Is.EqualTo(123));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field1));
					Assert.That(result[4].Field3, Is.EqualTo(4));
					Assert.That(result[4].Field4, Is.EqualTo(999));
					Assert.That(result[4].Field5, Is.EqualTo(888));

					Assert.That(result[5].Id, Is.EqualTo(InitialSourceData[3].Id + 10));
					Assert.That(result[5].Field1, Is.EqualTo(123));
					Assert.That(result[5].Field2, Is.EqualTo(10));
					Assert.That(result[5].Field3, Is.Null);
					Assert.That(result[5].Field4, Is.EqualTo(999));
					Assert.That(result[5].Field5, Is.EqualTo(888));
				}
			}
		}

		[Test]
		public void InsertPartialSourceProjection_KnownFieldInSetter([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db)
						.Select(s => new TestMapping1() { Id = s.Id, Field1 = s.Field1, Field2 = s.Field2}))
					.OnTargetKey()
					.InsertWhenNotMatched(_ => new TestMapping1()
					{
						Id     = 10 + _.Id,
						Field1 = 123,
						Field2 = _.Field1,
						Field3 = _.Field2,
						Field4 = 999,
						Field5 = 888
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id + 10));
					Assert.That(result[4].Field1, Is.EqualTo(123));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field1));
					Assert.That(result[4].Field3, Is.EqualTo(4));
					Assert.That(result[4].Field4, Is.EqualTo(999));
					Assert.That(result[4].Field5, Is.EqualTo(888));

					Assert.That(result[5].Id, Is.EqualTo(InitialSourceData[3].Id + 10));
					Assert.That(result[5].Field1, Is.EqualTo(123));
					Assert.That(result[5].Field2, Is.EqualTo(InitialSourceData[3].Field1));
					Assert.That(result[5].Field3, Is.Null);
					Assert.That(result[5].Field4, Is.EqualTo(999));
					Assert.That(result[5].Field5, Is.EqualTo(888));
				}
			}
		}

		[Test]
		public void DataContextTest([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db))
					.OnTargetKey()
					.InsertWhenNotMatched(_ => new TestMapping1
					{
						Id     = 10 + _.Id,
						Field1 = 123,
						Field2 = _.Field1,
						Field3 = _.Field2,
						Field4 = 999,
						Field5 = 888
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id + 10));
					Assert.That(result[4].Field1, Is.EqualTo(123));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field1));
					Assert.That(result[4].Field3, Is.EqualTo(4));
					Assert.That(result[4].Field4, Is.EqualTo(999));
					Assert.That(result[4].Field5, Is.EqualTo(888));

					Assert.That(result[5].Id, Is.EqualTo(InitialSourceData[3].Id + 10));
					Assert.That(result[5].Field1, Is.EqualTo(123));
					Assert.That(result[5].Field2, Is.EqualTo(InitialSourceData[3].Field1));
					Assert.That(result[5].Field3, Is.Null);
					Assert.That(result[5].Field4, Is.EqualTo(999));
					Assert.That(result[5].Field5, Is.EqualTo(888));
				}
			}
		}
		#endregion

		#region Insert<TEntity>(predicate, create)
		[Test]
		public void SameSourceInsertWithPredicateAndCreate([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db))
					.OnTargetKey()
					.InsertWhenNotMatchedAnd(
						_ => _.Field2 != null,
						_ => new TestMapping1()
						{
							Id     = 10 + _.Id,
							Field1 = 123,
							Field2 = _.Field1,
							Field3 = _.Field2,
							Field4 = 999,
							Field5 = 888
						})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id + 10));
					Assert.That(result[4].Field1, Is.EqualTo(123));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field1));
					// SkipInsert is ignored by explicit insert. Is it correct?
					//Assert.IsNull(result[4].Field3);
					Assert.That(result[4].Field3, Is.EqualTo(4));
					Assert.That(result[4].Field4, Is.EqualTo(999));
					//Assert.IsNull(result[4].Field5);
					Assert.That(result[4].Field5, Is.EqualTo(888));
				}
			}
		}

		[Test]
		public void InsertWithPredicatePartialSourceProjection_KnownFieldInCondition([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource1(db)
						.Select(s => new TestMapping1() { Id = s.Id, Field2 = s.Field2, Field1 = s.Field1 }))
					.OnTargetKey()
					.InsertWhenNotMatchedAnd(
						_ => _.Field2 != null,
						_ => new TestMapping1()
						{
							Id     = 10 + _.Id,
							Field1 = 123,
							Field2 = _.Field1,
							Field3 = _.Field2,
							Field4 = 999,
							Field5 = 888
						})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id + 10));
					Assert.That(result[4].Field1, Is.EqualTo(123));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field1));
					// SkipInsert is ignored by explicit insert. Is it correct?
					//Assert.IsNull(result[4].Field3);
					Assert.That(result[4].Field3, Is.EqualTo(4));
					Assert.That(result[4].Field4, Is.EqualTo(999));
					//Assert.IsNull(result[4].Field5);
					Assert.That(result[4].Field5, Is.EqualTo(888));
				}
			}
		}

		[Test]
		public void SameSourceInsertWithPredicateAndCreatePartialSourceProjection_UnknownFieldInCondition([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var exception = Assert.Catch(
					() => table
					    .Merge()
					    .Using(GetSource1(db).Select(_ => new TestMapping1() { Id = _.Id, Field1 = _.Field1 }))
					    .OnTargetKey()
					    .InsertWhenNotMatchedAnd(
						_ => _.Field2 != null,
						_ => new TestMapping1()
						{
							Id     = 10 + _.Id,
							Field1 = 123,
							Field2 = _.Field1,
							Field4 = 999,
							Field5 = 888
						})
					.Merge())!;

				Assert.That(exception, Is.InstanceOf<LinqToDBException>());
				Assert.That(exception.Message,  Does.EndWith(".Field2' could not be converted to SQL."));
			}
		}
		#endregion

		#region Insert<TTarget, TSource>(create) + different source/match combinations
		[Test]
		public void OtherSourceInsertFromTable([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db))
					.On((t, s) => t.Id == s.OtherId)
					.InsertWhenNotMatched(s => new TestMapping1()
					{
						Id     = s.OtherId,
						Field1 = s.OtherField1,
						Field2 = s.OtherField2,
						Field3 = s.OtherField3,
						Field4 = s.OtherField4,
						Field5 = s.OtherField5
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[2], result[4], null, null);
				AssertRow(InitialSourceData[3], result[5], null, 216);
			}
		}

		[Test]
		public void OtherSourceInsertFromQuery([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db).Where(_ => _.OtherId == 5))
					.On((t, s) => t.Id == s.OtherId && s.OtherField3 != null)
					.InsertWhenNotMatched(s => new TestMapping1()
					{
						Id     = s.OtherId,
						Field1 = s.OtherField5,
						Field2 = s.OtherField4,
						Field3 = s.OtherField3,
						Field4 = s.OtherField2,
						Field5 = s.OtherField1
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id));
					Assert.That(result[4].Field1, Is.Null);
					Assert.That(result[4].Field2, Is.Null);
					Assert.That(result[4].Field3, Is.EqualTo(InitialSourceData[2].Field3));
					Assert.That(result[4].Field4, Is.EqualTo(4));
					Assert.That(result[4].Field5, Is.EqualTo(10));
				}
			}
		}

		[Test]
		public void OtherSourceInsertFromQueryWithSelect([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db).Select(_ => new TestMapping2()
					{
						OtherId     = _.OtherId,
						OtherField1 = _.OtherField1,
						OtherField2 = _.OtherField2,
						OtherField3 = _.OtherId + _.OtherId,
						OtherField4 = _.OtherId + _.OtherId,
						OtherField5 = _.OtherField5
					}))
					.On((t, s) => t.Id == s.OtherId)
					.InsertWhenNotMatched(s => new TestMapping1()
					{
						Id     = s.OtherId,
						Field1 = s.OtherField3,
						Field2 = s.OtherField2,
						Field3 = s.OtherField1,
						Field4 = s.OtherField4,
						Field5 = s.OtherField5
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id));
					Assert.That(result[4].Field1, Is.EqualTo(10));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field2));
					Assert.That(result[4].Field3, Is.EqualTo(InitialSourceData[2].Field1));
					Assert.That(result[4].Field4, Is.EqualTo(10));
					Assert.That(result[4].Field5, Is.Null);

					Assert.That(result[5].Id, Is.EqualTo(InitialSourceData[3].Id));
					Assert.That(result[5].Field1, Is.EqualTo(12));
					Assert.That(result[5].Field2, Is.EqualTo(InitialSourceData[3].Field2));
					Assert.That(result[5].Field3, Is.EqualTo(InitialSourceData[3].Field1));
					Assert.That(result[5].Field4, Is.EqualTo(12));
					Assert.That(result[5].Field5, Is.Null);
				}
			}
		}

		[Test]
		public void OtherSourceInsertFromList([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetInitialSourceData2())
					.On((t, s) => t.Id == s.OtherId)
					.InsertWhenNotMatched(s => new TestMapping1()
					{
						Id     = s.OtherId,
						Field1 = s.OtherField1,
						Field2 = s.OtherField5,
						Field3 = s.OtherField2,
						Field4 = s.OtherField4,
						Field5 = s.OtherField3
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[4].Id, Is.EqualTo(InitialSourceData[2].Id));
					Assert.That(result[4].Field1, Is.EqualTo(InitialSourceData[2].Field1));
					Assert.That(result[4].Field2, Is.EqualTo(InitialSourceData[2].Field5));
					Assert.That(result[4].Field3, Is.EqualTo(InitialSourceData[2].Field2));
					Assert.That(result[4].Field4, Is.EqualTo(InitialSourceData[2].Field4));
					Assert.That(result[4].Field5, Is.Null);

					Assert.That(result[5].Id, Is.EqualTo(InitialSourceData[3].Id));
					Assert.That(result[5].Field1, Is.EqualTo(InitialSourceData[3].Field1));
					Assert.That(result[5].Field2, Is.EqualTo(InitialSourceData[3].Field5));
					Assert.That(result[5].Field3, Is.EqualTo(InitialSourceData[3].Field2));
					Assert.That(result[5].Field4, Is.EqualTo(InitialSourceData[3].Field4));
					Assert.That(result[5].Field5, Is.EqualTo(116));
				}
			}
		}

		[Test]
		public void OtherSourceInsertFromEmptyList([MergeDataContextSource(TestProvName.AllOracle)] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(Array.Empty<TestMapping2>())
					.On((t, s) => t.Id == s.OtherId)
					.InsertWhenNotMatched(s => new TestMapping1()
					{
						Id     = s.OtherId,
						Field1 = s.OtherField1,
						Field2 = s.OtherField5,
						Field3 = s.OtherField2,
						Field4 = s.OtherField4,
						Field5 = s.OtherField3
					})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();
				using (Assert.EnterMultipleScope())
				{
					Assert.That(rows, Is.Zero);

					Assert.That(result, Has.Count.EqualTo(4));
				}

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
			}
		}
		#endregion

		#region Insert<TTarget, TSource>(predicate, create)
		[Test]
		public void OtherSourceInsertWithPredicate([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db))
					.On((t, s) => t.Id == s.OtherId)
					.InsertWhenNotMatchedAnd(
						s => s.OtherField4 == 216,
						s => new TestMapping1()
						{
							Id     = s.OtherId,
							Field1 = s.OtherField1,
							Field2 = s.OtherField2,
							Field3 = s.OtherField3,
							Field4 = s.OtherField4,
							Field5 = s.OtherField5
						})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		[Test]
		public void AnonymousSourceInsertWithPredicate([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db).Select(_ => new
					{
						Key     = _.OtherId,
						Field01 = _.OtherField1,
						Field02 = _.OtherField2,
						Field03 = _.OtherField3,
						Field04 = _.OtherField4,
						Field05 = _.OtherField5,
					}))
					.On((t, s)  => t.Id == s.Key)
					.InsertWhenNotMatchedAnd(
						s       => s.Field04 == 216,
						s       => new TestMapping1()
						{
							Id     = s.Key,
							Field1 = s.Field01,
							Field2 = s.Field02,
							Field3 = s.Field03,
							Field4 = s.Field04,
							Field5 = s.Field05
						})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		[Test]
		public void AnonymousListSourceInsertWithPredicate([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db).OrderBy(_ => _.OtherId).ToList().Select(_ => new
					{
						Key     = _.OtherId,
						Field01 = _.OtherField1,
						Field02 = _.OtherField2,
						Field03 = _.OtherField3,
						Field04 = _.OtherField4,
						Field05 = _.OtherField5,
					}))
					.On((t, s)  => t.Id == s.Key)
					.InsertWhenNotMatchedAnd(
						s       => s.Field04 == 216,
						s       => new TestMapping1()
						{
							Id     = s.Key,
							Field1 = s.Field01,
							Field2 = s.Field02,
							Field3 = s.Field03,
							Field4 = s.Field04,
							Field5 = s.Field05
						})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				AssertRowCount(5, result.Count, context);

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				if (result.Count != 6)
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		[Test]
		public void InsertReservedAndCaseNames([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db).Select(_ => new
					{
						field  = _.OtherId,
						Field  = _.OtherField1,
						and    = _.OtherField2,
						Target = _.OtherField3,
						Source = _.OtherField4,
						@case  = _.OtherField5
					}))
					.On((t, s) => t.Id == s.field)
					.InsertWhenNotMatchedAnd(
						s      => s.Source == 216,
						s      => new TestMapping1()
						{
							Id     = s.field,
							Field1 = s.Field,
							Field2 = s.and,
							Field3 = s.Target,
							Field4 = s.Source,
							Field5 = s.@case
						})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}

		[Test]
		public void InsertReservedAndCaseNamesFromList([MergeDataContextSource(
			TestProvName.AllInformix, TestProvName.AllSapHana, ProviderName.Firebird25)]
			string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = table
					.Merge()
					.Using(GetSource2(db).ToList().OrderBy(s => s.OtherId).Select(_ => new
					{
						@as    = _.OtherId,
						take   = _.OtherField1,
						skip   = _.OtherField2,
						Skip   = _.OtherField3,
						insert = _.OtherField4,
						SELECT = _.OtherField5
					}))
					.On((t, s) => t.Id == s.@as)
					.InsertWhenNotMatchedAnd(
						s      => s.insert == 216,
						s      => new TestMapping1()
						{
							Id     = s.@as,
							Field1 = s.take,
							Field2 = s.skip,
							Field3 = s.Skip,
							Field4 = s.insert,
							Field5 = s.SELECT
						})
					.Merge();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				if (!context.IsAnyOf(ProviderName.Sybase))
					Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				if (!context.IsAnyOf(ProviderName.Sybase))
					AssertRow(InitialSourceData[3], result[4], null, 216);
			}
		}
		#endregion

		#region Async
		[Test]
		public async Task SameSourceInsertFromTableAsync([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = await table
					.Merge()
					.Using(GetSource1(db))
					.OnTargetKey()
					.InsertWhenNotMatched()
					.MergeAsync();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(2, rows, context);

				Assert.That(result, Has.Count.EqualTo(6));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[2], result[4], null, null);
				AssertRow(InitialSourceData[3], result[5], null, 216);
			}
		}

		[Test]
		public async Task SameSourceInsertFromQueryAsync([MergeDataContextSource] string context)
		{
			using (var db = GetDataContext(context))
			{
				PrepareData(db);

				var table = GetTarget(db);

				var rows = await table
					.Merge()
					.Using(GetSource1(db).Where(_ => _.Id == 5))
					.OnTargetKey()
					.InsertWhenNotMatched()
					.MergeAsync();

				var result = table.OrderBy(_ => _.Id).ToList();

				AssertRowCount(1, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));

				AssertRow(InitialTargetData[0], result[0], null, null);
				AssertRow(InitialTargetData[1], result[1], null, null);
				AssertRow(InitialTargetData[2], result[2], null, 203);
				AssertRow(InitialTargetData[3], result[3], null, null);
				AssertRow(InitialSourceData[2], result[4], null, null);
			}
		}
		#endregion

		// https://imgflip.com/i/2a6oc8
		[ActiveIssue(
			Configuration = TestProvName.AllSybase,
			Details       = "Cross-join doesn't work in Sybase. Also see SqlLinqCrossJoinSubQuery test")]
		[Test]
		public void CrossJoinedSourceWithSingleFieldSelection([MergeDataContextSource(false)] string context)
		{
			using (var db = GetDataConnection(context))
			using (db.BeginTransaction())
			{
				// prepare test data
				db.GetTable<CrossJoinLeft>().Delete();
				db.GetTable<CrossJoinRight>().Delete();
				db.GetTable<CrossJoinResult>().Delete();

				db.Insert(new CrossJoinLeft() { Id = 1 });
				db.Insert(new CrossJoinLeft() { Id = 2 });
				db.Insert(new CrossJoinRight() { Id = 10 });
				db.Insert(new CrossJoinRight() { Id = 20 });
				db.Insert(new CrossJoinResult() { Id = 11, LeftId = 100, RightId = 200 });

				var source = from t1 in db.GetTable<CrossJoinLeft>()
							 from t2 in db.GetTable<CrossJoinRight>()
							 select new
							 {
								 RightId = t2.Id
							 };

				var rows = db.GetTable<CrossJoinResult>()
					.Merge()
					.Using(source)
					.On((t, s) => t.Id == s.RightId)
					.InsertWhenNotMatched(s => new CrossJoinResult()
					{
						RightId = s.RightId
					})
					.Merge();

				// sort on client, see SortedMergeResultsIssue test for details
				var result = db.GetTable<CrossJoinResult>().AsEnumerable().OrderBy(_ => _.Id).ThenBy(_ => _.RightId).ToList();

				AssertRowCount(4, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[0].Id, Is.Zero);
					Assert.That(result[0].LeftId, Is.Zero);
					Assert.That(result[0].RightId, Is.EqualTo(10));

					Assert.That(result[1].Id, Is.Zero);
					Assert.That(result[1].LeftId, Is.Zero);
					Assert.That(result[1].RightId, Is.EqualTo(10));

					Assert.That(result[2].Id, Is.Zero);
					Assert.That(result[2].LeftId, Is.Zero);
					Assert.That(result[2].RightId, Is.EqualTo(20));

					Assert.That(result[3].Id, Is.Zero);
					Assert.That(result[3].LeftId, Is.Zero);
					Assert.That(result[3].RightId, Is.EqualTo(20));

					Assert.That(result[4].Id, Is.EqualTo(11));
					Assert.That(result[4].LeftId, Is.EqualTo(100));
					Assert.That(result[4].RightId, Is.EqualTo(200));
				}
			}
		}

		// same as CrossJoinedSourceWithSingleFieldSelection test but with server-side sort
		// it returns incorrectly ordered data for DB2, PostgreSQL and Oracle for some reason
		[ActiveIssue(Configurations = new[] { ProviderName.DB2, TestProvName.AllOracle, TestProvName.AllSybase, TestProvName.AllPostgreSQL15Plus })]
		[Test]
		public void SortedMergeResultsIssue([MergeDataContextSource(false)] string context)
		{
			using (var db = GetDataConnection(context))
			using (db.BeginTransaction())
			{
				// prepare test data
				db.GetTable<CrossJoinLeft>().Delete();
				db.GetTable<CrossJoinRight>().Delete();
				db.GetTable<CrossJoinResult>().Delete();

				db.Insert(new CrossJoinLeft() { Id = 1 });
				db.Insert(new CrossJoinLeft() { Id = 2 });
				db.Insert(new CrossJoinRight() { Id = 10 });
				db.Insert(new CrossJoinRight() { Id = 20 });
				db.Insert(new CrossJoinResult() { Id = 11, LeftId = 100, RightId = 200 });

				var source = from t1 in db.GetTable<CrossJoinLeft>()
							 from t2 in db.GetTable<CrossJoinRight>()
							 select new
							 {
								 RightId = t2.Id
							 };

				var rows = db.GetTable<CrossJoinResult>()
					.Merge()
					.Using(source)
					.On((t, s) => t.Id == s.RightId)
					.InsertWhenNotMatched(s => new CrossJoinResult()
					{
						RightId = s.RightId
					})
					.Merge();

				var result = db.GetTable<CrossJoinResult>().OrderBy(_ => _.Id).ThenBy(_ => _.RightId).ToList();

				AssertRowCount(4, rows, context);

				Assert.That(result, Has.Count.EqualTo(5));
				using (Assert.EnterMultipleScope())
				{
					Assert.That(result[0].Id, Is.Zero);
					Assert.That(result[0].LeftId, Is.Zero);
					Assert.That(result[0].RightId, Is.EqualTo(10));

					Assert.That(result[1].Id, Is.Zero);
					Assert.That(result[1].LeftId, Is.Zero);
					Assert.That(result[1].RightId, Is.EqualTo(10));

					Assert.That(result[2].Id, Is.Zero);
					Assert.That(result[2].LeftId, Is.Zero);
					Assert.That(result[2].RightId, Is.EqualTo(20));

					Assert.That(result[3].Id, Is.Zero);
					Assert.That(result[3].LeftId, Is.Zero);
					Assert.That(result[3].RightId, Is.EqualTo(20));

					Assert.That(result[4].Id, Is.EqualTo(11));
					Assert.That(result[4].LeftId, Is.EqualTo(100));
					Assert.That(result[4].RightId, Is.EqualTo(200));
				}
			}
		}
	}
}
