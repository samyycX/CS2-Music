using YamlDotNet.Serialization;

namespace Music;

public record struct CloudMusicApiConfig()
{
  [YamlMember(Alias = "cookie")]
  public string Cookie = "";
}

public record struct MusicApiConfig()
{
  [YamlMember(Alias = "cloudmusic")]
  public CloudMusicApiConfig CloudMusic = new();
}

public record struct ConfigModel()
{
  [YamlMember(Alias = "musicapi")]
  public MusicApiConfig MusicApi = new();

  [YamlMember(Alias = "adminflag")]
  public string AdminFlag = "@css/admin";

  [YamlMember(Alias = "volume")]
  public float Volume = 0.8f;

  [YamlMember(Alias = "lyricinterval")]
  public float LyricInterval = 39.8f;

  [YamlMember(Alias = "debug")]
  public bool Debug = false;
};