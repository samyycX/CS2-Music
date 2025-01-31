using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.DependencyInjection;
using MusicAPI;

namespace Music;

public class Music : BasePlugin
{
  public override string ModuleName => "Music";
  public override string ModuleVersion => "1.0.0";
  public override string ModuleAuthor => "samyyc";

  public override string ModuleDescription => "A music player plugin.";

  public static Music Instance { get; private set; } = null!;


  public override void Load(bool hotReload)
  {
    Instance = this;
    Log.Init(Logger, Localizer);
    Config.Init(ModuleDirectory);
    MusicWebAPI.Init(Config.GetConfig().MusicApi.CloudMusic.Cookie);
    PlayManager.Init();

    RegisterListener<Listeners.OnMapStart>(OnMapStart);
    RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
    RegisterListener<Listeners.OnClientPutInServer>(OnPlayerConnected);
    RegisterListener<Listeners.OnClientDisconnect>(OnPlayerDisconnect);
    RegisterListener<Listeners.OnTick>(OnTick);
    RegisterListener<Listeners.CheckTransmit>(CheckTransmit);
    RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

    HookUserMessage(118, (msg) =>
    {
      var player = Utilities.GetPlayerFromIndex(msg.ReadInt("entityindex"));
      if (player == null) return HookResult.Continue;
      if (MyMenuManager.Input(player.Slot, msg.ReadString("param2")))
      {
        return HookResult.Stop;
      }
      return HookResult.Continue;
    }, HookMode.Pre);


    if (hotReload)
    {
      HudLyricManager.Recollect();
      MyMenuManager.ReloadPlayer();
    }
  }

  public void OnMapStart(string map)
  {
    HudLyricManager.Reload();
  }

  public void OnMapEnd()
  {
    MyMenuManager.ClearPlayer();
  }

  public void CheckTransmit(CCheckTransmitInfoList infoList)
  {
    HudLyricManager.CheckTransmit(infoList);
  }

  public void OnPlayerConnected(int slot)
  {
    MyMenuManager.AddPlayer(slot, new MyMenuPlayer { Player = Utilities.GetPlayerFromSlot(slot)!, Buttons = 0 });
  }

  public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
  {
    HudLyricManager.InitPlayer(@event.Userid!.Slot);
    return HookResult.Continue;
  }

  public void OnPlayerDisconnect(int slot)
  {
    HudLyricManager.RemovePlayer(slot);
    MyMenuManager.RemovePlayer(slot);
  }

  public void OnTick()
  {
    HudLyricManager.Update();
    MyMenuManager.Update();
  }

  public override void Unload(bool hotReload)
  {
    PlayManager.Unload();
  }

  [ConsoleCommand("css_music")]
  public void OnMusicCommand(CCSPlayerController player, CommandInfo info)
  {
    MyMenuManager.OpenMainMenu(player, MusicMenuManager.GetMainMenu(player));
  }
}