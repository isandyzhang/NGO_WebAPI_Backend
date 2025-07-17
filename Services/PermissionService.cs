using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;
using System.Security.Claims;

namespace NGO_WebAPI_Backend.Services
{
    public enum UserRole
    {
        Staff,
        Supervisor,
        Admin
    }

    public enum PermissionAction
    {
        View,
        Approve,
        Reject,
        Distribute,
        Supervise,
        Delete
    }

    public class PermissionCheckResult
    {
        public bool Allowed { get; set; }
        public string? Reason { get; set; }

        public static PermissionCheckResult Allow() => new PermissionCheckResult { Allowed = true };
        public static PermissionCheckResult Deny(string reason) => new PermissionCheckResult { Allowed = false, Reason = reason };
    }

    public interface IPermissionService
    {
        Task<PermissionCheckResult> CanViewCaseAsync(int workerId, int caseId);
        Task<PermissionCheckResult> CanPerformActionAsync(int workerId, PermissionAction action, int? caseId = null);
        Task<List<int>> GetAccessibleCaseIdsAsync(int workerId);
        Task<UserRole?> GetUserRoleAsync(int workerId);
        Task<bool> IsWorkerResponsibleForCaseAsync(int workerId, int caseId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(NgoplatformDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PermissionCheckResult> CanViewCaseAsync(int workerId, int caseId)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(workerId);
                if (worker == null)
                {
                    return PermissionCheckResult.Deny("工作人員不存在");
                }

                var userRole = ParseUserRole(worker.Role);

                // 管理員和主管都可以查看所有案例（單一公司環境）
                if (userRole == UserRole.Admin || userRole == UserRole.Supervisor)
                {
                    return PermissionCheckResult.Allow();
                }

                // 檢查案例是否存在
                var caseExists = await _context.Cases.AnyAsync(c => c.CaseId == caseId);
                if (!caseExists)
                {
                    return PermissionCheckResult.Deny("案例不存在");
                }

                // 檢查工作人員是否負責該案例
                var isResponsible = await IsWorkerResponsibleForCaseAsync(workerId, caseId);
                if (!isResponsible)
                {
                    return PermissionCheckResult.Deny("您無權查看此案例");
                }

                return PermissionCheckResult.Allow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查案例查看權限時發生錯誤: WorkerId={WorkerId}, CaseId={CaseId}", workerId, caseId);
                return PermissionCheckResult.Deny("權限檢查失敗");
            }
        }

        public async Task<PermissionCheckResult> CanPerformActionAsync(int workerId, PermissionAction action, int? caseId = null)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(workerId);
                if (worker == null)
                {
                    return PermissionCheckResult.Deny("工作人員不存在");
                }

                var userRole = ParseUserRole(worker.Role);

                // 如果涉及特定案例，先檢查案例存取權限
                if (caseId.HasValue)
                {
                    var caseViewResult = await CanViewCaseAsync(workerId, caseId.Value);
                    if (!caseViewResult.Allowed)
                    {
                        return caseViewResult;
                    }
                }

                // 根據動作類型和角色檢查權限
                switch (action)
                {
                    case PermissionAction.View:
                        return PermissionCheckResult.Allow(); // 基本查看權限已在 CanViewCaseAsync 中檢查

                    case PermissionAction.Approve:
                    case PermissionAction.Reject:
                        // 員工和主管都可以批准/拒絕
                        if (userRole == UserRole.Staff || userRole == UserRole.Supervisor || userRole == UserRole.Admin)
                        {
                            return PermissionCheckResult.Allow();
                        }
                        return PermissionCheckResult.Deny("您沒有批准/拒絕權限");

                    case PermissionAction.Supervise:
                        // 只有主管和管理員可以執行監督操作
                        if (userRole == UserRole.Supervisor || userRole == UserRole.Admin)
                        {
                            return PermissionCheckResult.Allow();
                        }
                        return PermissionCheckResult.Deny("僅主管和管理員可以執行審核操作");

                    case PermissionAction.Distribute:
                        // 只有主管和管理員可以執行分發操作
                        if (userRole == UserRole.Supervisor || userRole == UserRole.Admin)
                        {
                            return PermissionCheckResult.Allow();
                        }
                        return PermissionCheckResult.Deny("僅主管和管理員可以執行分發操作");

                    case PermissionAction.Delete:
                        // 刪除需要至少與批准相同的權限
                        if (userRole == UserRole.Staff || userRole == UserRole.Supervisor || userRole == UserRole.Admin)
                        {
                            return PermissionCheckResult.Allow();
                        }
                        return PermissionCheckResult.Deny("您沒有刪除權限");

                    default:
                        return PermissionCheckResult.Deny("未知的操作類型");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查操作權限時發生錯誤: WorkerId={WorkerId}, Action={Action}, CaseId={CaseId}", workerId, action, caseId);
                return PermissionCheckResult.Deny("權限檢查失敗");
            }
        }

        public async Task<List<int>> GetAccessibleCaseIdsAsync(int workerId)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(workerId);
                if (worker == null)
                {
                    return new List<int>();
                }

                var userRole = ParseUserRole(worker.Role);

                // 管理員和主管都可以存取所有案例（單一公司環境）
                if (userRole == UserRole.Admin || userRole == UserRole.Supervisor)
                {
                    return await _context.Cases.Select(c => c.CaseId).ToListAsync();
                }

                // 員工只能存取自己負責的案例
                return await _context.Cases
                    .Where(c => c.WorkerId == workerId)
                    .Select(c => c.CaseId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得可存取案例列表時發生錯誤: WorkerId={WorkerId}", workerId);
                return new List<int>();
            }
        }

        public async Task<UserRole?> GetUserRoleAsync(int workerId)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(workerId);
                return worker != null ? ParseUserRole(worker.Role) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得用戶角色時發生錯誤: WorkerId={WorkerId}", workerId);
                return null;
            }
        }

        public async Task<bool> IsWorkerResponsibleForCaseAsync(int workerId, int caseId)
        {
            try
            {
                // 檢查工作人員是否負責該案例
                var isDirectlyResponsible = await _context.Cases
                    .AnyAsync(c => c.CaseId == caseId && c.WorkerId == workerId);

                if (isDirectlyResponsible)
                {
                    return true;
                }

                // 檢查是否為主管或管理員
                var worker = await _context.Workers.FindAsync(workerId);
                if (worker != null)
                {
                    var userRole = ParseUserRole(worker.Role);
                    
                    // 管理員和主管都可以監督所有案例（單一公司環境）
                    if (userRole == UserRole.Admin || userRole == UserRole.Supervisor)
                    {
                        return await _context.Cases.AnyAsync(c => c.CaseId == caseId);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查工作人員案例責任時發生錯誤: WorkerId={WorkerId}, CaseId={CaseId}", workerId, caseId);
                return false;
            }
        }

        private UserRole ParseUserRole(string? role)
        {
            return role?.ToLower() switch
            {
                "staff" => UserRole.Staff,
                "supervisor" => UserRole.Supervisor,
                "admin" => UserRole.Admin,
                _ => UserRole.Staff // 預設為員工
            };
        }
    }
}