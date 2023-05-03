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

namespace Cli.All.SapHana
{
	[Table("Person")]
	public class Person : IEquatable<Person>
	{
		[Column("PersonID"  , DataType  = DataType.Int32   , DbType   = "INTEGER"        , Length = 10            , Precision = 10, Scale     = 0 , IsPrimaryKey = true, IsIdentity = true, SkipOnInsert = true, SkipOnUpdate = true)] public int     PersonId   { get; set; } // INTEGER
		[Column("FirstName" , CanBeNull = false            , DataType = DataType.NVarChar, DbType = "NVARCHAR(50)", Length    = 50, Precision = 50, Scale        = 0                                                                )] public string  FirstName  { get; set; } = null!; // NVARCHAR(50)
		[Column("LastName"  , CanBeNull = false            , DataType = DataType.NVarChar, DbType = "NVARCHAR(50)", Length    = 50, Precision = 50, Scale        = 0                                                                )] public string  LastName   { get; set; } = null!; // NVARCHAR(50)
		[Column("MiddleName", DataType  = DataType.NVarChar, DbType   = "NVARCHAR(50)"   , Length = 50            , Precision = 50, Scale     = 0                                                                                   )] public string? MiddleName { get; set; } // NVARCHAR(50)
		[Column("Gender"    , DataType  = DataType.Char    , DbType   = "CHAR(1)"        , Length = 1             , Precision = 1 , Scale     = 0                                                                                   )] public char    Gender     { get; set; } // CHAR(1)

		#region IEquatable<T> support
		private static readonly IEqualityComparer<Person> _equalityComparer = ComparerBuilder.GetEqualityComparer<Person>(c => c.PersonId);

		public bool Equals(Person? other)
		{
			return _equalityComparer.Equals(this, other!);
		}

		public override int GetHashCode()
		{
			return _equalityComparer.GetHashCode(this);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as Person);
		}
		#endregion

		#region Associations
		/// <summary>
		/// FK_Doctor_Person backreference
		/// </summary>
		[Association(ThisKey = nameof(PersonId), OtherKey = nameof(SapHana.Doctor.PersonId))]
		public Doctor? Doctor { get; set; }

		/// <summary>
		/// FK_Patient_Person backreference
		/// </summary>
		[Association(ThisKey = nameof(PersonId), OtherKey = nameof(SapHana.Patient.PersonId))]
		public Patient? Patient { get; set; }
		#endregion
	}
}