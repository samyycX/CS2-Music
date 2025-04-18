using System.Reflection;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

public unsafe static class Audio2
{
  public delegate void PlayStartHandler(int slot);
  public delegate void PlayEndHandler(int slot);
  public delegate void PlayHandler(int slot);

  private static class NativeMethods
  {
    public delegate void NativeSetPlayerHearing(int slot, bool hearing);

    public delegate void NativeSetAllPlayerHearing(bool hearing);

    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool NativeIsHearing(int slot);

    public delegate void NativePlayToPlayer(int slot, [MarshalAs(UnmanagedType.LPArray)] byte[] audioBuffer, int audioBufferSize, string audioPath, int audioPathSize, float volume = 1f);

    public delegate void NativePlay([MarshalAs(UnmanagedType.LPArray)] byte[] audioBuffer, int audioBufferSize, string audioPath, int audioPathSize, float volume = 1f);

    public delegate void NativeStopAllPlaying();

    public delegate void NativeStopPlaying(int slot);

    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool NativeIsPlaying(int slot);

    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool NativeIsAllPlaying();

    public delegate int NativeRegisterPlayStartListener([MarshalAs(UnmanagedType.FunctionPtr)] PlayStartHandler callback);

    public delegate void NativeUnregisterPlayStartListener(int id);

    public delegate int NativeRegisterPlayEndListener([MarshalAs(UnmanagedType.FunctionPtr)] PlayEndHandler callback);

    public delegate void NativeUnregisterPlayEndListener(int id);

    public delegate int NativeRegisterPlayListener([MarshalAs(UnmanagedType.FunctionPtr)] PlayHandler callback);

    public delegate void NativeUnregisterPlayListener(int id);

    public delegate void NativeSetPlayer(int slot);

  }

  private static Dictionary<PlayStartHandler, int> _PlayStartListeners = new Dictionary<PlayStartHandler, int>();
  private static Dictionary<PlayEndHandler, int> _PlayEndListeners = new Dictionary<PlayEndHandler, int>();
  private static Dictionary<PlayHandler, int> _PlayListeners = new Dictionary<PlayHandler, int>();

  private static Dictionary<int, Delegate> _MetamodFunctions = new Dictionary<int, Delegate>();

  private unsafe static T GetMetamodFunction<T>(int index) where T : Delegate
  {
    if (_MetamodFunctions.TryGetValue(index, out var func))
    {
      return (T)func;
    }

    var pIAudio = Utilities.MetaFactory("Audio002");
    if (pIAudio == null)
    {
      throw new Exception("Metamod plugin 'audio' not found or not loaded.");
    }
    var pFunc = *(nint*)(*(nint*)pIAudio + index * sizeof(nint));
    _MetamodFunctions[index] = Marshal.GetDelegateForFunctionPointer<T>(pFunc);
    return (T)_MetamodFunctions[index];
  }

  /*
  * @param slot - player slot to set
  * @param hearing - whether player can hear
  */
  public static void SetPlayerHearing(int slot, bool hearing)
  {
    GetMetamodFunction<NativeMethods.NativeSetPlayerHearing>(0)(slot, hearing);
  }

  /*
  * @param hearing - whether all players can hear
  */
  public static void SetAllPlayerHearing(bool hearing)
  {
    GetMetamodFunction<NativeMethods.NativeSetAllPlayerHearing>(1)(hearing);
  }

  /*
  * @param slot - player slot to get
  * @return whether player can hear
  */
  public static bool IsHearing(int slot)
  {
    return GetMetamodFunction<NativeMethods.NativeIsHearing>(2)(slot);
  }

  /*
  * @param slot - player slot to set
  * @param audioBuffer - buffer string, contains audio data (like mp3, wav), will be decoded to pcm by ffmpeg,
    pass empty string means stop playing
  */
  public static void PlayToPlayerFromBuffer(int slot, byte[] audioBuffer, float volume = 1f)
  {
    GetMetamodFunction<NativeMethods.NativePlayToPlayer>(3)(slot, audioBuffer, audioBuffer.Length, "", 0, volume);
  }

