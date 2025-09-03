using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

public static class ImpersonationHelper
{
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool LogonUser(
        string lpszUsername,
        string lpszDomain,
        string lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        out SafeAccessTokenHandle phToken);

    public static void RunAs(string domain, string username, string password, Action action)
    {
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        const int LOGON32_PROVIDER_WINNT50 = 3;

        if (!LogonUser(username, domain, password,
            LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, out var safeTokenHandle))
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        WindowsIdentity.RunImpersonated(safeTokenHandle, action);
    }
}