using MusicAPI;

namespace Music;


public record PlayingSong
{
  public required Song Song { get; set; }
  public SongResource? SongResource { get; set; }
  public List<LyricLine>? Lyric { get; set; }
  public List<LyricLine>? TranslatedLyric { get; set; }
  public float Progress { get; set; }

  public int LastLyric { get; set; } = -1;
  public int LastTranslatedLyric { get; set; } = -1;

  public ulong PlayerSteamId { get; set; }
}

public record LyricLine
{
  public required long Time { get; set; }
  public required string Content { get; set; }
}