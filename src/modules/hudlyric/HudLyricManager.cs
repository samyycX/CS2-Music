using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace Music;

public class HudLyric
{
  public int Slot { get; set; }
  private bool Displaying = true;
  public CPointWorldText? Original { get; set; }
  public CPointWorldText? Translation { get; set; }

  public void EnsureOriginalWorldtext(Action action)
  {
    if (Original == null || !Original.IsValid)
    {
      if (HudLyricManager.UpdatingOriginalPlayers.Contains(Slot))
      {
        return;
      }
      HudLyricManager.UpdatingOriginalPlayers.Add(Slot);
      Music.Instance.AddTimer(0.1f, () =>
      {
        Server.NextFrame(() =>
        {
          Original = HudLyricManager.CreateWorldText(Slot, $"hudlyric_{Slot}_original", "", 50, "", -3.3f);
          Server.NextFrame(action);
          HudLyricManager.UpdatingOriginalPlayers.Remove(Slot);
        });
      });
    }
    else
    {
      action();
    }
  }

  public void EnsureTranslationWorldtext(Action action)
  {
    if (Translation == null || !Translation.IsValid)
    {
      if (HudLyricManager.UpdatingTranslationPlayers.Contains(Slot))
      {
        return;
      }
      HudLyricManager.UpdatingTranslationPlayers.Add(Slot);
      Music.Instance.AddTimer(0.1f, () =>
      {
        Server.NextFrame(() =>
        {
          Translation = HudLyricManager.CreateWorldText(Slot, $"hudlyric_{Slot}_translation", "", 50, "", -3.9f);
          Server.NextFrame(action);
          HudLyricManager.UpdatingTranslationPlayers.Remove(Slot);
        });
      });
    }
    else
    {
      action();
    }
  }

  public void UpdateOriginal(string line)
  {
    if (!Displaying) return;
    EnsureOriginalWorldtext(() =>
    {
      Original!.MessageText = line;
      Utilities.SetStateChanged(Original, "CPointWorldText", "m_messageText");
    });
  }

  public void UpdateTranslation(string line)
  {
    if (!Displaying) return;
    EnsureTranslationWorldtext(() =>
    {
      Translation!.MessageText = line;
      Utilities.SetStateChanged(Translation, "CPointWorldText", "m_messageText");
    });
  }

  public void UpdateColor(Color color)
  {
    EnsureOriginalWorldtext(() =>
    {
      Original!.Color = color;
      Utilities.SetStateChanged(Original, "CPointWorldText", "m_Color");
    });
    EnsureTranslationWorldtext(() =>
    {
      Translation!.Color = color;
      Utilities.SetStateChanged(Translation, "CPointWorldText", "m_Color");
    });
  }

  public bool IsDisplaying()
  {
    return Displaying;
  }

  public void SetDisplaying(bool displaying)
  {
    if (!displaying)
    {
      UpdateOriginal("");
      UpdateTranslation("");
    }
    Displaying = displaying;
  }

  public void Remove()
  {
    if (Original != null && Original.IsValid) Original.Remove();
    if (Translation != null && Translation.IsValid) Translation.Remove();
  }

}

public static class HudLyricManager
{
  private static Dictionary<int, HudLyric> _HudLyrics = new();

