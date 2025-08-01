﻿using System.Collections.Generic;
using System.Linq.Expressions;

using LinqToDB.Internal.Expressions;
using LinqToDB.Internal.SqlQuery;

using static LinqToDB.Internal.Reflection.Methods.LinqToDB.Merge;

namespace LinqToDB.Internal.Linq.Builder
{
	internal partial class MergeBuilder
	{
		[BuildsMethodCall(nameof(LinqExtensions.UpdateWhenNotMatchedBySourceAnd))]
		internal sealed class UpdateWhenNotMatchedBySource : MethodCallBuilder
		{
			public static bool CanBuildMethod(MethodCallExpression call)
				=> call.IsSameGenericMethod(UpdateWhenNotMatchedBySourceAndMethodInfo);

			protected override BuildSequenceResult BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
			{
				// UpdateWhenNotMatchedBySourceAnd(merge, searchCondition, setter)
				var mergeContext = (MergeContext)builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));

				var statement = mergeContext.Merge;
				var operation = new SqlMergeOperationClause(MergeOperationType.UpdateBySource);
				statement.Operations.Add(operation);

				var predicate = methodCall.Arguments[1];
				var setterLambda = methodCall.Arguments[2].UnwrapLambda();

				var setterExpression = mergeContext.SourceContext.PrepareSelfTargetLambda(setterLambda);

				var setterExpressions    = new List<UpdateBuilder.SetExpressionEnvelope>();
				var targetRef = mergeContext.SourceContext.TargetContextRef.WithType(setterExpression.Type);
				UpdateBuilder.ParseSetter(builder,
					targetRef, targetRef, setterExpression,
					setterExpressions);

				UpdateBuilder.InitializeSetExpressions(builder, mergeContext.TargetContext, mergeContext.SourceContext, setterExpressions, operation.Items, false);

				if (!predicate.IsNullValue())
				{
					var condition          = predicate.UnwrapLambda();
					var conditionCorrected = mergeContext.SourceContext.PrepareSelfTargetLambda(condition);

					operation.Where = new SqlSearchCondition();

					var saveIsSourceOuter = mergeContext.SourceContext.IsSourceOuter;
					mergeContext.SourceContext.IsSourceOuter = true;

					builder.BuildSearchCondition(mergeContext.TargetContext, conditionCorrected, operation.Where);

					mergeContext.SourceContext.IsSourceOuter = saveIsSourceOuter;
				}

				return BuildSequenceResult.FromContext(mergeContext);
			}
		}
	}
}