  /*
  * @param slot - player slot to set
  * @param audioFile - audio file path, must be absolute path to a audio file (like mp3, wav),
    will be decoded to pcm by ffmpeg, pass empty string means stop playing
  */
  public static void PlayToPlayerFromFile(int slot, string audioFile, float volume = 1f)
  {
    GetMetamodFunction<NativeMethods.NativePlayToPlayer>(4)(slot, [], 0, audioFile, audioFile.Length, volume);
  }

  /*
  * @param audioBuffer - buffer string, contains audio data (like mp3, wav), will be decoded to pcm by ffmpeg,
    pass empty string means stop playing
  */
  public static void PlayFromBuffer(byte[] audioBuffer, float volume = 1f)
  {
    GetMetamodFunction<NativeMethods.NativePlay>(5)(audioBuffer, audioBuffer.Length, "", 0, volume);
  }

  /*
  * @param audioFile - audio file path, must be absolute path to a audio file (like mp3, wav),
    will be decoded to pcm by ffmpeg, pass empty string means stop playing
  */
  public static void PlayFromFile(string audioFile, float volume = 1f)
  {
    GetMetamodFunction<NativeMethods.NativePlay>(6)([], 0, audioFile, audioFile.Length, volume);
  }

  /*
  * Stop all playing audio
  */
  public static void StopAllPlaying()
  {
    GetMetamodFunction<NativeMethods.NativeStopAllPlaying>(7)();
  }

  /*
  * @param slot - player slot to stop
  */
  public static void StopPlaying(int slot)
  {
    GetMetamodFunction<NativeMethods.NativeStopPlaying>(8)(slot);
  }

  /*
  * @param slot - player slot to get
  * @return whether there are audio playing for a specific player
  */
  public static bool IsPlaying(int slot)
  {
    return GetMetamodFunction<NativeMethods.NativeIsPlaying>(9)(slot);
  }

  /*
  * @return whether there are audio playing for all players
  */
  public static bool IsAllPlaying()
  {
    return GetMetamodFunction<NativeMethods.NativeIsAllPlaying>(10)();
  }

  /*
  * @param handler - play start handler
  * @return id - listener id, you can ignore it
  * @note the slot will be either player slot or -1, -1 means all players
  */
  public static int RegisterPlayStartListener(PlayStartHandler handler)
  {
    var id = GetMetamodFunction<NativeMethods.NativeRegisterPlayStartListener>(11)(handler);
    _PlayStartListeners[handler] = id;
    return id;
  }

  /*
  * @param handler - play start handler
  */
  public static void UnregisterPlayStartListener(PlayStartHandler handler)
  {
    GetMetamodFunction<NativeMethods.NativeUnregisterPlayStartListener>(12)(_PlayStartListeners[handler]);
    _PlayStartListeners.Remove(handler);
  }

  /*
  * @param handler - play end handler
  * @return id - listener id, you can ignore it
  * @note the slot will be either player slot or -1, -1 means all players
  */
  public static int RegisterPlayEndListener(PlayEndHandler handler)
  {
    var id = GetMetamodFunction<NativeMethods.NativeRegisterPlayEndListener>(13)(handler);
    _PlayEndListeners[handler] = id;
    return id;
  }
  /*
  * @param handler - play end handler
  */
  public static void UnregisterPlayEndListener(PlayEndHandler handler)
  {
    GetMetamodFunction<NativeMethods.NativeUnregisterPlayEndListener>(14)(_PlayEndListeners[handler]);
    _PlayEndListeners.Remove(handler);
  }
  public static int RegisterPlayListener(PlayHandler handler)
  {
    var id = GetMetamodFunction<NativeMethods.NativeRegisterPlayListener>(15)(handler);
    _PlayListeners[handler] = id;
    return id;
  }
  public static void UnregisterPlayListener(PlayHandler handler)
  {
    GetMetamodFunction<NativeMethods.NativeUnregisterPlayListener>(16)(_PlayListeners[handler]);
    _PlayListeners.Remove(handler);
  }
  /*
  * @param slot - player slot to set
  */
  public static void SetPlayer(int slot)
  {
    GetMetamodFunction<NativeMethods.NativeSetPlayer>(17)(slot);
  }
}