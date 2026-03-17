namespace thirdpartynetwork.Application.Dtos.Metric
{
    public class NetworkMetricsDto
    {
        public Guid NetworkId { get; set; }
        public int FileCount { get; set; } = 0;
        public long FileStorage { get; set; } = 0; // in KB
    
        public int BlogCount { get; set; } = 0;
    
        public int ConfigurationCount { get; set; } = 0;
    
        public int CustomPageCount { get; set; } = 0;
        public int CustomPageBlockCount { get; set; } = 0;
        public int CustomPageBlockSize { get; set; } = 0; // in Chars
    }
}
