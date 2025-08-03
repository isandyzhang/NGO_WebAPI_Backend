using FluentValidation;
using NGO_WebAPI_Backend.DTOs; using NGO_WebAPI_Backend.Models.Infrastructure;

namespace NGO_WebAPI_Backend.Validators
{
    /// <summary>
    /// 更新個案資料驗證器
    /// </summary>
    public class UpdateCaseDtoValidator : AbstractValidator<UpdateCaseDto>
    {
        public UpdateCaseDtoValidator()
        {
            // 姓名驗證（可選）
            RuleFor(x => x.Name)
                .Length(2, 50).WithMessage("姓名長度必須在 2-50 字元之間")
                .Matches(@"^[\u4e00-\u9fa5a-zA-Z\s]+$").WithMessage("姓名只能包含中文、英文字母和空格")
                .When(x => !string.IsNullOrEmpty(x.Name));

            // 電話驗證（可選）
            RuleFor(x => x.Phone)
                .Matches(@"^09\d{8}$").WithMessage("請輸入正確的手機號碼格式 (09xxxxxxxx)")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            // 身分證字號驗證（可選）
            RuleFor(x => x.IdentityNumber)
                .Length(10).WithMessage("身分證字號必須為 10 位數")
                .Matches(@"^[A-Z][0-9]{9}$").WithMessage("身分證字號格式錯誤，應為 1 個英文字母加 9 個數字")
                .Must(BeValidTaiwanIdentityNumber).WithMessage("身分證字號驗證碼錯誤")
                .When(x => !string.IsNullOrEmpty(x.IdentityNumber));

            // 生日驗證（可選）
            RuleFor(x => x.Birthday)
                .LessThan(DateTime.Now).WithMessage("生日不能是未來日期")
                .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("生日不能超過 120 年前")
                .When(x => x.Birthday.HasValue);

            // 電子郵件驗證（可選）
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("請輸入正確的電子郵件format")
                .When(x => !string.IsNullOrEmpty(x.Email));

            // 性別驗證（可選）
            RuleFor(x => x.Gender)
                .Must(gender => string.IsNullOrEmpty(gender) || 
                               new[] { "Male", "Female", "Other", "male", "female", "other" }.Contains(gender))
                .WithMessage("性別只能是 Male、Female 或 Other")
                .When(x => !string.IsNullOrEmpty(x.Gender));

            // 狀態驗證（可選）
            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || 
                               new[] { "active", "inactive", "suspended" }.Contains(status.ToLower()))
                .WithMessage("狀態只能是 active、inactive 或 suspended")
                .When(x => !string.IsNullOrEmpty(x.Status));

            // 城市驗證（可選）
            RuleFor(x => x.City)
                .MaximumLength(20).WithMessage("城市名稱不能超過 20 個字元")
                .When(x => !string.IsNullOrEmpty(x.City));

            // 區域驗證（可選）
            RuleFor(x => x.District)
                .MaximumLength(20).WithMessage("區域名稱不能超過 20 個字元")
                .When(x => !string.IsNullOrEmpty(x.District));

            // 詳細地址驗證（可選）
            RuleFor(x => x.DetailAddress)
                .MaximumLength(200).WithMessage("詳細地址不能超過 200 個字元")
                .When(x => !string.IsNullOrEmpty(x.DetailAddress));

            // 描述驗證（可選）
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("描述不能超過 1000 個字元")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // 個人照片URL驗證（可選）
            RuleFor(x => x.ProfileImage)
                .Must(BeValidUrl).WithMessage("個人照片必須是有效的 URL")
                .When(x => !string.IsNullOrEmpty(x.ProfileImage));
        }

        /// <summary>
        /// 驗證台灣身分證字號
        /// </summary>
        private static bool BeValidTaiwanIdentityNumber(string identityNumber)
        {
            if (string.IsNullOrEmpty(identityNumber) || identityNumber.Length != 10)
                return false;

            // 台灣身分證字號驗證規則
            var letterValues = new Dictionary<char, int>
            {
                {'A', 10}, {'B', 11}, {'C', 12}, {'D', 13}, {'E', 14},
                {'F', 15}, {'G', 16}, {'H', 17}, {'I', 34}, {'J', 18},
                {'K', 19}, {'L', 20}, {'M', 21}, {'N', 22}, {'O', 35},
                {'P', 23}, {'Q', 24}, {'R', 25}, {'S', 26}, {'T', 27},
                {'U', 28}, {'V', 29}, {'W', 32}, {'X', 30}, {'Y', 31}, {'Z', 33}
            };

            char firstLetter = identityNumber[0];
            if (!letterValues.ContainsKey(firstLetter))
                return false;

            try
            {
                // 取得字母對應的數字
                int letterValue = letterValues[firstLetter];
                
                // 計算驗證碼
                int sum = (letterValue / 10) + (letterValue % 10) * 9;
                
                // 加上後9位數字的權重
                for (int i = 1; i < 9; i++)
                {
                    int digit = int.Parse(identityNumber[i].ToString());
                    sum += digit * (9 - i);
                }
                
                // 加上最後一位數字
                int lastDigit = int.Parse(identityNumber[9].ToString());
                sum += lastDigit;

                // 檢查是否能被10整除
                return sum % 10 == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 驗證URL格式
        /// </summary>
        private static bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}