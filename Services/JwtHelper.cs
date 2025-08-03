using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// JWT 助手類 - 提供 JWT Token 相關的輔助方法
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// 從 Controller 的 HttpContext 中提取 WorkerId
        /// </summary>
        /// <param name="controller">控制器實例</param>
        /// <returns>WorkerId，如果無法提取則返回 null</returns>
        public static int? GetWorkerIdFromToken(ControllerBase controller)
        {
            try
            {
                var user = controller.HttpContext.User;
                
                // 嘗試從自定義 claim "WorkerId" 取得
                var workerIdClaim = user.FindFirst("WorkerId");
                if (workerIdClaim != null && int.TryParse(workerIdClaim.Value, out int workerId))
                {
                    return workerId;
                }

                // 嘗試從標準 claim NameIdentifier 取得
                var nameIdentifierClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (nameIdentifierClaim != null && int.TryParse(nameIdentifierClaim.Value, out int workerIdFromNameIdentifier))
                {
                    return workerIdFromNameIdentifier;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 從 Controller 的 HttpContext 中提取使用者資訊
        /// </summary>
        /// <param name="controller">控制器實例</param>
        /// <returns>使用者資訊，如果無法提取則返回 null</returns>
        public static UserInfo? GetUserInfoFromToken(ControllerBase controller)
        {
            try
            {
                var user = controller.HttpContext.User;
                
                var workerIdClaim = user.FindFirst("WorkerId");
                var emailClaim = user.FindFirst(ClaimTypes.Email);
                var nameClaim = user.FindFirst(ClaimTypes.Name);
                var roleClaim = user.FindFirst(ClaimTypes.Role);

                if (workerIdClaim != null && int.TryParse(workerIdClaim.Value, out int workerId))
                {
                    return new UserInfo
                    {
                        WorkerId = workerId,
                        Email = emailClaim?.Value ?? string.Empty,
                        Name = nameClaim?.Value ?? string.Empty,
                        Role = roleClaim?.Value ?? string.Empty
                    };
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 檢查 token 是否有效且包含必要的 claims
        /// </summary>
        /// <param name="controller">控制器實例</param>
        /// <returns>true 如果 token 有效</returns>
        public static bool IsTokenValid(ControllerBase controller)
        {
            try
            {
                var user = controller.HttpContext.User;
                return user.Identity?.IsAuthenticated == true && GetWorkerIdFromToken(controller).HasValue;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 使用者資訊模型
    /// </summary>
    public class UserInfo
    {
        public int WorkerId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}