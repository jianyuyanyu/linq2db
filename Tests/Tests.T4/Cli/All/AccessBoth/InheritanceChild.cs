// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.Tools.Comparers;
using System;
using System.Collections.Generic;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.All.Access.Both
{
	[Table("InheritanceChild")]
	public class InheritanceChild : IEquatable<InheritanceChild>
	{
		[Column("InheritanceChildId" , DataType = DataType.Int32  , DbType = "Long"       , IsPrimaryKey = true)] public int     InheritanceChildId  { get; set; } // Long
		[Column("InheritanceParentId", DataType = DataType.Int32  , DbType = "Long"                            )] public int     InheritanceParentId { get; set; } // Long
		[Column("TypeDiscriminator"  , DataType = DataType.Int32  , DbType = "Long"                            )] public int?    TypeDiscriminator   { get; set; } // Long
		[Column("Name"               , DataType = DataType.VarChar, DbType = "VarChar(50)", Length       = 50  )] public string? Name                { get; set; } // VarChar(50)

		#region IEquatable<T> support
		private static readonly IEqualityComparer<InheritanceChild> _equalityComparer = ComparerBuilder.GetEqualityComparer<InheritanceChild>(c => c.InheritanceChildId);

		public bool Equals(InheritanceChild? other)
		{
			return _equalityComparer.Equals(this, other!);
		}

		public override int GetHashCode()
		{
			return _equalityComparer.GetHashCode(this);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as InheritanceChild);
		}
		#endregion
	}
}