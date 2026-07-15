using TRS.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;

namespace TRS.Middlewares
{
    public class ImpersonationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<LdapSettingDto> ldapSettingDto;
        public ImpersonationMiddleware(RequestDelegate next, IOptions<LdapSettingDto> ldapSettingDto)
        {
            this.ldapSettingDto = ldapSettingDto;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
            const int LOGON32_PROVIDER_WINNT50 = 3;

            SafeAccessTokenHandle safeTokenHandle;

            bool success = LogonUser(ldapSettingDto.Value.UserName, ldapSettingDto.Value.Domain, ldapSettingDto.Value.Password,
                LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, out safeTokenHandle);

            if (!success || safeTokenHandle.IsInvalid)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            await WindowsIdentity.RunImpersonatedAsync(safeTokenHandle, async () =>
            {
                await _next(context);
            });
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out SafeAccessTokenHandle phToken);
    }
}