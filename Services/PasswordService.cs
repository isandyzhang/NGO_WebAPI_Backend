using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// Argon2密碼加密服務
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 32; // 256位元
        private const int HashSize = 64; // 512位元
        private const int DegreeOfParallelism = 1;
        private const int Iterations = 2;
        private const int MemorySize = 1024; // 1MB

        /// <summary>
        /// 使用Argon2id雜湊密碼
        /// </summary>
        /// <param name="password">明文密碼</param>
        /// <returns>Base64編碼的雜湊密碼（包含鹽值）</returns>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("密碼不能為空", nameof(password));

            // 生成隨機鹽值
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // 使用Argon2id雜湊密碼
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize
            };

            var hash = argon2.GetBytes(HashSize);

            // 將鹽值和雜湊值組合並編碼為Base64
            var combined = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, combined, 0, SaltSize);
            Array.Copy(hash, 0, combined, SaltSize, HashSize);

            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        /// <param name="password">明文密碼</param>
        /// <param name="hashedPassword">Base64編碼的雜湊密碼</param>
        /// <returns>驗證結果</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                // 解碼Base64字串
                var combined = Convert.FromBase64String(hashedPassword);

                if (combined.Length != SaltSize + HashSize)
                    return false;

                // 提取鹽值和雜湊值
                var salt = new byte[SaltSize];
                var hash = new byte[HashSize];
                Array.Copy(combined, 0, salt, 0, SaltSize);
                Array.Copy(combined, SaltSize, hash, 0, HashSize);

                // 使用相同參數雜湊輸入密碼
                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
                {
                    Salt = salt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize
                };

                var newHash = argon2.GetBytes(HashSize);

                // 使用固定時間比較防止時序攻擊
                return CryptographicOperations.FixedTimeEquals(hash, newHash);
            }
            catch
            {
                return false;
            }
        }
    }
}