namespace NEsper.Benchmarks.EndToEnd;

public class QuoteEvent
{
    public string Symbol   { get; set; } = string.Empty;
    public double BidPrice { get; set; }
    public double AskPrice { get; set; }
}
