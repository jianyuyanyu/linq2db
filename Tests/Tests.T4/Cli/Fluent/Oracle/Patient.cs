// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------


#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.Fluent.Oracle
{
	public class Patient
	{
		public decimal PersonId  { get; set; } // NUMBER
		public string  Diagnosis { get; set; } = null!; // NVARCHAR2(256)

		#region Associations
		/// <summary>
		/// Fk_Patient_Person
		/// </summary>
		public Person FkPerson { get; set; } = null!;
		#endregion
	}
}