namespace Digipolis.Caching.Handlers
{
    internal class CacheHandlerWithOptions
    {
        public ICacheHandler CacheHandler { get; set; }
        public int MinutesToCache { get; set; }
        public int MilliSecondsBeforeTimeout { get; set; }
    }
}
