// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB.Mapping;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.Default.DB2
{
	[Table("Doctor")]
	public class Doctor
	{
		[Column("PersonID", IsPrimaryKey = true )] public int    PersonId { get; set; } // INTEGER
		[Column("Taxonomy", CanBeNull    = false)] public string Taxonomy { get; set; } = null!; // VARCHAR(50)

		#region Associations
		/// <summary>
		/// FK_Doctor_Person
		/// </summary>
		[Association(CanBeNull = false, ThisKey = nameof(PersonId), OtherKey = nameof(DB2.Person.PersonId))]
		public Person Person { get; set; } = null!;
		#endregion
	}
}
