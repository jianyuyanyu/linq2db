﻿using System;

using LinqToDB.Internal.Extensions;
using LinqToDB.Internal.SqlProvider;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.SqlQuery;

namespace LinqToDB.Internal.DataProvider.Firebird
{
	public class FirebirdSqlExpressionConvertVisitor : SqlExpressionConvertVisitor
	{
		public FirebirdSqlExpressionConvertVisitor(bool allowModify) : base(allowModify)
		{
		}

		#region LIKE

		protected static string[] LikeFirebirdEscapeSymbols = { "_", "%" };

		public override string[] LikeCharactersToEscape    => LikeFirebirdEscapeSymbols;
		public override bool     LikeValueParameterSupport => false;

		#endregion

		public override IQueryElement ConvertSqlBinaryExpression(SqlBinaryExpression element)
		{
			var newElement = base.ConvertSqlBinaryExpression(element);

			if (!ReferenceEquals(newElement, element))
				return Visit(newElement);

			switch (element.Operation)
			{
				case "%": return new SqlFunction(element.Type, "Mod", element.Expr1, element.Expr2);
				case "&": return new SqlFunction(element.Type, "Bin_And", element.Expr1, element.Expr2);
				case "|": return new SqlFunction(element.Type, "Bin_Or", element.Expr1, element.Expr2);
				case "^": return new SqlFunction(element.Type, "Bin_Xor", element.Expr1, element.Expr2);
				case "+": return element.SystemType == typeof(string) ? new SqlBinaryExpression(element.SystemType, element.Expr1, "||", element.Expr2, element.Precedence) : element;
			}

			return element;
		}

		protected virtual bool? GetCaseSensitiveParameter(SqlPredicate.SearchString predicate)
		{
			var caseSensitive = predicate.CaseSensitive.EvaluateExpression(EvaluationContext);

			if (caseSensitive is char chr)
			{
				if (chr == '0')
					return false;

				if (chr == '1')
					return true;
			}
			else if (caseSensitive is bool boolValue)
				return boolValue;

			return null;
		}

		public override ISqlPredicate ConvertSearchStringPredicate(SqlPredicate.SearchString predicate)
		{
			ISqlExpression expr;

			var caseSensitive = GetCaseSensitiveParameter(predicate);

			// for explicit case-sensitive search we apply "CAST({0} AS BLOB)" to searched string as COLLATE's collation is character set-dependent
			switch (predicate.Kind)
			{
				case SqlPredicate.SearchString.SearchKind.EndsWith:
				{
					if (caseSensitive == false)
					{
						predicate = new SqlPredicate.SearchString(
							PseudoFunctions.MakeToLower(predicate.Expr1, MappingSchema),
							predicate.IsNot,
							PseudoFunctions.MakeToLower(predicate.Expr2, MappingSchema), predicate.Kind,
							predicate.CaseSensitive);
					}
					else if (caseSensitive == true)
					{
						predicate = new SqlPredicate.SearchString(
							new SqlExpression(MappingSchema.GetDbDataType(typeof(string)), "CAST({0} AS BLOB)", Precedence.Primary, predicate.Expr1),
							predicate.IsNot,
							predicate.Expr2,
							predicate.Kind,
							predicate.CaseSensitive);
					}

					return ConvertSearchStringPredicateViaLike(predicate);
				}
				case SqlPredicate.SearchString.SearchKind.StartsWith:
				{
					expr = new SqlExpression(MappingSchema.GetDbDataType(typeof(bool)),
						predicate.IsNot ? "{0} NOT STARTING WITH {1}" : "{0} STARTING WITH {1}",
						Precedence.Comparison,
						SqlFlags.IsPredicate,
						ParametersNullabilityType.IfAnyParameterNullable,
						TryConvertToValue(
							caseSensitive == false
								? PseudoFunctions.MakeToLower(predicate.Expr1, MappingSchema)
								: caseSensitive == true
									? new SqlExpression(MappingSchema.GetDbDataType(typeof(string)), "CAST({0} AS BLOB)", Precedence.Primary, predicate.Expr1)
									: predicate.Expr1,
							EvaluationContext),
						TryConvertToValue(
							caseSensitive == false
								? PseudoFunctions.MakeToLower(predicate.Expr2, MappingSchema)
								: predicate.Expr2, EvaluationContext)) {CanBeNull = false};
					break;
				}
				case SqlPredicate.SearchString.SearchKind.Contains:
				{
					if (caseSensitive == false)
					{
						expr = new SqlExpression(MappingSchema.GetDbDataType(typeof(bool)),
							predicate.IsNot ? "{0} NOT CONTAINING {1}" : "{0} CONTAINING {1}",
							precedence : Precedence.Comparison,
							flags : SqlFlags.IsPredicate,
							nullabilityType : ParametersNullabilityType.IfAnyParameterNullable,
							TryConvertToValue(predicate.Expr1, EvaluationContext),
							TryConvertToValue(predicate.Expr2, EvaluationContext)) { CanBeNull = false };
					}
					else
					{
						if (caseSensitive == true)
						{
							predicate = new SqlPredicate.SearchString(
								new SqlExpression(MappingSchema.GetDbDataType(typeof(string)), "CAST({0} AS BLOB)", Precedence.Primary, predicate.Expr1),
								predicate.IsNot,
								predicate.Expr2,
								predicate.Kind,
								new SqlValue(false));
						}

						return ConvertSearchStringPredicateViaLike(predicate);
					}

					break;
				}
				default:
					throw new InvalidOperationException($"Unexpected predicate: {predicate.Kind}");
			}

			return new SqlSearchCondition(false, canBeUnknown: null, new SqlPredicate.Expr(expr));
		}

