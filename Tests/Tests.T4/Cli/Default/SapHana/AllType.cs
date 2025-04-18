// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB.Mapping;
using System;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.Default.SapHana
{
	[Table("AllTypes")]
	public class AllType
	{
		[Column("ID"                  , IsPrimaryKey = true, IsIdentity = true, SkipOnInsert = true, SkipOnUpdate = true)] public int       Id                   { get; set; } // INTEGER
		[Column("bigintDataType"                                                                                        )] public long?     BigintDataType       { get; set; } // BIGINT
		[Column("smallintDataType"                                                                                      )] public short?    SmallintDataType     { get; set; } // SMALLINT
		[Column("decimalDataType"                                                                                       )] public decimal?  DecimalDataType      { get; set; } // DECIMAL(38, 10)
		[Column("decFloatDataType"                                                                                      )] public decimal?  DecFloatDataType     { get; set; } // DECIMAL
		[Column("smalldecimalDataType"                                                                                  )] public decimal?  SmalldecimalDataType { get; set; } // SMALLDECIMAL
		[Column("intDataType"                                                                                           )] public int?      IntDataType          { get; set; } // INTEGER
		[Column("tinyintDataType"                                                                                       )] public byte?     TinyintDataType      { get; set; } // TINYINT
		[Column("floatDataType"                                                                                         )] public double?   FloatDataType        { get; set; } // DOUBLE
		[Column("realDataType"                                                                                          )] public float?    RealDataType         { get; set; } // REAL
		[Column("dateDataType"                                                                                          )] public DateTime? DateDataType         { get; set; } // DATE
		[Column("timeDataType"                                                                                          )] public TimeSpan? TimeDataType         { get; set; } // TIME
		[Column("seconddateDataType"                                                                                    )] public DateTime? SeconddateDataType   { get; set; } // SECONDDATE
		[Column("timestampDataType"                                                                                     )] public DateTime? TimestampDataType    { get; set; } // TIMESTAMP
		[Column("charDataType"                                                                                          )] public char?     CharDataType         { get; set; } // CHAR(1)
		[Column("char20DataType"                                                                                        )] public string?   Char20DataType       { get; set; } // CHAR(20)
		[Column("varcharDataType"                                                                                       )] public string?   VarcharDataType      { get; set; } // VARCHAR(20)
		[Column("textDataType"                                                                                          )] public string?   TextDataType         { get; set; } // TEXT
		[Column("shorttextDataType"                                                                                     )] public string?   ShorttextDataType    { get; set; } // SHORTTEXT
		[Column("ncharDataType"                                                                                         )] public char?     NcharDataType        { get; set; } // NCHAR(1)
		[Column("nchar20DataType"                                                                                       )] public string?   Nchar20DataType      { get; set; } // NCHAR(20)
		[Column("nvarcharDataType"                                                                                      )] public string?   NvarcharDataType     { get; set; } // NVARCHAR(20)
		[Column("alphanumDataType"                                                                                      )] public string?   AlphanumDataType     { get; set; } // ALPHANUM
		[Column("binaryDataType"                                                                                        )] public byte[]?   BinaryDataType       { get; set; } // BINARY(10)
		[Column("varbinaryDataType"                                                                                     )] public byte[]?   VarbinaryDataType    { get; set; } // VARBINARY(10)
		[Column("blobDataType"                                                                                          )] public byte[]?   BlobDataType         { get; set; } // BLOB
		[Column("clobDataType"                                                                                          )] public string?   ClobDataType         { get; set; } // CLOB
		[Column("nclobDataType"                                                                                         )] public string?   NclobDataType        { get; set; } // NCLOB
	}
}
