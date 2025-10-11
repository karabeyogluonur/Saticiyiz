namespace ST.Application.DTOs.Messages
{
    public class UINotificationDto
    {
        public string Type { get; set; } = "info";
        public string Message { get; set; } = string.Empty;
        public string? Title { get; set; }
        public int? Duration { get; set; }
    }
}
