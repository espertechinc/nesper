namespace NEsper.Benchmarks.EndToEnd;

public class TradeEvent
{
    public string Symbol    { get; set; } = string.Empty;
    public double Price     { get; set; }
    public long   Volume    { get; set; }
    public long   Timestamp { get; set; }
}
