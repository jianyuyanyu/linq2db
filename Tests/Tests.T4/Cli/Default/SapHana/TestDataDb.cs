// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.Default.SapHana
{
	public partial class TestDataDB : DataConnection
	{
		public TestDataDB()
		{
			InitDataContext();
		}

		public TestDataDB(string configuration)
			: base(configuration)
		{
			InitDataContext();
		}

		public TestDataDB(DataOptions<TestDataDB> options)
			: base(options.Options)
		{
			InitDataContext();
		}

		partial void InitDataContext();

		public ITable<AllType>                   AllTypes                   => this.GetTable<AllType>();
		public ITable<AllTypesGeo>               AllTypesGeos               => this.GetTable<AllTypesGeo>();
		public ITable<BulkInsertLowerCaseColumn> BulkInsertLowerCaseColumns => this.GetTable<BulkInsertLowerCaseColumn>();
		public ITable<BulkInsertUpperCaseColumn> BulkInsertUpperCaseColumns => this.GetTable<BulkInsertUpperCaseColumn>();
		public ITable<Child>                     Children                   => this.GetTable<Child>();
		public ITable<CollatedTable>             CollatedTables             => this.GetTable<CollatedTable>();
		public ITable<Doctor>                    Doctors                    => this.GetTable<Doctor>();
		public ITable<GrandChild>                GrandChildren              => this.GetTable<GrandChild>();
		public ITable<IndexTable>                IndexTables                => this.GetTable<IndexTable>();
		public ITable<IndexTable2>               IndexTable2                => this.GetTable<IndexTable2>();
		public ITable<InheritanceChild>          InheritanceChildren        => this.GetTable<InheritanceChild>();
		public ITable<InheritanceParent>         InheritanceParents         => this.GetTable<InheritanceParent>();
		public ITable<LinqDataType>              LinqDataTypes              => this.GetTable<LinqDataType>();
		public ITable<Parent>                    Parents                    => this.GetTable<Parent>();
		public ITable<Patient>                   Patients                   => this.GetTable<Patient>();
		public ITable<Person>                    People                     => this.GetTable<Person>();
		public ITable<TestIdentity>              TestIdentities             => this.GetTable<TestIdentity>();
		public ITable<TestMerge1>                TestMerge1                 => this.GetTable<TestMerge1>();
		public ITable<TestMerge2>                TestMerge2                 => this.GetTable<TestMerge2>();
		public ITable<PrdGlobalEccCvMara>        PrdGlobalEccCvMaras        => this.GetTable<PrdGlobalEccCvMara>();
		public ITable<ParentChildView>           ParentChildViews           => this.GetTable<ParentChildView>();
		public ITable<ParentView>                ParentViews                => this.GetTable<ParentView>();
	}

	public static partial class ExtensionMethods
	{
		#region Table Extensions
		public static AllType? Find(this ITable<AllType> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<AllType?> FindAsync(this ITable<AllType> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static AllTypesGeo? Find(this ITable<AllTypesGeo> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<AllTypesGeo?> FindAsync(this ITable<AllTypesGeo> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Doctor? Find(this ITable<Doctor> table, int personId)
		{
			return table.FirstOrDefault(e => e.PersonId == personId);
		}

		public static Task<Doctor?> FindAsync(this ITable<Doctor> table, int personId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PersonId == personId, cancellationToken);
		}

		public static IndexTable? Find(this ITable<IndexTable> table, int pkField1, int pkField2)
		{
			return table.FirstOrDefault(e => e.PkField1 == pkField1 && e.PkField2 == pkField2);
		}

		public static Task<IndexTable?> FindAsync(this ITable<IndexTable> table, int pkField1, int pkField2, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PkField1 == pkField1 && e.PkField2 == pkField2, cancellationToken);
		}

		public static IndexTable2? Find(this ITable<IndexTable2> table, int pkField1, int pkField2)
		{
			return table.FirstOrDefault(e => e.PkField1 == pkField1 && e.PkField2 == pkField2);
		}

		public static Task<IndexTable2?> FindAsync(this ITable<IndexTable2> table, int pkField1, int pkField2, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PkField1 == pkField1 && e.PkField2 == pkField2, cancellationToken);
		}

		public static InheritanceChild? Find(this ITable<InheritanceChild> table, int inheritanceChildId)
		{
			return table.FirstOrDefault(e => e.InheritanceChildId == inheritanceChildId);
		}

		public static Task<InheritanceChild?> FindAsync(this ITable<InheritanceChild> table, int inheritanceChildId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.InheritanceChildId == inheritanceChildId, cancellationToken);
		}

		public static InheritanceParent? Find(this ITable<InheritanceParent> table, int inheritanceParentId)
		{
			return table.FirstOrDefault(e => e.InheritanceParentId == inheritanceParentId);
		}

		public static Task<InheritanceParent?> FindAsync(this ITable<InheritanceParent> table, int inheritanceParentId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.InheritanceParentId == inheritanceParentId, cancellationToken);
		}

		public static Patient? Find(this ITable<Patient> table, int personId)
		{
			return table.FirstOrDefault(e => e.PersonId == personId);
		}

		public static Task<Patient?> FindAsync(this ITable<Patient> table, int personId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PersonId == personId, cancellationToken);
		}

		public static Person? Find(this ITable<Person> table, int personId)
		{
			return table.FirstOrDefault(e => e.PersonId == personId);
		}

		public static Task<Person?> FindAsync(this ITable<Person> table, int personId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PersonId == personId, cancellationToken);
		}

		public static TestIdentity? Find(this ITable<TestIdentity> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<TestIdentity?> FindAsync(this ITable<TestIdentity> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static TestMerge1? Find(this ITable<TestMerge1> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<TestMerge1?> FindAsync(this ITable<TestMerge1> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static TestMerge2? Find(this ITable<TestMerge2> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<TestMerge2?> FindAsync(this ITable<TestMerge2> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static PrdGlobalEccCvMara? Find(this ITable<PrdGlobalEccCvMara> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<PrdGlobalEccCvMara?> FindAsync(this ITable<PrdGlobalEccCvMara> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}
		#endregion
	}
}
