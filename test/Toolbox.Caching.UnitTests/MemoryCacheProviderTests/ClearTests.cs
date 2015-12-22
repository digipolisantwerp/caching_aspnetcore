using System.Collections.Generic;
using Toolbox.Caching;
using Xunit;

namespace Toolbox.Caching.UnitTests.MemoryCacheProviderTests
{
	public class ClearTests
    {
        [Fact]
        public void SingleObjectIsClearedFromCache()
        {
			using (var cacheProvider = new MemoryCacheProvider())
			{
				cacheProvider.Add<string>("Wijk", "Wijk", true);
				cacheProvider.Clear<string>("Wijk");

				Assert.Null(cacheProvider.Get<string>("Wijk"));
			}
        }

		[Fact]
		public void EmptyCacheIsCleared()
		{
			using (var cacheProvider = new MemoryCacheProvider())
			{
				cacheProvider.Clear<string>("Wijk");
				Assert.Null(cacheProvider.Get<string>("Wijk"));
			}
        }

        [Fact]
        public void ListIsClearedFromCache()
        {
			using (var cacheProvider = new MemoryCacheProvider())
			{

				cacheProvider.Add<List<string>>("Wijken", new List<string>(), true);
				cacheProvider.Clear<List<string>>("Wijken");

				Assert.Null(cacheProvider.Get<List<string>>("Wijken"));
			}
        }

        [Fact]
        public void ListIsClearedFromEmptyCache()
        {
			using (var cacheProvider = new MemoryCacheProvider())
			{

				cacheProvider.Clear<List<string>>("Wijken");

				Assert.Null(cacheProvider.Get<List<string>>("Wijken"));
			}
        }
    }
}