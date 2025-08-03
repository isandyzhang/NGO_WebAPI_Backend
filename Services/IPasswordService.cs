namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// 密碼服務接口
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// 使用Argon2雜湊密碼
        /// </summary>
        /// <param name="password">明文密碼</param>
        /// <returns>雜湊後的密碼</returns>
        string HashPassword(string password);

        /// <summary>
        /// 驗證密碼
        /// </summary>
        /// <param name="password">明文密碼</param>
        /// <param name="hashedPassword">雜湊密碼</param>
        /// <returns>驗證結果</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}