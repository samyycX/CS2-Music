using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Menu;

namespace Music;

public static class MusicMenuManager
{

  public static WasdMyMenu GetMainMenu(CCSPlayerController player)
  {
    var menu = new WasdMyMenu { Title = "Music" };
    var price = StoreApiManager.IsStoreApiAvailable() ? Music.Instance.Config.General.Price : 0;
    if (PlayManager.IsInQueue(player) || PlayManager.IsPlaying(player))
    {
      menu.AddOption(new TextOption
      {
        Text = $"您已点歌: {PlayManager.GetRequestedSong(player)?.Name}",
      });
      if (PlayManager.IsInQueue(player))
      {
        menu.AddOption(new SelectOption
        {
          Text = "取消点歌",
          Select = (player, option, menu) =>
          {
            PlayManager.RemoveFromQueue(player);
            player.PrintToChat(Music.Instance.Localizer["msg.playremovequeue"]);
            Music.RefundPlayer(player);
            MyMenuManager.CloseMenu(player);
          }
        });
      }
    }
    else
    {
      menu.AddOption(new InputOption
      {
        Text = $"点歌 {(price > 0 ? $"[{price}积分]" : "")}",
        WaitingScreen = "请在聊天框输入歌名... (按Tab取消)",
        InputHint = Music.Instance.Localizer["msg.inputhint"],
        Disabled = StoreApiManager.IsStoreApiAvailable() && StoreApiManager.GetStoreApi().GetPlayerCredits(player) < price,
        OnInput = (player, menu, input) =>
        {
          var thread = new Thread(() =>
          {
            Task.Run(async () =>
            {
              var songs = await MusicWebAPI.Search(Platform.Netease, input, 50, 1);
              var submenu = new WasdMyMenu { Title = "结果" };
              foreach (var song in songs)
              {
                var confirm = new WasdMyMenu { Title = song.Display() };
                confirm.AddOption(new TextOption { Text = $"<font color='#9ee1f0'>{song.Name}</font>" });
                confirm.AddOption(new TextOption { Text = $"<font color='#9ee1f0'>作者</font> {string.Join(", ", song.Artists.Take(3).ToArray())}" });
                confirm.AddOption(new TextOption { Text = $"<font color='#9ee1f0'>专辑</font> {song.AlbumName}" });
                confirm.AddOption(new TextOption { Text = $"" });

                confirm.AddOption(new SelectOption
                {
                  Text = "确认",
                  Select = (player, option, menu) =>
                  {
                    if (StoreApiManager.IsStoreApiAvailable())
                    {
                      var storeApi = StoreApiManager.GetStoreApi();
                      if (storeApi.GetPlayerCredits(player) >= Music.Instance.Config.General.Price)
                      {
                        storeApi.SetPlayerCredits(player, storeApi.GetPlayerCredits(player) - Music.Instance.Config.General.Price);
                      } else {
                        player.PrintToChat(Music.Instance.Localizer["msg.insufficientcredit"]);
                        return;
                      }
                    }
                    int position = PlayManager.AddToQueue(player, song);
                    if (position != 0)
                    {
                      player.PrintToChat(Music.Instance.Localizer["msg.playqueued", position]);
                    }
                    player.PrintToChat(Music.Instance.Localizer["msg.credit", Music.Instance.Config.General.Price]);
                    MyMenuManager.CloseMenu(player);
                  }
                });

                confirm.AddOption(new SelectOption
                {
                  Text = "返回",
                  Select = (player, option, menu) =>
                  {
                    MyMenuManager.CloseSubMenu(player);
                  }
                });

                submenu.AddOption(new SubMenuOption { NextMenu = confirm, Text = song.Display() });
              }
              MyMenuManager.GetPlayer(player.Slot).PendingInput = null;
              MyMenuManager.OpenSubMenu(player, submenu);
            }).GetAwaiter().GetResult();
          });
          thread.Start();
        }
      });
    }
    menu.AddOption(new MultiSelectOption
    {
      Text = "声音: " + (PlayManager.IsPlayerHearing(player) ? "开" : "关"),
      Select = (player, option, menu) =>
      {
        PlayManager.TogglePlayerHearing(player);
        option.Text = "声音: " + (PlayManager.IsPlayerHearing(player) ? "开" : "关");
        player.PrintToChat(PlayManager.IsPlayerHearing(player) ? Music.Instance.Localizer["msg.hearingon"] : Music.Instance.Localizer["msg.hearingoff"]);
        option.IsSelected = true;
      }
    });
    menu.AddOption(new MultiSelectOption
    {
      Text = "歌词: " + (HudLyricManager.IsDisplaying(player.Slot) ? "开" : "关"),
      Select = (player, option, menu) =>
      {
        HudLyricManager.ToggleDisplaying(player.Slot);
        option.Text = "歌词: " + (HudLyricManager.IsDisplaying(player.Slot) ? "开" : "关");
        player.PrintToChat(HudLyricManager.IsDisplaying(player.Slot) ? Music.Instance.Localizer["msg.lyricon"] : Music.Instance.Localizer["msg.lyricoff"]);
        option.IsSelected = true;
      }
    });
    menu.AddOption(new MultiSelectOption
    {
      Text = "查看当前歌曲队列",
      Select = (player, option, menu) =>
      {
        PlayManager.PrintQueue(player);
        option.IsSelected = true;
      }
    });
    if (AdminManager.PlayerHasPermissions(player, Music.Instance.Config.General.AdminFlag))
    {
      WasdMyMenu adminMenu = new WasdMyMenu { Title = "管理员控制" };
      adminMenu.AddOption(new SelectOption
      {
        Text = "强制停止当前歌曲",
        Select = (player, option, menu) =>
        {
          PlayManager.Stop(false);
          player.PrintToChat(Music.Instance.Localizer["msg.stopsuccess"]);
          MyMenuManager.CloseMenu(player);
        }
      });
      adminMenu.AddOption(new SelectOption
      {
        Text = "强制停止当前歌曲并清空队列",
        Select = (player, option, menu) =>
        {
          PlayManager.Stop(true);
          player.PrintToChat(Music.Instance.Localizer["msg.clearsuccess"]);
          MyMenuManager.CloseMenu(player);
        }
      });
      menu.AddOption(new SubMenuOption { NextMenu = adminMenu, Text = "管理员控制" });
    }

    return menu;

  }

}