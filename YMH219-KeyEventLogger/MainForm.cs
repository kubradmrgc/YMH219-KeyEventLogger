namespace KeyEventLogger;

/// <summary>
/// Ana pencere: kayıt kontrolleri, olay listesi ve tuşların deneneceği yazı alanı.
/// </summary>
public class MainForm : Form
{
    private readonly Button startButton = CreateButton("Başlat");
    private readonly Button stopButton = CreateButton("Durdur");
    private readonly Button clearButton = CreateButton("Temizle");
    private readonly Button saveButton = CreateButton("Kaydet (.txt)");
    private readonly Label recordingIndicator = new()
    {
        AutoSize = false,
        Size = new Size(170, 32),
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
        ForeColor = Color.White,
        BackColor = Color.DimGray,
        Text = "■ KAYIT KAPALI",
        Margin = new Padding(16, 3, 3, 3)
    };
    private readonly ListBox logList = new()
    {
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", 10f),
        IntegralHeight = false,
        HorizontalScrollbar = true
    };
    private readonly TextBox inputBox = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        AcceptsReturn = true,
        AcceptsTab = true,
        ScrollBars = ScrollBars.Vertical,
        Font = new Font("Segoe UI", 11f)
    };
    private readonly StatusStrip statusStrip = new();
    private readonly ToolStripStatusLabel focusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Hazır" };
    private readonly ToolStripStatusLabel countLabel = new() { Text = "Kayıt: 0" };

    public MainForm()
    {
        Text = "YMH219 - Key Event Logger";
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(8, 8, 8, 4)
        };
        toolbar.Controls.AddRange(new Control[] { startButton, stopButton, clearButton, saveButton, recordingIndicator });

        var logGroup = new GroupBox { Text = "Kayıtlar", Dock = DockStyle.Fill, Padding = new Padding(8) };
        logGroup.Controls.Add(logList);

        var inputGroup = new GroupBox
        {
            Text = "Deneme alanı - tuşlar yalnızca bu pencere aktifken yakalanır",
            Dock = DockStyle.Bottom,
            Height = 120,
            Padding = new Padding(8)
        };
        inputGroup.Controls.Add(inputBox);

        statusStrip.Items.AddRange(new ToolStripItem[] { focusLabel, countLabel });

        Controls.Add(logGroup);
        Controls.Add(inputGroup);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);

        KeyDown += MainForm_KeyDown;
        Shown += (_, _) => inputBox.Focus();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        AddEntry(KeyLogEntry.FromKeyEvent(KeyEventKind.KeyDown, e, DateTime.Now));
    }

    private void AddEntry(KeyLogEntry entry)
    {
        logList.Items.Add(entry);
        logList.TopIndex = logList.Items.Count - 1;
        countLabel.Text = $"Kayıt: {logList.Items.Count}";
    }

    private static Button CreateButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(100, 32),
            Margin = new Padding(3)
        };
    }
}
