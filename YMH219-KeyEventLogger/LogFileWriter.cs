using System.Text;

namespace KeyEventLogger;

public static class LogFileWriter
{
    public static void Save(string path, IReadOnlyCollection<KeyLogEntry> entries)
    {
        using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
        writer.WriteLine("YMH219 Key Event Logger - kayıt dosyası");
        writer.WriteLine($"Oluşturulma zamanı: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine($"Kayıt sayısı: {entries.Count}");
        writer.WriteLine(new string('-', 60));

        foreach (KeyLogEntry entry in entries)
        {
            writer.WriteLine(entry);
        }
    }

    public static string SuggestFileName(DateTime now)
    {
        return $"keylog_{now:yyyyMMdd_HHmmss}.txt";
    }
}
