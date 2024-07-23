using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.PInvoke;

namespace WinapiPrank;

internal class KeyHook : IDisposable
{
    private readonly uint _key;
    private readonly TimeSpan _maxDelayBetweenKeyStrokes;
    private readonly int _numberOfContiguousKeyStrokesToTrigger;
    private readonly Action _actionToTrigger;
    private readonly int? _randomness;

    private int _strokesCounter = 0;
    private DateTime _lastStroke = DateTime.MinValue;
    private HHOOK _hook = HHOOK.Null;

    internal KeyHook(VIRTUAL_KEY key, TimeSpan maxDelayBetweenStrokes, int numberOfContiguousKeyStrokesToTrigger, Action actionToTrigger, int? randomness = null)
    {
        _key = (uint)key;
        _maxDelayBetweenKeyStrokes = maxDelayBetweenStrokes;
        _numberOfContiguousKeyStrokesToTrigger = numberOfContiguousKeyStrokesToTrigger;
        _actionToTrigger = actionToTrigger;
        _randomness = randomness;
    }

    public bool Setup()
    {
        _hook = SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, HookCallback, HINSTANCE.Null, 0);
        return !_hook.IsNull;
    }

    public void Run(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested && GetMessage(out MSG _, HWND.Null, 0, 0)) { }
    }

    private unsafe LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        KBDLLHOOKSTRUCT* keyboard = (KBDLLHOOKSTRUCT*)lParam.Value;
        if ((*keyboard).vkCode == _key && wParam == WM_KEYDOWN)
        {
            var now = DateTime.Now;
            if (now - _lastStroke <= _maxDelayBetweenKeyStrokes)
            {
                _strokesCounter++;
            }
            else
            {
                _strokesCounter = 1;
            }
            _lastStroke = now;
            if (_strokesCounter == _numberOfContiguousKeyStrokesToTrigger)
            {
                if (_randomness.HasValue)
                {
                    if (Random.Shared.Next(_randomness.Value) == 0)
                    {
                        _actionToTrigger();
                    }
                }
                else
                {
                    _actionToTrigger();
                }
                _strokesCounter = 0;
            }
        }
        return CallNextHookEx(HHOOK.Null, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hook.IsNull) return;
        UnhookWindowsHookEx(_hook);
        _hook = HHOOK.Null;
    }
}