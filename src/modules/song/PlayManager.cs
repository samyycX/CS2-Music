using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using MusicAPI;

namespace Music;
public static class PlayManager
{

  private static Queue<PlayingSong> PlayQueue = new();
  private static PlayingSong? CurrentSong;

  public static void Init()
  {
    Audio.RegisterPlayStartListener(PlayStartListener);
    Audio.RegisterPlayEndListener(PlayEndListener);
    Audio.RegisterPlayListener(PlayListener);
  }

  public static void Unload()
  {
    Audio.UnregisterPlayStartListener(PlayStartListener);
    Audio.UnregisterPlayEndListener(PlayEndListener);
    Audio.UnregisterPlayListener(PlayListener);
  }

  private static List<LyricLine> ResolveLyric(string lyric)
  {
    List<LyricLine> lyrics = new();
    var lines = lyric.Split('\n');
    foreach (var line in lines)
    {
      try
      {
        var parts = line.Split(']', 2);
        var timeStr = parts[0].Substring(1);
        // time is 00:00.000
        var minute = int.Parse(timeStr.Split(":")[0]);
        var second = int.Parse(timeStr.Split(":")[1].Split(".")[0]);
        var millisecond = int.Parse(timeStr.Split(":")[1].Split(".")[1]);
        var time = minute * 60 * 1000 + second * 1000 + millisecond;
        var text = parts[1];
        lyrics.Add(new LyricLine { Time = time, Content = text });
      }
      catch (Exception e)
      {
        continue;
      }
    }
    return lyrics;
  }

  public static bool IsPlaying(CCSPlayerController player)
  {
    return CurrentSong?.PlayerSteamId == player.SteamID;
  }

  public static bool IsInQueue(CCSPlayerController player)
  {
    return PlayQueue.Any(song => song.PlayerSteamId == player.SteamID);
  }

  public static void RemoveFromQueue(CCSPlayerController player)
  {
    PlayQueue = new(PlayQueue.Where(song => song.PlayerSteamId != player.SteamID));
  }

  public static Song? GetRequestedSong(CCSPlayerController player)
  {
    if (IsPlaying(player)) return CurrentSong?.Song;
    return PlayQueue.FirstOrDefault(song => song.PlayerSteamId == player.SteamID)?.Song;
  }

  public static int AddToQueue(CCSPlayerController player, Song song)
  {
    PlayingSong playingSong = new()
    {
      Song = song,
      PlayerSteamId = player.SteamID
    };
    if (CurrentSong == null && PlayQueue.Count == 0)
    {
      CurrentSong = playingSong;
      Server.PrintToChatAll(Music.Instance.Localizer["msg.playnext", CurrentSong.Song.Display()]);
      Play();
      return 0;
    }
    PlayQueue.Enqueue(playingSong);
    return PlayQueue.Count;
  }

  private static async void Play()
  {
    if (CurrentSong == null) return;
    try
    {
      CurrentSong.SongResource = await MusicWebAPI.GetSongResource(Platform.Netease, CurrentSong.Song.Id);
      var lyric = await MusicWebAPI.GetLyric(Platform.Netease, CurrentSong.Song.LyricId);
      if (lyric != null && lyric.HasTimeInfo)
      {
        CurrentSong.Lyric = ResolveLyric(lyric.OriginalLyric);
        if (!string.IsNullOrEmpty(lyric.TranslatedLyric))
        {
          CurrentSong.TranslatedLyric = ResolveLyric(lyric.TranslatedLyric);
        }
      }
      // read from url
      using var httpClient = new HttpClient();
      if (CurrentSong.SongResource == null || string.IsNullOrEmpty(CurrentSong.SongResource.Url))
      {
        Server.NextFrame(() =>
        {
          if (Utilities.GetPlayerFromSteamId(CurrentSong.PlayerSteamId) != null)
          {
            Music.RefundPlayer(Utilities.GetPlayerFromSteamId(CurrentSong.PlayerSteamId)!);
            Utilities.GetPlayerFromSteamId(CurrentSong.PlayerSteamId)!.PrintToChat(Music.Instance.Localizer["msg.playfailed", CurrentSong.Song.Display()]);
            CurrentSong = null;
            PlayNext();
            
          }
        });
        return;
      }
      var buffer = await httpClient.GetByteArrayAsync(CurrentSong.SongResource!.Url);
      Audio.PlayFromBuffer(buffer, Music.Instance.Config.General.Volume);
      Server.NextFrame(() =>
      {
        Server.PrintToChatAll(Music.Instance.Localizer["msg.playstart", CurrentSong.Song.Display()]);
      });
    }
    catch (Exception e)
    {
      Log.LogError(e.Message);
      Server.NextFrame(() =>
      {
        Music.RefundPlayer(Utilities.GetPlayerFromSteamId(CurrentSong.PlayerSteamId)!);
        Server.PrintToChatAll(Music.Instance.Localizer["msg.playfailed", CurrentSong.Song.Display()]);
        CurrentSong = null;
        PlayNext();
      });
      return;
    }

  }

