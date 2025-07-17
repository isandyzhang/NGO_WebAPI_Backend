using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly PermissionAction _action;
        private readonly string? _caseIdParameter;

        public PermissionAttribute(PermissionAction action, string? caseIdParameter = null)
        {
            _action = action;
            _caseIdParameter = caseIdParameter;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();

            // 從 Authorization header 獲取 token
            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "未提供有效的授權token" });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var workerId = jwtService.GetWorkerIdFromToken(token);

            if (workerId == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "無效的授權token" });
                return;
            }

            int? caseId = null;
            
            // 如果指定了案例ID參數名稱，從請求中獲取
            if (!string.IsNullOrEmpty(_caseIdParameter))
            {
                // 嘗試從路由參數獲取
                if (context.ActionArguments.ContainsKey(_caseIdParameter))
                {
                    if (int.TryParse(context.ActionArguments[_caseIdParameter]?.ToString(), out int parsedCaseId))
                    {
                        caseId = parsedCaseId;
                    }
                }
                // 如果路由參數中沒有，嘗試從查詢字符串獲取
                else if (context.HttpContext.Request.Query.ContainsKey(_caseIdParameter))
                {
                    if (int.TryParse(context.HttpContext.Request.Query[_caseIdParameter], out int parsedCaseId))
                    {
                        caseId = parsedCaseId;
                    }
                }
            }

            // 對於需要從 RegularSuppliesNeed 中獲取 caseId 的情況
            if (caseId == null && context.ActionArguments.ContainsKey("id"))
            {
                caseId = await GetCaseIdFromNeedIdAsync(context, context.ActionArguments["id"]);
            }

            // 檢查權限
            var permissionResult = await permissionService.CanPerformActionAsync(workerId.Value, _action, caseId);
            
            if (!permissionResult.Allowed)
            {
                context.Result = new ForbidResult();
                return;
            }

            // 將 workerId 添加到 HttpContext 中供後續使用
            context.HttpContext.Items["WorkerId"] = workerId.Value;
            
            await next();
        }

        private async Task<int?> GetCaseIdFromNeedIdAsync(ActionExecutingContext context, object? needIdObj)
        {
            if (needIdObj == null || !int.TryParse(needIdObj.ToString(), out int needId))
                return null;

            try
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<NGO_WebAPI_Backend.Models.NgoplatformDbContext>();
                var need = await dbContext.RegularSuppliesNeeds.FindAsync(needId);
                return need?.CaseId;
            }
            catch
            {
                return null;
            }
        }
    }
}