namespace KeyEventLogger;

public readonly record struct KeyCount(string KeyName, int Count);

/// <summary>
/// Kayıtlardan toplam olay sayısını, basılan tuş sayısını ve en sık basılan tuşları hesaplar.
/// Tuş sayımı KeyDown olaylarına göre yapılır.
/// </summary>
public sealed class KeyStatistics
{
    private KeyStatistics(int totalEvents, int totalKeyPresses, IReadOnlyList<KeyCount> topKeys)
    {
        TotalEvents = totalEvents;
        TotalKeyPresses = totalKeyPresses;
        TopKeys = topKeys;
    }

    public int TotalEvents { get; }
    public int TotalKeyPresses { get; }
    public IReadOnlyList<KeyCount> TopKeys { get; }

    public static KeyStatistics Calculate(IEnumerable<KeyLogEntry> entries, int topCount = 5)
    {
        List<KeyLogEntry> list = entries.ToList();
        List<KeyLogEntry> keyDowns = list.Where(e => e.Kind == KeyEventKind.KeyDown).ToList();

        List<KeyCount> topKeys = keyDowns
            .GroupBy(e => e.KeyName)
            .Select(g => new KeyCount(g.Key, g.Count()))
            .OrderByDescending(k => k.Count)
            .ThenBy(k => k.KeyName, StringComparer.Ordinal)
            .Take(topCount)
            .ToList();

        return new KeyStatistics(list.Count, keyDowns.Count, topKeys);
    }
}
