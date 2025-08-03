namespace NGO_WebAPI_Backend.DTOs
{
    /// <summary>
    /// 個案資料傳輸物件
    /// </summary>
    public class CaseDto
    {
        public int CaseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public string Address { get; set; } = string.Empty;
        public int WorkerId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImage { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? DetailAddress { get; set; }
        public string? WorkerName { get; set; }
        public string? SpeechToTextAudioUrl { get; set; }
    }

    /// <summary>
    /// 建立個案資料傳輸物件
    /// </summary>
    public class CreateCaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public int? WorkerId { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImage { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? DetailAddress { get; set; }
        public string? SpeechToTextAudioUrl { get; set; }
    }

    /// <summary>
    /// 更新個案資料傳輸物件
    /// </summary>
    public class UpdateCaseDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? IdentityNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImage { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? DetailAddress { get; set; }
    }
}