namespace dohoangdanh.Models
{
    public class LocationLog
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}