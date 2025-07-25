﻿using System;
using System.Runtime.CompilerServices;

namespace LinqToDB.Internal.Extensions
{
	public static class TypeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Type UnwrapNullableType(this Type type)
			=> Nullable.GetUnderlyingType(type) ?? type;

		/// <summary>
		/// Returns <c>true</c> if type is reference type or <see cref="Nullable{T}"/>.
		/// </summary>
		/// <param name="type">Type to test.</param>
		/// <returns><c>true</c> if type is reference type or <see cref="Nullable{T}"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsNullableType(this Type type)
			=> !type.IsValueType || type.IsNullable();

		// don't change visibility, used by linq2db.EntityFramework
		public static bool IsInteger(this Type type)
		{
			type = type.UnwrapNullableType();

			return type    == typeof(int)
			       || type == typeof(long)
			       || type == typeof(short)
			       || type == typeof(byte)
			       || type == typeof(uint)
			       || type == typeof(ulong)
			       || type == typeof(ushort)
			       || type == typeof(sbyte)
			       || type == typeof(char);
		}

		internal static bool IsNumericType(this Type? type)
		{
			if (type == null)
				return false;

			type = type.UnwrapNullableType();

			return type.IsInteger()
			       || type == typeof(decimal)
			       || type == typeof(float)
			       || type == typeof(double);
		}

		internal static bool IsSignedInteger(this Type type)
		{
			return type    == typeof(int)
			       || type == typeof(long)
			       || type == typeof(short)
			       || type == typeof(sbyte);
		}

		internal static bool IsSignedType(this Type? type)
		{
			return type != null &&
			       (type.IsSignedInteger()
			        || type == typeof(decimal)
			        || type == typeof(double)
			        || type == typeof(float)
			       );
		}
	}
}
