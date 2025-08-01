﻿using System.Linq.Expressions;

using LinqToDB.Internal.Expressions;
using LinqToDB.Internal.Reflection;

namespace LinqToDB.Internal.Linq.Builder
{
	[BuildsMethodCall(nameof(LinqInternalExtensions.ApplyModifierInternal))]
	sealed class ApplyModifierBuilder : MethodCallBuilder
	{
		public static bool CanBuildMethod(MethodCallExpression call)
			=> call.IsSameGenericMethod(Methods.LinqToDB.ApplyModifierInternal);

		protected override BuildSequenceResult BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			var modifier = methodCall.Arguments[1].EvaluateExpression<TranslationModifier>()!;
			builder.PushTranslationModifier(modifier, true);
			var sequence = builder.TryBuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));
			builder.PopTranslationModifier();

			return sequence;
		}
	}
}
