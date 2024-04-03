namespace BidenSurfer.Infras
{
    public class DataResponseWs
    {
        public string? symbol { get; set; }
        public string? channel { get; set; }
        public object? data { get; set; }
        public long? ts { get; set; }
    }
}
