﻿using System;
using System.Linq;

using LinqToDB.Mapping;

namespace LinqToDB.Internal.SqlQuery
{
	//TODO: Investigate how to implement only ISqlTableSource interface
	public sealed class SqlRawSqlTable : SqlTable
	{
		public string SQL      { get; }
		public bool   IsScalar { get; }

		public ISqlExpression[] Parameters { get; private set; }

		public SqlRawSqlTable(
			EntityDescriptor endtityDescriptor,
			string           sql,
			bool             isScalar,
			ISqlExpression[] parameters)
			: base(endtityDescriptor)
		{
			SQL        = sql        ?? throw new ArgumentNullException(nameof(sql));
			IsScalar   = isScalar;
			Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

			foreach (var value in parameters)
				if (value == null) throw new ArgumentNullException(nameof(parameters));
		}

		internal SqlRawSqlTable(
			int              id, 
			string           alias, 
			Type             objectType,
			SqlField[]       fields,
			string           sql,
			bool             isScalar,
			ISqlExpression[] parameters)
			: base(id, string.Empty, alias, new (string.Empty), objectType, null, fields, SqlTableType.RawSql, null, TableOptions.NotSet, null)
		{
			SQL        = sql;
			Parameters = parameters;
			IsScalar   = isScalar;
		}

		public SqlRawSqlTable(SqlRawSqlTable table, ISqlExpression[] parameters)
			: base(table.ObjectType, null, table.TableName)
		{
			Alias              = table.Alias;

			SequenceAttributes = table.SequenceAttributes;

			AddRange(table.Fields.Select(f => new SqlField(f)));

			SQL                = table.SQL;
			Parameters         = parameters;
		}

		public override QueryElementType ElementType  => QueryElementType.SqlRawSqlTable;

		public override QueryElementTextWriter ToString(QueryElementTextWriter writer)
		{
			writer
				.DebugAppendUniqueId(this)
				.AppendLine("(")
				.Append(SQL)
				.Append(')')
				.AppendLine();

			return writer;
		}

		#region IQueryElement Members

		public string SqlText => this.ToDebugString();

		#endregion
	}
}
