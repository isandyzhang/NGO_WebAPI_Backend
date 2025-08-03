using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend
{
    /// <summary>
    /// 密碼加密功能測試類別
    /// </summary>
    public class TestPasswordEncryption
    {
        public static void RunTests()
        {
            var passwordService = new PasswordService();
            
            Console.WriteLine("=== Argon2 密碼加密測試 ===");
            
            // 測試1: 基本密碼雜湊和驗證
            string password1 = "testPassword123";
            string hashedPassword1 = passwordService.HashPassword(password1);
            bool isValid1 = passwordService.VerifyPassword(password1, hashedPassword1);
            
            Console.WriteLine($"原始密碼: {password1}");
            Console.WriteLine($"雜湊密碼: {hashedPassword1}");
            Console.WriteLine($"驗證結果: {isValid1}");
            Console.WriteLine();
            
            // 測試2: 錯誤密碼驗證
            bool isInvalid = passwordService.VerifyPassword("wrongPassword", hashedPassword1);
            Console.WriteLine($"錯誤密碼驗證: {isInvalid}");
            Console.WriteLine();
            
            // 測試3: 相同密碼產生不同雜湊值（鹽值隨機性）
            string hashedPassword2 = passwordService.HashPassword(password1);
            bool isSamePassword = passwordService.VerifyPassword(password1, hashedPassword2);
            Console.WriteLine($"相同密碼的另一個雜湊: {hashedPassword2}");
            Console.WriteLine($"雜湊值是否相同: {hashedPassword1 == hashedPassword2}");
            Console.WriteLine($"驗證是否成功: {isSamePassword}");
            Console.WriteLine();
            
            // 測試4: 空密碼處理
            try
            {
                passwordService.HashPassword("");
                Console.WriteLine("空密碼處理: 未拋出異常（錯誤）");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("空密碼處理: 正確拋出異常");
            }
            
            // 測試5: null密碼處理
            try
            {
                passwordService.HashPassword(null!);
                Console.WriteLine("null密碼處理: 未拋出異常（錯誤）");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("null密碼處理: 正確拋出異常");
            }
            
            Console.WriteLine("\n=== 測試完成 ===");
        }
    }
}