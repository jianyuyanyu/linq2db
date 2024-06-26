// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------


#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.NoMetadata.Firebird
{
	public class Person
	{
		public int     PersonId   { get; set; } // integer
		public string  FirstName  { get; set; } = null!; // varchar(50)
		public string  LastName   { get; set; } = null!; // varchar(50)
		public string? MiddleName { get; set; } // varchar(50)
		public char    Gender     { get; set; } // char(1)

		#region Associations
		/// <summary>
		/// FK_Doctor_Person backreference
		/// </summary>
		public Doctor? Doctor { get; set; }

		/// <summary>
		/// INTEG_18 backreference
		/// </summary>
		public Patient? Patient { get; set; }
		#endregion
	}
}
