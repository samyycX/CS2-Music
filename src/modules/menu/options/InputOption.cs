using CounterStrikeSharp.API.Core;
namespace Music;

public class InputOption : MenuOption
{
  public string WaitingScreen { get; set; } = "";

  public string InputHint { get; set; } = "";

  public Action<CCSPlayerController, WasdMyMenu, string> OnInput { get; set; } = (player, menu, input) => { };

  public override void Next(CCSPlayerController player, WasdMyMenu menu)
  {
    MyMenuManager.GetPlayer(player.Slot).PendingInput = this;
    player.PrintToChat(InputHint);
  }

  public void Input(CCSPlayerController player, WasdMyMenu menu, string input)
  {
    OnInput(player, menu, input);
  }

  public override bool Prev(CCSPlayerController player, WasdMyMenu menu)
  {
    return false;
  }
}