// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB.Mapping;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.Interceptors.SQLite
{
	[Table("InheritanceChild")]
	public class InheritanceChild
	{
		[Column("InheritanceChildId" )] public long    InheritanceChildId  { get; internal set; } // integer
		[Column("InheritanceParentId")] public long    InheritanceParentId { get; internal set; } // integer
		[Column("TypeDiscriminator"  )] public long?   TypeDiscriminator   { get; internal set; } // integer
		[Column("Name"               )] public string? Name                { get; internal set; } // nvarchar(50)
	}
}