﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Internal.Async;
using LinqToDB.Internal.Linq;
using LinqToDB.Linq;

using NUnit.Framework;

using Tests.Model;

namespace Tests.xUpdate
{
	public partial class MergeTests
	{
		public static IEnumerable<TestCaseData> _nullParameterCases
		{
			get
			{
				return new TestDelegate[]
				{
					() => LinqExtensions.Merge<Child>(null!),

					() => LinqExtensions.Merge<Child>(null!, "hint"),
					() => LinqExtensions.Merge<Child>(new FakeTable<Child>(), null!),

					() => LinqExtensions.MergeInto<Child, Child>(null!, new FakeTable<Child>()),
					() => LinqExtensions.MergeInto<Child, Child>(Array.Empty<Child>().AsQueryable(), null!),

					() => LinqExtensions.MergeInto<Child, Child>(null!, new FakeTable<Child>(), "hint"),
					() => LinqExtensions.MergeInto<Child, Child>(Array.Empty<Child>().AsQueryable(), null!, "hint"),
					() => LinqExtensions.MergeInto<Child, Child>(Array.Empty<Child>().AsQueryable(), new FakeTable<Child>(), null!),

					() => LinqExtensions.Using<Child, Child>(null!, Array.Empty<Child>().AsQueryable()),
					() => LinqExtensions.Using<Child, Child>(new FakeMergeUsing<Child>(), null!),

					() => LinqExtensions.Using<Child, Child>(null!, []),
					() => LinqExtensions.Using<Child, Child>(new FakeMergeUsing<Child>(), null!),

					() => LinqExtensions.UsingTarget<Child>(null!),

					() => LinqExtensions.On<Child, Child, int>(null!, t => 1, s => 1),
					() => LinqExtensions.On<Child, Child, int>(new FakeMergeOn<Child, Child>(), null!, s => 1),
					() => LinqExtensions.On<Child, Child, int>(new FakeMergeOn<Child, Child>(), t => 1, null!),

					() => LinqExtensions.On<Child, Child>(null!, (t, s) => true),
					() => LinqExtensions.On<Child, Child>(new FakeMergeOn<Child, Child>(), null!),

					() => LinqExtensions.OnTargetKey<Child>(null!),

					() => LinqExtensions.InsertWhenNotMatched<Child>(null!),

					() => LinqExtensions.InsertWhenNotMatchedAnd<Child>(null!, c => true),
					() => LinqExtensions.InsertWhenNotMatchedAnd<Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.InsertWhenNotMatched<Child, Child>(null!, c => c),
					() => LinqExtensions.InsertWhenNotMatched<Child, Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.InsertWhenNotMatchedAnd<Child, Child>(null!, c => true, c => c),
					() => LinqExtensions.InsertWhenNotMatchedAnd<Child, Child>(new FakeMergeSource<Child, Child>(), null!, c => c),
					() => LinqExtensions.InsertWhenNotMatchedAnd<Child, Child>(new FakeMergeSource<Child, Child>(), c => true, null!),

					() => LinqExtensions.UpdateWhenMatched<Child>(null!),

					() => LinqExtensions.UpdateWhenMatchedAnd<Child>(null!, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedAnd<Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.UpdateWhenMatched<Child, Child>(null!, (t, s) => t),
					() => LinqExtensions.UpdateWhenMatched<Child, Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.UpdateWhenMatchedAnd<Child, Child>(null!, (t, s) => true, (t, s) => t),
					() => LinqExtensions.UpdateWhenMatchedAnd<Child, Child>(new FakeMergeSource<Child, Child>(), null!, (t, s) => t),
					() => LinqExtensions.UpdateWhenMatchedAnd<Child, Child>(new FakeMergeSource<Child, Child>(), (t, s) => true, null!),

					() => LinqExtensions.UpdateWhenMatchedThenDelete<Child>(null!, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedThenDelete<Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.UpdateWhenMatchedAndThenDelete<Child>(null!, (t, s) => true, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedAndThenDelete<Child>(new FakeMergeSource<Child, Child>(), null!, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedAndThenDelete<Child>(new FakeMergeSource<Child, Child>(), (t, s) => true, null!),

					() => LinqExtensions.UpdateWhenMatchedThenDelete<Child, Child>(null!, (t, s) => t, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedThenDelete<Child, Child>(new FakeMergeSource<Child, Child>(), null!, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedThenDelete<Child, Child>(new FakeMergeSource<Child, Child>(), (t, s) => t, null!),

					() => LinqExtensions.UpdateWhenMatchedAndThenDelete<Child, Child>(null!, (t, s) => true, (t, s) => t, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedAndThenDelete<Child, Child>(new FakeMergeSource<Child, Child>(), null!, (t, s) => t, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedAndThenDelete<Child, Child>(new FakeMergeSource<Child, Child>(), (t, s) => true, null!, (t, s) => true),
					() => LinqExtensions.UpdateWhenMatchedAndThenDelete<Child, Child>(new FakeMergeSource<Child, Child>(), (t, s) => true, (t, s) => t, null!),

					() => LinqExtensions.DeleteWhenMatched<Child, Child>(null!),

					() => LinqExtensions.DeleteWhenMatchedAnd<Child, Child>(null!, (t, s) => true),
					() => LinqExtensions.DeleteWhenMatchedAnd<Child, Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.UpdateWhenNotMatchedBySource<Child, Child>(null!, t => t),
					() => LinqExtensions.UpdateWhenNotMatchedBySource<Child, Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.UpdateWhenNotMatchedBySourceAnd<Child, Child>(null!, t => true, t => t),
					() => LinqExtensions.UpdateWhenNotMatchedBySourceAnd<Child, Child>(new FakeMergeSource<Child, Child>(), null!, t => t),
					() => LinqExtensions.UpdateWhenNotMatchedBySourceAnd<Child, Child>(new FakeMergeSource<Child, Child>(), t => true, null!),

					() => LinqExtensions.DeleteWhenNotMatchedBySource<Child, Child>(null!),

					() => LinqExtensions.DeleteWhenNotMatchedBySourceAnd<Child, Child>(null!, t => true),
					() => LinqExtensions.DeleteWhenNotMatchedBySourceAnd<Child, Child>(new FakeMergeSource<Child, Child>(), null!),

					() => LinqExtensions.Merge<Child, Child>(null!)
				}.Select((data, i) => new TestCaseData(data).SetName($"Merge.API.Null Parameters.{i}"));
			}
		}

		[TestCaseSource(nameof(_nullParameterCases))]
		public void MergeApiNullParameter(TestDelegate action)
		{
			Assert.Throws<ArgumentNullException>(action);
		}

		sealed class FakeMergeSource<TTarget, TSource> : IMergeableSource<TTarget, TSource>
		{ }

		sealed class FakeMergeUsing<TTarget> : IMergeableUsing<TTarget>
		{ }

		sealed class FakeMergeOn<TTarget, TSource> : IMergeableOn<TTarget, TSource>
		{ }

		private sealed class FakeMergeQuery<TTarget, TSource> :
			IMergeableUsing<TTarget>,
			IMergeableOn<TTarget, TSource>,
			IMergeableSource<TTarget, TSource>,
			IMergeable<TTarget, TSource>,
			IQueryable<TTarget>
		{
			private readonly Expression _expression;

			public FakeMergeQuery(Expression expression)
			{
				_expression = expression;
			}

			public IQueryable<TTarget> Query => this;

			public Expression Expression => _expression;

			public Type ElementType => throw new NotImplementedException();

			public IQueryProvider Provider => new FakeQueryProvider();

			public IEnumerator<TTarget> GetEnumerator()
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}

		sealed class FakeQueryProvider : IQueryProvider
		{
			IQueryable IQueryProvider.CreateQuery(Expression expression)
			{
				throw new NotImplementedException();
			}

			IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
			{
				return new FakeMergeQuery<TElement, TElement>(expression);
			}

			object IQueryProvider.Execute(Expression expression)
			{
				throw new NotImplementedException();
			}

			TResult IQueryProvider.Execute<TResult>(Expression expression)
			{
				throw new NotImplementedException();
			}
		}

		sealed class FakeTable<TEntity> : ITable<TEntity>
			where TEntity : notnull
		{
			IDataContext            IExpressionQuery.DataContext                                  => throw new NotImplementedException();
			Expression              IExpressionQuery.Expression                                   => throw new NotImplementedException();
			IReadOnlyList<QuerySql> IExpressionQuery.GetSqlQueries(SqlGenerationOptions? options) => throw new NotImplementedException();

			Type                    IQueryable.         ElementType                               => throw new NotImplementedException();
			Expression              IQueryable.         Expression                                => throw new NotImplementedException();
			IQueryProvider          IQueryable.         Provider                                  => new FakeQueryProvider();
			Expression              IQueryProviderAsync.Expression                                => throw new NotImplementedException();

			Expression IExpressionQuery<TEntity>.Expression => Expression.Constant((ITable<TEntity>)this);

			IQueryable IQueryProvider.CreateQuery(Expression expression)
			{
				throw new NotImplementedException();
			}

			IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
			{
				throw new NotImplementedException();
			}

			object IQueryProvider.Execute(Expression expression)
			{
				throw new NotImplementedException();
			}

			TResult IQueryProvider.Execute<TResult>(Expression expression)
			{
				throw new NotImplementedException();
			}

			Task<TResult> IQueryProviderAsync.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
			{
				throw new NotImplementedException();
			}

			Task<IAsyncEnumerable<TResult>> IQueryProviderAsync.ExecuteAsyncEnumerable<TResult>(Expression expression, CancellationToken cancellationToken)
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			public string?      ServerName   { get; }
			public string?      DatabaseName { get; }
			public string?      SchemaName   { get; }
			public string       TableName    { get; } = null!;
			public TableOptions TableOptions { get; }
			public string?      TableID      { get; }

			public string GetTableName()
			{
				throw new NotImplementedException();
			}
		}
	}
}
