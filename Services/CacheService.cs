using StackExchange.Redis;
using System.Text.Json;
namespace CahchingWebApiTest.Services;
public class CacheService : ICacheService
{
    private IDatabase _cacheDb;
    public CacheService()
    {
        var redis = ConnectionMultiplexer.Connect("localhost:6769");
        _cacheDb = redis.GetDatabase();
    }
    T ICacheService.GetData<T>(string key)
    {
        var value = _cacheDb.StringGet(key);
        if (!String.IsNullOrEmpty(value))

            return JsonSerializer.Deserialize<T>(value);

        return default;
    }

    object ICacheService.RemoveData(string key)
    {
        var _exist = _cacheDb.KeyExists(key);
        if (_exist)
            return _cacheDb.KeyDelete(key);

        return false;
    }

    bool ICacheService.SetData<T>(string key, T value, DateTimeOffset expirationTime)
    {
        var expireTime = expirationTime.DateTime.Subtract(DateTime.Now);
        var isSet = _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expireTime);
        return isSet;
    }
}