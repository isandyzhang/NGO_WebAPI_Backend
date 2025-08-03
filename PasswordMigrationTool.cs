using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend
{
    /// <summary>
    /// 密碼遷移工具 - 將所有明碼密碼轉換為Argon2雜湊
    /// </summary>
    public class PasswordMigrationTool
    {
        private readonly NgoplatformDbContext _context;
        private readonly IPasswordService _passwordService;

        public PasswordMigrationTool(NgoplatformDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        /// <summary>
        /// 執行密碼遷移 - 將所有工作人員密碼設為 pw123456 並使用Argon2加密
        /// </summary>
        public async Task MigratePasswordsAsync()
        {
            Console.WriteLine("=== 開始密碼遷移 ===");
            
            try
            {
                // 取得所有工作人員
                var workers = await _context.Workers.ToListAsync();
                Console.WriteLine($"找到 {workers.Count} 個工作人員帳號");

                const string newPassword = "pw123456";
                string hashedPassword = _passwordService.HashPassword(newPassword);
                
                Console.WriteLine($"新密碼: {newPassword}");
                Console.WriteLine($"雜湊密碼長度: {hashedPassword.Length} 字元");
                Console.WriteLine();

                int updatedCount = 0;
                foreach (var worker in workers)
                {
                    string oldPassword = worker.Password ?? "無密碼";
                    worker.Password = hashedPassword;
                    
                    Console.WriteLine($"更新工作人員: {worker.Name} ({worker.Email})");
                    Console.WriteLine($"  舊密碼: {oldPassword}");
                    Console.WriteLine($"  新雜湊: {hashedPassword.Substring(0, 20)}...");
                    Console.WriteLine();
                    
                    updatedCount++;
                }

                // 儲存變更
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"=== 密碼遷移完成 ===");
                Console.WriteLine($"成功更新 {updatedCount} 個帳號的密碼");
                Console.WriteLine($"所有帳號的新密碼都是: {newPassword}");
                Console.WriteLine("所有密碼已使用Argon2加密儲存");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"密碼遷移失敗: {ex.Message}");
                Console.WriteLine($"錯誤詳情: {ex}");
                throw;
            }
        }

        /// <summary>
        /// 驗證遷移結果
        /// </summary>
        public async Task VerifyMigrationAsync()
        {
            Console.WriteLine("\n=== 驗證遷移結果 ===");
            
            const string testPassword = "pw123456";
            var workers = await _context.Workers.Take(3).ToListAsync();
            
            foreach (var worker in workers)
            {
                bool isValid = _passwordService.VerifyPassword(testPassword, worker.Password ?? "");
                Console.WriteLine($"{worker.Name} ({worker.Email}): {(isValid ? "✓ 驗證成功" : "✗ 驗證失敗")}");
            }
        }
    }
}