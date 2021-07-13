namespace JisuiBatch
{
    public class AppSettings
    {
        public UtilityDirectory UtilityDirectory { get; set; }
        public WorkingDirectory WorkingDirectory { get; set; }
        public int DegreeOfParallelism { get; set; }
    }

    public class UtilityDirectory
    {
        public string ImageMagick { get; set; }
        public string Mozjpeg { get; set; }
        public string Pngquant { get; set; }
        public string SevenZip { get; set; }
    }

    public class WorkingDirectory
    {
        public string Root { get; set; }
        public string ColorImage { get; set; }
        public string GrayImage { get; set; }
        public string TextImage { get; set; }
        public string ProcessedImage { get; set; }
    }
}
