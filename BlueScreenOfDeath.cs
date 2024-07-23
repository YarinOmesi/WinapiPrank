using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace WinapiPrank;

public static class BlueScreenOfDeath
{
    private const uint STATUS_ASSERTION_FAILURE = 0xC0000420;

    public static bool Trigger()
    {
#if EnabbleBSOD
        RtlAdjustPrivilege(19, true, false, out bool previousValue);

        NTSTATUS ntstatus = NtRaiseHardError(STATUS_ASSERTION_FAILURE,
            0,
            0,
            IntPtr.Zero,
            6,
            out uint oul
        );

        return ntstatus == 0;
#else
        return false;
#endif
    }

    [DllImport("ntdll.dll")]
    private static extern uint RtlAdjustPrivilege(
        int privilege,
        bool bEnablePrivilege,
        bool isThreadPrivilege,
        out bool previousValue
    );

    [DllImport("ntdll.dll")]
    private static extern NTSTATUS NtRaiseHardError(
        uint errorStatus,
        uint numberOfParameters,
        uint unicodeStringParameterMask,
        IntPtr parameters,
        uint validResponseOption,
        out uint response
    );
}