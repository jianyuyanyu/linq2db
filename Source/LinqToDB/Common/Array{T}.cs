﻿using System;
using System.ComponentModel;

namespace LinqToDB.Common
{
	/// <summary>
	/// Empty array instance helper.
	/// </summary>
	/// <typeparam name="T">Array element type.</typeparam>
	// TODO: Remove in v7 (cleanup of random unused code)
	[Obsolete("This API will be removed in version 7"), EditorBrowsable(EditorBrowsableState.Never)]
	public static class Array<T>
	{
		internal static T[] Append(T[] array, T newElement)
		{
			var oldSize = array.Length;

			Array.Resize(ref array, oldSize + 1);

			array[oldSize] = newElement;

			return array;
		}

		internal static T[] Append(T[] array, T[]? otherArray)
		{
			if (otherArray == null || otherArray.Length == 0)
				return array;

			var oldSize = array.Length;

			Array.Resize(ref array, oldSize + otherArray.Length);

			for (int i = 0; i < otherArray.Length; i++)
			{
				array[oldSize + i] = otherArray[i];
			}

			return array;
		}
	}
}