  public static void PrintQueue(CCSPlayerController player)
  {
    if (PlayQueue.Count == 0 && CurrentSong == null)
    {
      player.PrintToChat(Music.Instance.Localizer["msg.queueinfonone"]);
    }
    else
    {
      player.PrintToChat(Music.Instance.Localizer["msg.queueinfostart"]);
      for (int i = 0; i < PlayQueue.Count; i++)
      {
        player.PrintToChat(Music.Instance.Localizer["msg.queueinfo", i + 1, PlayQueue.ElementAt(i).Song.Display()]);
      }
      if (CurrentSong != null)
      {
        player.PrintToChat(Music.Instance.Localizer["msg.queueinfoplaying", CurrentSong.Song.Display()]);
      }
    }
  }



  public static void PlayStartListener(int slot)
  {
    Server.NextFrame(() =>
    {
      if (Music.Instance.Config.General.Debug)
      {
        Server.PrintToChatAll("Audio::PlayStart");
      }
    });
  }

  public static void PlayNext()
  {
    if (PlayQueue.Count > 0)
    {
      PlayingSong playingSong = PlayQueue.Dequeue();
      Server.PrintToChatAll(Music.Instance.Localizer["msg.playnext", playingSong.Song.Display()]);
      CurrentSong = playingSong;
      Music.Instance.AddTimer(2f, () =>
      {
        Server.NextFrame(() =>
        {
          Play();
        });
      });
    }
  }

  public static void PlayEndListener(int slot)
  {
    Server.NextFrame(() =>
    {
      if (Music.Instance.Config.General.Debug)
      {
        Server.PrintToChatAll("Audio::PlayEnd");
      }
      HudLyricManager.UpdateOriginal("");
      HudLyricManager.UpdateTranslation("");
      CurrentSong = null;
      PlayNext();
    });
  }

  public static void PlayListener(int slot)
  {
    if (slot != -1) return;
    Server.NextFrame(() =>
    {
      if (CurrentSong == null) return;
      if (CurrentSong.Lyric == null) return;
      var nextLine = CurrentSong.LastLyric == CurrentSong.Lyric.Count() - 1 ? null : CurrentSong.Lyric[CurrentSong.LastLyric + 1];
      if (nextLine == null) return;
      if (CurrentSong.Progress > nextLine.Time)
      {
        CurrentSong.LastLyric = CurrentSong.LastLyric + 1;
        // Utilities.GetPlayers().ForEach(player =>
        // {
        //   player.PrintToHint(line.Content, (float)fullTime / 1000 + 2);
        // });
      }
      var line = CurrentSong.Lyric[CurrentSong.LastLyric];
      float fullTime = (float)(CurrentSong.LastLyric == CurrentSong.Lyric.Count() - 1 ? CurrentSong.SongResource.Duration : CurrentSong.Lyric[CurrentSong.LastLyric + 1].Time) - line.Time;
      float progress = (CurrentSong.Progress - line.Time) / fullTime;
      HudLyricManager.UpdateOriginal(line.Content);

      if (CurrentSong.TranslatedLyric == null) return;
      var nextTranslatedLine = CurrentSong.LastTranslatedLyric == CurrentSong.TranslatedLyric.Count() - 1 ? null : CurrentSong.TranslatedLyric[CurrentSong.LastTranslatedLyric + 1];
      if (nextTranslatedLine == null) return;
      if (CurrentSong.Progress > nextTranslatedLine.Time)
      {
        CurrentSong.LastTranslatedLyric = CurrentSong.LastTranslatedLyric + 1;
        // Utilities.GetPlayers().ForEach(player =>
        // {
        //   player.PrintToHint(line.Content, (float)fullTime / 1000 + 2);
        // });
        // var fullTime = (isLast ? CurrentSong.SongResource.Duration : CurrentSong.Lyric[i + 1].Time) - line.Time;
        // var lineProgress = (progress - line.Time) / fullTime;
        // HudLyricManager.Update(line.Content, CurrentSong.TranslatedLyric != null ? CurrentSong.TranslatedLyric[i].Content : "", (float)lineProgress);
      }
      if (CurrentSong.LastTranslatedLyric == -1) return;
      var translatedLine = CurrentSong.TranslatedLyric[CurrentSong.LastTranslatedLyric];
      HudLyricManager.UpdateTranslation(translatedLine.Content);
    });
    if (CurrentSong != null)
    {
      CurrentSong.Progress += Music.Instance.Config.General.LyricInterval;
    }
  }

  public static void Stop(bool clear)
  {
    if (clear)
    {
      PlayQueue.Clear();
    }
    Audio.StopAllPlaying();
  }

  public static bool IsPlayerHearing(CCSPlayerController player)
  {
    return Audio.IsHearing(player.Slot);
  }

  public static void TogglePlayerHearing(CCSPlayerController player)
  {
    Audio.SetPlayerHearing(player.Slot, !Audio.IsHearing(player.Slot));
  }

  public static void SetAllPlayerHearing(bool hearing)
  {
    Audio.SetAllPlayerHearing(hearing);
  }

  public static string Display(this Song song)
  {
    return $"{song.Name} - {string.Join(",", song.Artists.Take(3).ToArray())}";
  }
}