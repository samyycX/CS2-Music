using CounterStrikeSharp.API.Core;
using Tomlyn.Model;

namespace Music;

public class MusicApi
{
  public string NeteaseMusicCookie { get; set; } = "";
}

public class General
{
  public string AdminFlag { get; set; } = "@css/admin";
  public float Volume { get; set; } = 0.8f;
  public float LyricInterval { get; set; } = 29.8f;
  public int Price { get; set; } = 100;
  public float RefundRate { get; set; } = 1f;
  public bool Debug { get; set; } = false;

};

public class MusicConfig : BasePluginConfig
{
  public General General { get; set; } = new();
  public MusicApi MusicApi { get; set; } = new();
}