  public static List<int> UpdatingOriginalPlayers = new();
  public static List<int> UpdatingTranslationPlayers = new();
  public static CPointWorldText CreateWorldText(int slot, string id, string text, int size = 100, string font = "Microsoft YaHei", float shiftY = -3)
  {
    CCSPlayerPawn pawn = Utilities.GetPlayerFromSlot(slot)?.PlayerPawn.Value!;

    var handle = new CHandle<CCSGOViewModel>((IntPtr)(pawn.ViewModelServices!.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel") + 4));
    if (!handle.IsValid)
    {
      CCSGOViewModel viewmodel = Utilities.CreateEntityByName<CCSGOViewModel>("predicted_viewmodel")!;
      viewmodel.DispatchSpawn();
      handle.Raw = viewmodel.EntityHandle.Raw;
      Utilities.SetStateChanged(pawn, "CCSPlayerPawnBase", "m_pViewModelServices");
    }

    CPointWorldText worldText = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext")!;
    worldText.MessageText = text;
    worldText.Enabled = true;
    worldText.FontSize = size;
    worldText.Fullbright = true;
    worldText.WorldUnitsPerPx = 0.01f;
    worldText.FontName = font;
    worldText.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER;
    worldText.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
    worldText.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
    worldText.PrivateVScripts = id;
    worldText.CRenderComponent!.IsRenderingWithViewModels = true;

    QAngle eyeAngles = pawn.EyeAngles;
    Vector forward = new(), right = new(), up = new();
    NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

    Vector offset = new();
    offset += forward * 7;
    offset += right * 0;
    offset += up * shiftY;

    QAngle angles = new()
    {
      Y = eyeAngles.Y + 270,
      Z = 90 - eyeAngles.X,
      X = 0
    };

    worldText.DispatchSpawn();
    worldText.Teleport(pawn.AbsOrigin! + offset + new Vector(0, 0, pawn.ViewOffset.Z), angles, null);
    Server.NextFrame(() =>
    {
      worldText.AcceptInput("SetParent", handle.Value, null, "!activator");
      Utilities.SetStateChanged(worldText, "CBaseModelEntity", "m_CRenderComponent");

    });

    return worldText;
  }
  public static void LyricInit(int slot)
  {
    HudLyric lyric = new HudLyric { Slot = slot };
    lyric.EnsureOriginalWorldtext(() => { });
    lyric.EnsureTranslationWorldtext(() => { });
    _HudLyrics[slot] = lyric;
  }

  public static void Recollect()
  {
    foreach (var worldtext in Utilities.FindAllEntitiesByDesignerName<CPointWorldText>("point_worldtext"))
    {
      if (!string.IsNullOrEmpty(worldtext.PrivateVScripts))
      {
        try
        {

          var slot = int.Parse(worldtext.PrivateVScripts.Split('_')[1]);
          var type = worldtext.PrivateVScripts.Split('_')[2];
          if (!_HudLyrics.ContainsKey(slot))
          {
            _HudLyrics[slot] = new HudLyric { Slot = slot };
          }
          if (type == "original")
          {
            _HudLyrics[slot].Original = worldtext;
          }
          else if (type == "translation")
          {
            _HudLyrics[slot].Translation = worldtext;
          }
        }
        catch (Exception e)
        {
          continue;
        }
      }
    }

    foreach (var lyric in _HudLyrics.Values)
    {
      lyric.EnsureOriginalWorldtext(() => { });
      lyric.EnsureTranslationWorldtext(() => { });
    }
  }

  public static void CheckTransmit(CCheckTransmitInfoList infoList)
  {
    var allWorldtexts = _HudLyrics.Values.Select(lyric => lyric.Original).Concat(_HudLyrics.Values.Select(lyric => lyric.Translation)).ToList().Select(worldtext => worldtext?.Index ?? uint.MaxValue).ToArray();
    foreach (var (info, player) in infoList)
    {
      if (player == null) continue;
      var hasLyric = _HudLyrics.TryGetValue(player.Slot, out var lyric);
      for (int i = 0; i < allWorldtexts.Length; i++)
      {
        if (allWorldtexts[i] == uint.MaxValue) continue;
        if (!hasLyric || (lyric!.Original?.Index != allWorldtexts[i] && lyric!.Translation?.Index != allWorldtexts[i]))
        {
          info.TransmitAlways.Remove(allWorldtexts[i]);
          info.TransmitEntities.Remove(allWorldtexts[i]);
        }
      }
    }
  }

  public static void Reload()
  {
    _HudLyrics = new();
  }

  public static void Update()
  {
    var speed = 0.2f;
    var hue = Math.Abs(Server.TickCount * speed % 720 - 360);
    var (r, g, b) = HslToRgb(hue, 1, 0.5);
    var color = Color.FromArgb(r, g, b);
    foreach (var lyric in _HudLyrics.Values)
    {
      lyric.UpdateColor(color);
    }
  }

  public static void InitPlayer(int slot)
  {
    if (_HudLyrics.ContainsKey(slot)) return;
    LyricInit(slot);
  }

  public static void RemovePlayer(int slot)
  {
    if (!_HudLyrics.ContainsKey(slot)) return;
    _HudLyrics[slot].Remove();
    _HudLyrics.Remove(slot);
  }

  public static void ToggleDisplaying(int slot)
  {
    if (!_HudLyrics.ContainsKey(slot)) return;
    _HudLyrics[slot].SetDisplaying(!_HudLyrics[slot].IsDisplaying());
  }

  public static bool IsDisplaying(int slot)
  {
    if (!_HudLyrics.ContainsKey(slot)) return false;
    return _HudLyrics[slot].IsDisplaying();
  }

  public static void UpdateOriginal(string line)
  {
    foreach (var lyric in _HudLyrics.Values)
    {
      lyric.UpdateOriginal(line);
    }
  }

  public static void UpdateTranslation(string line)
  {
    foreach (var lyric in _HudLyrics.Values)
    {
      lyric.UpdateTranslation(line);
    }
  }

  static (int, int, int) HslToRgb(double h, double s, double l)
  {
    double c = (1 - Math.Abs(2 * l - 1)) * s;
    double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
    double m = l - c / 2;

    double r, g, b;

    if (0 <= h && h < 60)
    {
      r = c; g = x; b = 0;
    }
    else if (60 <= h && h < 120)
    {
      r = x; g = c; b = 0;
    }
    else if (120 <= h && h < 180)
    {
      r = 0; g = c; b = x;
    }
    else if (180 <= h && h < 240)
    {
      r = 0; g = x; b = c;
    }
    else if (240 <= h && h < 300)
    {
      r = x; g = 0; b = c;
    }
    else
    {
      r = c; g = 0; b = x;
    }

    r = (r + m) * 255;
    g = (g + m) * 255;
    b = (b + m) * 255;

    return ((int)r, (int)g, (int)b);
  }

  public static void PrintToHint(this CCSPlayerController player, string message, float time)
  {
    if (string.IsNullOrEmpty(message)) return;
    time = time > 0 ? time : 1;
    var hint = Utilities.CreateEntityByName<CEnvInstructorHint>("env_instructor_hint")!;
    hint.Target = player.Index.ToString();
    hint.HintTargetEntity = player.Index.ToString();
    hint.Static = false;
    hint.Timeout = (int)time;
    hint.Caption = message + "       ";
    hint.Binding = "use_binding";
    hint.NoOffscreen = true;
    hint.DispatchSpawn();
    hint.AcceptInput("ShowHint", player, null, "!activator");

    Music.Instance.AddTimer(time, () =>
      {
        if (hint.IsValid)
        {
          hint.AcceptInput("Kill");
        }
      }, TimerFlags.STOP_ON_MAPCHANGE);
  }
}