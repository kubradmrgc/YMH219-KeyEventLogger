namespace KeyEventLogger;

/// <summary>
/// Kayıtları başlangıç ve bitiş zamanı arasında (iki uç dahil) süzer.
/// </summary>
public sealed class KeyLogFilter
{
    public KeyLogFilter(DateTime from, DateTime to)
    {
        if (to < from)
        {
            throw new ArgumentException("Bitiş zamanı başlangıç zamanından önce olamaz.", nameof(to));
        }

        From = from;
        To = to;
    }

    public DateTime From { get; }
    public DateTime To { get; }

    /// <summary>
    /// Saniye hassasiyetindeki seçimden filtre oluşturur; bitiş saniyesinin tamamı aralığa dahil edilir.
    /// </summary>
    public static KeyLogFilter FromSeconds(DateTime from, DateTime to)
    {
        DateTime start = TruncateToSecond(from);
        DateTime end = TruncateToSecond(to).AddSeconds(1).AddTicks(-1);
        return new KeyLogFilter(start, end);
    }

    public bool Matches(KeyLogEntry entry)
    {
        return entry.Timestamp >= From && entry.Timestamp <= To;
    }

    public IEnumerable<KeyLogEntry> Apply(IEnumerable<KeyLogEntry> entries)
    {
        return entries.Where(Matches);
    }

    public override string ToString()
    {
        return $"{From:dd.MM.yyyy HH:mm:ss} - {To:dd.MM.yyyy HH:mm:ss}";
    }

    private static DateTime TruncateToSecond(DateTime value)
    {
        return new DateTime(value.Ticks - value.Ticks % TimeSpan.TicksPerSecond, value.Kind);
    }
}
