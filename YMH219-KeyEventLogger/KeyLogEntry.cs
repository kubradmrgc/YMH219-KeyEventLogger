namespace KeyEventLogger;

public enum KeyEventKind
{
    KeyDown,
    KeyUp,
    KeyPress
}

/// <summary>
/// Tek bir klavye olayının zaman, olay türü ve tuş bilgisini tutan kayıt.
/// </summary>
public sealed class KeyLogEntry
{
    public KeyLogEntry(DateTime timestamp, KeyEventKind kind, string keyName, string modifiers = "")
    {
        Timestamp = timestamp;
        Kind = kind;
        KeyName = keyName;
        Modifiers = modifiers;
    }

    public DateTime Timestamp { get; }
    public KeyEventKind Kind { get; }
    public string KeyName { get; }
    public string Modifiers { get; }

    public static KeyLogEntry FromKeyEvent(KeyEventKind kind, KeyEventArgs e, DateTime timestamp)
    {
        return new KeyLogEntry(timestamp, kind, DescribeKey(e.KeyCode), DescribeModifiers(e.KeyCode, e.Modifiers));
    }

    public static KeyLogEntry FromKeyPress(KeyPressEventArgs e, DateTime timestamp)
    {
        return new KeyLogEntry(timestamp, KeyEventKind.KeyPress, DescribeChar(e.KeyChar));
    }

    public override string ToString()
    {
        string text = $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff}  {Kind,-8}  {KeyName}";
        return Modifiers.Length == 0 ? text : $"{text}  [{Modifiers}]";
    }

    private static string DescribeKey(Keys key)
    {
        return key switch
        {
            >= Keys.D0 and <= Keys.D9 => ((char)('0' + (key - Keys.D0))).ToString(),
            Keys.ShiftKey => "Shift",
            Keys.ControlKey => "Ctrl",
            Keys.Menu => "Alt",
            Keys.Enter => "Enter",
            Keys.Back => "Backspace",
            Keys.CapsLock => "CapsLock",
            Keys.PageUp => "PageUp",
            Keys.PageDown => "PageDown",
            Keys.Escape => "Esc",
            _ => key.ToString()
        };
    }

    private static string DescribeChar(char c)
    {
        return c switch
        {
            ' ' => "Space",
            '\r' => "Enter",
            '\b' => "Backspace",
            '\t' => "Tab",
            '\u001b' => "Esc",
            _ when char.IsControl(c) => $"Kontrol karakteri 0x{(int)c:X2}",
            _ => $"'{c}'"
        };
    }

    private static string DescribeModifiers(Keys key, Keys modifiers)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(Keys.Control) && key != Keys.ControlKey)
        {
            parts.Add("Ctrl");
        }

        if (modifiers.HasFlag(Keys.Shift) && key != Keys.ShiftKey)
        {
            parts.Add("Shift");
        }

        if (modifiers.HasFlag(Keys.Alt) && key != Keys.Menu)
        {
            parts.Add("Alt");
        }

        return string.Join("+", parts);
    }
}
