﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace LinqToDB.Internal.Cache
{
	enum EvictionReason
	{
		None,

		/// <summary>
		/// Manually
		/// </summary>
		Removed,

		/// <summary>
		/// Overwritten
		/// </summary>
		Replaced,

		/// <summary>
		/// Timed out
		/// </summary>
		Expired,

		/// <summary>
		/// Event
		/// </summary>
		TokenExpired,

		/// <summary>
		/// Overflow
		/// </summary>
		Capacity,
	}
}
