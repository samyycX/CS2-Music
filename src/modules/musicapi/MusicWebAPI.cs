using CounterStrikeSharp.API.Core;
using MusicAPI;
namespace Music;

public enum Platform
{
  Netease,
}

public static class MusicWebAPI
{
  private static NeteaseAPI NeteaseAPI = new();

  public static void Init(string cookie)
  {
    NeteaseAPI.Headers.Add("Cookie", $"MUSIC_U={cookie}");
  }

  public static async Task<List<Song>> Search(Platform platform, string keyword, int limit, int page)
  {
    switch (platform)
    {
      case Platform.Netease:
        return await NeteaseAPI.Search(keyword, limit: limit, page: page) ?? new List<Song>();
    }
    return new List<Song>();
  }

  public static async Task<Lyric?> GetLyric(Platform platform, string lyricId)
  {
    switch (platform)
    {
      case Platform.Netease:
        return await NeteaseAPI.GetLyric(lyricId);
    }
    return null;
  }

  public static async Task<SongResource?> GetSongResource(Platform platform, string id)
  {
    switch (platform)
    {
      case Platform.Netease:
        return await NeteaseAPI.GetSongResource(id);
    }
    return null;
  }

}