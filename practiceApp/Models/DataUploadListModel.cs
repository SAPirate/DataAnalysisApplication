namespace practiceApp.Models
{
    public class DataUploadListModel
    {
        public List<DataUpload> DataUploads { get; set; }

        public string SearchEmail { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }

}
