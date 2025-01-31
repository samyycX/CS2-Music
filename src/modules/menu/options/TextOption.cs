using CounterStrikeSharp.API.Core;

namespace Music;

public class TextOption : MenuOption
{
  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {

  }

  public override bool Prev(CCSPlayerController player, WasdMyMenu menu)
  {
    return false;
  }
}

