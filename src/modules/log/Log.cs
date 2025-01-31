using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Music;

public static class Log
{
  private static ILogger? _Logger;

  private static IStringLocalizer? _Localizer;

  public static void Init(ILogger logger, IStringLocalizer localizer)
  {
    _Logger = logger;
    _Localizer = localizer;
  }

  private static void _Log(LogLevel level, string msg)
  {
    if (_Localizer == null || _Logger == null) return;
    _Logger?.Log(level, msg);
  }

  public static void LogInfo(string msg)
  {
    _Log(LogLevel.Information, msg);
  }

  public static void LogError(string msg)
  {
    _Log(LogLevel.Error, msg);
  }

  public static void LogWarning(string msg)
  {
    _Log(LogLevel.Warning, msg);
  }

  public static void LogDebug(string msg)
  {
    if (Config.GetConfig().Debug)
    {
      _Log(LogLevel.Debug, msg);
    }
  }

  public static void LogWithLang(string msg)
  {
    if (_Localizer != null) LogInfo(_Localizer[msg]);
  }

  public static void LogErrorWithLang(string msg)
  {
    if (_Localizer != null) LogError(_Localizer[msg]);
  }

  public static void LogWarningWithLang(string msg)
  {
    if (_Localizer != null) LogWarning(_Localizer[msg]);
  }

  public static void LogDebugWithLang(string msg)
  {
    if (_Localizer != null) LogDebug(_Localizer[msg]);
  }
}