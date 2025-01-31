using CounterStrikeSharp.API.Core;
namespace Music;

public class SubMenuOption : MenuOption
{
  public required WasdMyMenu NextMenu { get; set; }
  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {
    MyMenuManager.OpenSubMenu(player, NextMenu);
  }

  public override bool Prev(CCSPlayerController player, WasdMyMenu menu)
  {
    return false;
  }

  public override void Rerender(CCSPlayerController player, WasdMyMenu menu)
  {
    NextMenu.Rerender(player);
  }
}