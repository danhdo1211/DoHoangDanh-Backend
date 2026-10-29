namespace dohoangdanh.Models
{
    public class AppRule
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string AppName { get; set; } = "";
        public bool IsBlocked { get; set; }
        public int DailyLimitMinutes { get; set; }
    }
}