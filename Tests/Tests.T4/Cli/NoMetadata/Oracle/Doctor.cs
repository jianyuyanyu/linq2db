// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------


#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.NoMetadata.Oracle
{
	public class Doctor
	{
		public decimal PersonId { get; set; } // NUMBER
		public string  Taxonomy { get; set; } = null!; // NVARCHAR2(50)

		#region Associations
		/// <summary>
		/// FK_Doctor_Person
		/// </summary>
		public Person Person { get; set; } = null!;
		#endregion
	}
}