		protected override ISqlExpression ConvertConversion(SqlCastExpression cast)
		{
			if (cast.SystemType.ToUnderlying() == typeof(bool))
			{
				if (cast.Type.DataType == DataType.Boolean && cast.Expression is not ISqlPredicate)
				{
					if (cast.Expression is SqlValue)
						return cast.Expression;

					var sc = new SqlSearchCondition()
						.AddNotEqual(cast.Expression, new SqlValue(QueryHelper.GetDbDataType(cast.Expression, MappingSchema), 0), DataOptions.LinqOptions.CompareNulls);
					return sc;
				}
			}
			else if (cast.SystemType.ToUnderlying() == typeof(string) && cast.Expression.SystemType?.ToUnderlying() == typeof(Guid))
			{
				// TODO: think how to use FirebirdMemberTranslator.GuidMemberTranslator.TranslateGuildToString instead of code duplication here
				return PseudoFunctions.MakeToLower(new SqlFunction(QueryHelper.GetDbDataType(cast, MappingSchema), "UUID_TO_CHAR", cast.Expression), MappingSchema);
			}
			else if (cast.SystemType.ToUnderlying() == typeof(Guid) && cast.Expression.SystemType?.ToUnderlying() == typeof(string))
			{
				return new SqlFunction(cast.Type, "CHAR_TO_UUID", cast.Expression);
			}
			else if (cast.ToType.DataType == DataType.Decimal)
			{
				if (cast.ToType.Precision == null && cast.ToType.Scale == null)
				{
					//TODO: check default precision and scale
					cast = cast.WithToType(cast.ToType.WithPrecisionScale(18, 10));
				}
			}

			cast = FloorBeforeConvert(cast);

			return base.ConvertConversion(cast);
		}

		protected override IQueryElement VisitExprPredicate(SqlPredicate.Expr predicate)
		{
			if (predicate.ElementType == QueryElementType.ExprPredicate && predicate.Expr1 is SqlParameter p && p.Type.DataType != DataType.Boolean)
			{
				predicate = new SqlPredicate.ExprExpr(p, SqlPredicate.Operator.Equal, MappingSchema.GetSqlValue(p.Type, true), DataOptions.LinqOptions.CompareNulls == CompareNulls.LikeClr ? true : null);
				return Visit(predicate);
			}

			return base.VisitExprPredicate(predicate);
		}

		public override ISqlExpression ConvertSqlFunction(SqlFunction func)
		{
			switch (func)
			{
				case { Name: PseudoFunctions.LENGTH }:
					return func.WithName("CHAR_LENGTH");

				default:
					return base.ConvertSqlFunction(func);
			}
		}

		protected override ISqlExpression WrapColumnExpression(ISqlExpression expr)
		{
			if (expr is SqlValue
				{
					Value: uint or long or ulong or float or double or decimal
				} value)
			{
				expr = new SqlCastExpression(expr, value.ValueType, null, isMandatory: true);
			}

			if (expr is SqlParameter { IsQueryParameter: false } param)
			{
				var paramType = param.Type.SystemType.UnwrapNullableType();
				if (paramType == typeof(uint)
					|| paramType == typeof(long)
					|| paramType == typeof(ulong)
					|| paramType == typeof(float)
					|| paramType == typeof(double)
					|| paramType == typeof(decimal))
					expr = new SqlCastExpression(expr, param.Type, null, isMandatory: true);
			}

			return base.WrapColumnExpression(expr);
		}
	}
}
