namespace practiceApp.Models
{
    public class DataUpload
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public string SentToEmail { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public string UploadedBy { get; set; }
    }

}
