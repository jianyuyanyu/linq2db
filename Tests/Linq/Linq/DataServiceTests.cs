﻿#if NETFRAMEWORK
using System.Data.Services.Providers;

using NUnit.Framework;

namespace Tests.Linq
{
	using Model;
	using LinqToDB.Remote;

	[TestFixture]
	public class DataServiceTests
	{
		[Test]
		public void Test1()
		{
			var ds = new DataService<NorthwindDB>();
			var mp = ds.GetService(typeof(IDataServiceMetadataProvider));
		}
	}
}
#endif
