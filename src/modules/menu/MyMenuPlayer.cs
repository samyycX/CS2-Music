using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Music;

public class MyMenuPlayer
{
  public required CCSPlayerController Player { get; set; }
  public Stack<WasdMyMenu> Menus { get; set; } = new();

  public InputOption? PendingInput { get; set; }

  public PlayerButtons Buttons { get; set; }

  public string CenterHtml { get; set; } = "";

  public void OpenMainMenu(WasdMyMenu menu)
  {
    Menus.Clear();
    Menus.Push(menu);
    Render();
  }

  public void CloseMenu()
  {
    if (PendingInput != null)
    {
      PendingInput = null;
      Render();
      return;
    }
    Menus.Clear();
  }

  public void OpenSubMenu(WasdMyMenu menu)
  {
    Menus.Push(menu);
    Render();
  }

  public void CloseSubMenu()
  {
    Menus.Pop();
    Render();
  }

  public void ScrollUp()
  {
    Menus.Peek().ScrollUp();
    Render();
  }

  public void ScrollDown()
  {
    Menus.Peek().ScrollDown();
    Render();
  }

  public void Next()
  {
    Menus.Peek().Next(Player);
    if (HasMenu()) Render();
    else CenterHtml = "";
  }

  public void Prev()
  {
    bool preventClose = Menus.Peek().Prev(Player);
    if (preventClose && HasMenu()) Render();
    if (!preventClose && Menus.Count > 1)
    {
      CloseSubMenu();
    }
  }
  public void ToTop()
  {
    Menus.Peek().ToTop();
    Render();
  }
  public void ToSelected()
  {
    Menus.Peek().ToSelected();
    Render();
  }

  public bool HasMenu()
  {
    return Menus.Count > 0;
  }

  public bool Input(string input)
  {
    PendingInput?.Input(Player, Menus.Peek(), input);
    return PendingInput != null;
  }

  public void Render()
  {
    if (PendingInput != null)
    {
      CenterHtml = PendingInput.WaitingScreen;
      return;
    }
    CenterHtml = Menus.Peek().Render();
  }

  public void Rerender()
  {
    Menus.ElementAt(Menus.Count - 1).Rerender(Player); // the root menu should contains all the path to submenus and eventually update them all
  }

}