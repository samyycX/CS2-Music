using CounterStrikeSharp.API.Core.Capabilities;
using StoreApi;

namespace Music;

public static class StoreApiManager
{
    public static PluginCapability<IStoreApi?> StoreApi { get; private set; }

    private static IStoreApi? _StoreApi;
    public static void Init()
    {
        try
        {
            StoreApi = new("store:api");
            _StoreApi = StoreApi.Get();
        } catch (Exception)
    {
            Log.LogWarning("未找到 Store 插件，已关闭相关功能，不影响正常使用。");
        }
    }

    public static bool IsStoreApiAvailable()
    {
        return _StoreApi != null;
    }

    public static IStoreApi GetStoreApi()
    {
        if (_StoreApi == null)
        {
            throw new InvalidOperationException("StoreApi 未初始化");
        }
        return _StoreApi;
    }
}
