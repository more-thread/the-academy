using System.Security.Cryptography;
using System.Text;
using TRS.Global;

namespace TRS.Services
{
    public interface IEmployeeAuthenticationService
    {
        Task<string> ValidateAndDecryptEmployeeIdAsync(string encryptedId);
        string DecryptEmployeeId(string encryptedId);
    }

    public class EmployeeAuthenticationService : IEmployeeAuthenticationService
    {
        private readonly ILogger<EmployeeAuthenticationService> _logger;
        private readonly GlobalService _globalService;
        private readonly IConfiguration _configuration;

        public EmployeeAuthenticationService(
            ILogger<EmployeeAuthenticationService> logger,
            GlobalService globalService,
            IConfiguration configuration)
        {
            _logger = logger;
            _globalService = globalService;
            _configuration = configuration;
        }

        public async Task<string> ValidateAndDecryptEmployeeIdAsync(string encryptedId)
        {
            if (string.IsNullOrEmpty(encryptedId))
                return null;

            try
            {
                // Decrypt the employee ID
                var employeeNo = DecryptEmployeeId(encryptedId);
                
                if (string.IsNullOrEmpty(employeeNo))
                    return null;

                return employeeNo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating encrypted employee ID");
                return null;
            }
        }

        public string DecryptEmployeeId(string encryptedId)
        {
            try
            {
                // URL decode the encrypted string
                encryptedId = Uri.UnescapeDataString(encryptedId);
                
                // Get encryption key from configuration (fallback to original key)
                var encryptionKey = _configuration["Authentication:EncryptionKey"] ?? "MAKV2SPBNI99212";
                var salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

                byte[] cipherBytes = Convert.FromBase64String(encryptedId);
                
                using (Aes encryptor = Aes.Create())
                {
                    using (var pdb = new Rfc2898DeriveBytes(encryptionKey, salt))
                    {
                        encryptor.Key = pdb.GetBytes(32);
                        encryptor.IV = pdb.GetBytes(16);
                        
                        using (var ms = new MemoryStream())
                        {
                            using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(cipherBytes, 0, cipherBytes.Length);
                            }
                            var decrypted = Encoding.Unicode.GetString(ms.ToArray()).Trim('\0');
                            _logger.LogInformation("Successfully decrypted employee ID: {EmployeeNo}", decrypted);
                            return decrypted;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting employee ID: {EncryptedId}", encryptedId);
                throw new CryptographicException("Failed to decrypt employee ID", ex);
            }
        }
    }
}