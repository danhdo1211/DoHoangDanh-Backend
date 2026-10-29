namespace dohoangdanh.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Model { get; set; }
        public int ChildId { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}