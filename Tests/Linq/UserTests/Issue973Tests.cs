﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using LinqToDB;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.Mapping;

using NUnit.Framework;

using Shouldly;

using Tests.Model;

namespace Tests.UserTests
{
	/// <summary>
	/// Builder for parameterized sql IN expression
	/// </summary>
	/// <seealso cref="Sql.IExtensionCallBuilder" />
	public class InExpressionItemBuilder : Sql.IExtensionCallBuilder
	{
		/// <summary>
		/// Builds the parameterized sql IN expression
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <exception cref="ArgumentNullException">Values for \"In\" operation should not be empty - values</exception>
		/// <see cref="SqlExtensions.In{T}(Sql.ISqlExtension,T,IEnumerable{T})"/>
		/// <seealso cref="SqlExtensions.In{T}(Sql.ISqlExtension,T,IEnumerable{T})"/>
		public void Build(Sql.ISqExtensionBuilder builder)
		{
			var parameterName = (builder.Arguments[2] as MemberExpression)?.Member.Name ?? "p";

			var values = builder.GetValue<System.Collections.IEnumerable>("values")
				?.OfType<object>()
				.ToArray();

			if (values == null || values.Length == 0)
			{
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
				throw new ArgumentNullException(nameof(values), "Values for \"In\" operation should not be empty");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
			}

			foreach (var value in values)
			{
				var param = new SqlParameter(new DbDataType(value?.GetType() ?? typeof(object)), parameterName, value);
				builder.AddParameter("values", param);
			}
		}
	}

	public static class SqlExtensions
	{
		/// <summary>
		/// Sql IN expression extension
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ext">The ext.</param>
		/// <param name="field">The field.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		[Sql.Extension("{field} IN ({values, ', '})", IsPredicate = true, BuilderType = typeof(InExpressionItemBuilder), ServerSideOnly = true)]
		public static bool In<T>(this Sql.ISqlExtension? ext, [ExprParameter] T field, [SqlQueryDependent] IEnumerable<T> values)
		{
			return values.Contains(field);
		}

		[Sql.Extension("{field} IN ({values, ', '})", IsPredicate = true, BuilderType = typeof(InExpressionItemBuilder), ServerSideOnly = true)]
		public static bool In<T>(this Sql.ISqlExtension? ext, [ExprParameter] T field, [SqlQueryDependent] params T[] values)
		{
			return values.Contains(field);
		}
	}

	public class Issue973Tests : TestBase
	{
		private IQueryable<Parent> GetParents(IDataContext db, ICollection<int?> values)
		{
			var param = 4;
			var resultQuery =
					from o in db.GetTable<Parent>()
					where Sql.Ext.In(o.ParentID, values) || o.ParentID == param
					select o;
			return resultQuery;
		}

		private IQueryable<Parent> GetParentsNative(IDataContext db, ICollection<int?> values)
		{
			var param = 4;
			var resultQuery =
					from o in db.GetTable<Parent>()
					where values.Contains(o.ParentID) || o.ParentID == param
					select o;
			return resultQuery;
		}

		[Test]
		public void Test([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var values1 = new int?[] { 1, 2, 3, null };
				var values2 = new int?[] { 4, 5, 6, null };

				AreEqual(
					GetParentsNative(db, values1),
					GetParents(db, values1));

				AreEqual(
					GetParentsNative(db, values2),
					GetParents(db, values2));
			}
		}

		[Test]
		public void TestCache([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var values1 = new int?[] { 1, 2, 3, null };
				var values2 = new int?[] { 4, 5, 6, null };

				var query11 = GetParents(db, values1);
				var result11 = query11.ToArray();

				var cm1 = query11.GetCacheMissCount();

				var query12 = GetParents(db, values1);
				var result12 = query12.ToArray();

				query12.GetCacheMissCount().ShouldBe(cm1);	

				var query21  = GetParents(db, values2);
				var result21 = query21.ToArray();

				var cm2 = query21.GetCacheMissCount();

				var query22  = GetParents(db, values2);
				var result22 = query22.ToArray();

				query22.GetCacheMissCount().ShouldBe(cm2);	

			}
		}

	}
}
