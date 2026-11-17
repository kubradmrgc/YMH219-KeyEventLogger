namespace KeyEventLogger;

public class KeyEventRecorder
{
    private readonly List<KeyLogEntry> entries = new();
    private readonly HashSet<KeyEventKind> enabledKinds = new() { KeyEventKind.KeyDown };

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

    public bool IsKindEnabled(KeyEventKind kind)
    {
        return enabledKinds.Contains(kind);
    }

    public void SetKindEnabled(KeyEventKind kind, bool enabled)
    {
        if (enabled)
        {
            enabledKinds.Add(kind);
        }
        else
        {
            enabledKinds.Remove(kind);
        }
    }

    public bool TryRecord(KeyLogEntry entry)
    {
        if (!IsRecording || !IsKindEnabled(entry.Kind))
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
