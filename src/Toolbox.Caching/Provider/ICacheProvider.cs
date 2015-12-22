namespace Toolbox.Caching
{
	public interface ICacheProvider
    {
        void Add<T>(string key, T value, bool overwrite = false);
        TEntity Get<TEntity>(string key);
        void Clear<TEntity>(string key);
    }
}
