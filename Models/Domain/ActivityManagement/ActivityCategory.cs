namespace NGO_WebAPI_Backend.Models.Domain.ActivityManagement;

/// <summary>
/// 活動分類標籤常數
/// </summary>
public static class ActivityCategory
{
    /// <summary>
    /// 活動分類選項
    /// </summary>
    public static readonly Dictionary<string, string> Categories = new Dictionary<string, string>
    {
        { "生活", "生活" },
        { "心靈", "心靈" },
        { "運動", "運動" },
        { "娛樂", "娛樂" },
        { "教育", "教育" },
        { "醫療", "醫療" },
        { "環保", "環保" },
        { "電子", "電子" },
        { "社福", "社福" }
    };

    /// <summary>
    /// 取得所有分類選項
    /// </summary>
    public static List<CategoryOption> GetAllCategories()
    {
        return Categories.Select(c => new CategoryOption
        {
            Value = c.Key,
            Label = c.Value
        }).ToList();
    }

    /// <summary>
    /// 驗證分類是否有效
    /// </summary>
    public static bool IsValidCategory(string? category)
    {
        if (string.IsNullOrEmpty(category))
            return true; // 允許空值

        return Categories.ContainsKey(category);
    }
}

/// <summary>
/// 分類選項資料傳輸物件
/// </summary>
public class CategoryOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}