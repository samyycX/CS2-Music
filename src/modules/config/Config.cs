using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace Music;

public static class Config
{
  private static ISerializer _Serializer = new SerializerBuilder().Build();
  private static IDeserializer _Deserializer = new DeserializerBuilder().Build();

  private static string _ModuleDirectory;

  private static ConfigModel _Config;
  public static void Init(string directory)
  {
    _ModuleDirectory = directory;
    Log.LogWithLang("log.config.loading");
    var path = Path.Combine(_ModuleDirectory, "../../configs/plugins/Music");
    if (!Directory.Exists(path))
    {
      Directory.CreateDirectory(path);
    }
    path = Path.Combine(path, "config.yml");
    if (!File.Exists(path))
    {
      var defaultConfig = _Serializer.Serialize(new ConfigModel());
      File.WriteAllText(path, defaultConfig);
    }
    _Config = _Deserializer.Deserialize<ConfigModel>(File.ReadAllText(path));
    var serializedConfig = _Serializer.Serialize(_Config);
    File.WriteAllText(path, serializedConfig);
    Log.LogWithLang("log.config.loaded");
  }

  public static void Reload()
  {
    var path = Path.Combine(_ModuleDirectory, "../../configs/plugins/Music/config.yml");
    _Config = _Deserializer.Deserialize<ConfigModel>(File.ReadAllText(path));
  }

  public static ConfigModel GetConfig()
  {
    return _Config;
  }

}