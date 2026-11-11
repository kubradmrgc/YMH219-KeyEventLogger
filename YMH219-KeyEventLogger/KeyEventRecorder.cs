namespace KeyEventLogger;

/// <summary>
/// Kayıt durumunu ve yakalanan olayları yönetir. Kayıt kapalıyken gelen olaylar yok sayılır.
/// </summary>
public class KeyEventRecorder
{
    private readonly List<KeyLogEntry> entries = new();

    public event EventHandler<KeyLogEntry>? EntryAdded;
    public event EventHandler? EntriesCleared;
    public event EventHandler? RecordingChanged;

    public bool IsRecording { get; private set; }
    public IReadOnlyList<KeyLogEntry> Entries => entries;

    public void Start()
    {
        SetRecording(true);
    }

    public void Stop()
    {
        SetRecording(false);
    }

    public bool TryRecord(KeyLogEntry entry)
    {
        if (!IsRecording)
        {
            return false;
        }

        entries.Add(entry);
        EntryAdded?.Invoke(this, entry);
        return true;
    }

    public void Clear()
    {
        entries.Clear();
        EntriesCleared?.Invoke(this, EventArgs.Empty);
    }

    private void SetRecording(bool recording)
    {
        if (IsRecording == recording)
        {
            return;
        }

        IsRecording = recording;
        RecordingChanged?.Invoke(this, EventArgs.Empty);
    }
}
