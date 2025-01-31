using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Music;

public static class MyMenuManager
{
  public static readonly Dictionary<int, MyMenuPlayer> Players = new();

  public static void OpenMainMenu(CCSPlayerController player, WasdMyMenu menu)
  {
    Players[player.Slot].OpenMainMenu(menu);
  }

  public static void CloseMenu(CCSPlayerController player)
  {
    Players[player.Slot].CloseMenu();

  }

  public static void OpenSubMenu(CCSPlayerController player, WasdMyMenu menu)
  {
    Players[player.Slot].OpenSubMenu(menu);
  }

  public static void CloseSubMenu(CCSPlayerController player)
  {
    Players[player.Slot].CloseSubMenu();
  }

  public static void AddPlayer(int slot, MyMenuPlayer menuPlayer)
  {
    if (Players.ContainsKey(slot))
    {
      Players.Remove(slot);
    }
    Players.Add(slot, menuPlayer);
  }

  public static MyMenuPlayer GetPlayer(int slot)
  {
    return Players[slot];
  }

  public static void RemovePlayer(int slot)
  {
    Players.Remove(slot);
  }

  public static void ClearPlayer()
  {
    Players.Clear();
  }

  public static void ReloadPlayer()
  {
    foreach (var player in Utilities.GetPlayers())
    {
      Players[player.Slot] = new MyMenuPlayer
      {
        Player = player,
        Buttons = player.Buttons
      };
    }
  }

  public static void RerenderPlayer(int slot)
  {
    Players[slot].Rerender();
  }

  public static bool Input(int slot, string input)
  {
    return Players[slot].Input(input);
  }

  public static void Update()
  {
    foreach (var player in Players.Values.Where(player => player.HasMenu()))
    {
      if ((player.Buttons & PlayerButtons.Forward) == 0 && (player.Player.Buttons & PlayerButtons.Forward) != 0)
      {
        player.ScrollUp();
      }
      else if ((player.Buttons & PlayerButtons.Back) == 0 && (player.Player.Buttons & PlayerButtons.Back) != 0)
      {
        player.ScrollDown();
      }
      else if ((player.Buttons & PlayerButtons.Moveright) == 0 && (player.Player.Buttons & PlayerButtons.Moveright) != 0)
      {
        player.Next();
      }
      else if ((player.Buttons & PlayerButtons.Moveleft) == 0 && (player.Player.Buttons & PlayerButtons.Moveleft) != 0)
      {
        player.Prev();
      }

      if (((long)player.Player.Buttons & 8589934592) == 8589934592)
      {
        player.CloseMenu();
      }

      player.Buttons = player.Player.Buttons;
      if (player.CenterHtml != "")
        Server.NextFrame(() =>
        player.Player.PrintToCenterHtml(player.CenterHtml)
    );
    }
  }
}