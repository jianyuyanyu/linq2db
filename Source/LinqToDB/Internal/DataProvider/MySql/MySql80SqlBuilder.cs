﻿using System.Text;

using LinqToDB.DataProvider;
using LinqToDB.Internal.SqlProvider;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.Mapping;

namespace LinqToDB.Internal.DataProvider.MySql
{
	public class MySql80SqlBuilder : MySqlSqlBuilder
	{
		public MySql80SqlBuilder(IDataProvider? provider, MappingSchema mappingSchema, DataOptions dataOptions, ISqlOptimizer sqlOptimizer, SqlProviderFlags sqlProviderFlags)
			: base(provider, mappingSchema, dataOptions, sqlOptimizer, sqlProviderFlags)
		{
		}

		protected override bool SupportsColumnAliasesInSource => true;

		MySql80SqlBuilder(BasicSqlBuilder parentBuilder) : base(parentBuilder)
		{
		}

		protected override ISqlBuilder CreateSqlBuilder()
		{
			return new MySql80SqlBuilder(this) { HintBuilder = HintBuilder };
		}

		protected override bool BuildJoinType(SqlJoinedTable join, SqlSearchCondition condition)
		{
			switch (join.JoinType)
			{
				case JoinType.CrossApply:
					// join with function implies lateral keyword
					if (join.Table.SqlTableType == SqlTableType.Function)
						StringBuilder.Append("INNER JOIN ");
					else
						StringBuilder.Append("INNER JOIN LATERAL ");
					return true;
				case JoinType.OuterApply:
					// join with function implies lateral keyword
					if (join.Table.SqlTableType == SqlTableType.Function)
						StringBuilder.Append("LEFT JOIN ");
					else
						StringBuilder.Append("LEFT JOIN LATERAL ");
					return true;
			}

			return base.BuildJoinType(join, condition);
		}
	}
}
