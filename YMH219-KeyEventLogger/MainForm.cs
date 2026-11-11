namespace KeyEventLogger;

/// <summary>
/// Ana pencere: kayıt kontrolleri, olay listesi ve tuşların deneneceği yazı alanı.
/// Tuşlar yalnızca bu pencere aktifken, formun KeyPreview özelliği ile yakalanır.
/// </summary>
public class MainForm : Form
{
    private const string BaseTitle = "YMH219 - Key Event Logger";

    private readonly KeyEventRecorder recorder = new();

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
        Margin = new Padding(16, 3, 3, 3)
    };
    private readonly CheckBox keyDownCheck = CreateKindCheck("KeyDown");
    private readonly CheckBox keyUpCheck = CreateKindCheck("KeyUp");
    private readonly CheckBox keyPressCheck = CreateKindCheck("KeyPress");
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
    private readonly ToolStripStatusLabel focusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel countLabel = new();

    public MainForm()
    {
        Text = BaseTitle;
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

        var optionsBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(8, 0, 8, 4)
        };
        var optionsLabel = new Label { Text = "Kaydedilecek olaylar:", AutoSize = true, Margin = new Padding(3, 6, 8, 3) };
        optionsBar.Controls.AddRange(new Control[] { optionsLabel, keyDownCheck, keyUpCheck, keyPressCheck });
        keyDownCheck.Checked = recorder.IsKindEnabled(KeyEventKind.KeyDown);
        keyUpCheck.Checked = recorder.IsKindEnabled(KeyEventKind.KeyUp);
        keyPressCheck.Checked = recorder.IsKindEnabled(KeyEventKind.KeyPress);

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
        Controls.Add(optionsBar);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);

        startButton.Click += (_, _) => { recorder.Start(); FocusInput(); };
        stopButton.Click += (_, _) => { recorder.Stop(); FocusInput(); };
        clearButton.Click += (_, _) => { recorder.Clear(); FocusInput(); };
        BindKindCheck(keyDownCheck, KeyEventKind.KeyDown);
        BindKindCheck(keyUpCheck, KeyEventKind.KeyUp);
        BindKindCheck(keyPressCheck, KeyEventKind.KeyPress);

        recorder.EntryAdded += Recorder_EntryAdded;
        recorder.EntriesCleared += (_, _) => { logList.Items.Clear(); UpdateCount(); };
        recorder.RecordingChanged += (_, _) => UpdateRecordingState();

        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
        KeyPress += MainForm_KeyPress;
        Activated += (_, _) => UpdateFocusState(true);
        Deactivate += (_, _) => UpdateFocusState(false);
        Shown += (_, _) => FocusInput();

        UpdateRecordingState();
        UpdateFocusState(false);
        UpdateCount();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        recorder.TryRecord(KeyLogEntry.FromKeyEvent(KeyEventKind.KeyDown, e, DateTime.Now));
    }

    private void MainForm_KeyUp(object? sender, KeyEventArgs e)
    {
        recorder.TryRecord(KeyLogEntry.FromKeyEvent(KeyEventKind.KeyUp, e, DateTime.Now));
    }

    private void MainForm_KeyPress(object? sender, KeyPressEventArgs e)
    {
        recorder.TryRecord(KeyLogEntry.FromKeyPress(e, DateTime.Now));
    }

    private void BindKindCheck(CheckBox check, KeyEventKind kind)
    {
        check.CheckedChanged += (_, _) =>
        {
            recorder.SetKindEnabled(kind, check.Checked);
            FocusInput();
        };
    }

    private void Recorder_EntryAdded(object? sender, KeyLogEntry entry)
    {
        logList.Items.Add(entry);
        logList.TopIndex = logList.Items.Count - 1;
        UpdateCount();
    }

    private void UpdateRecordingState()
    {
        bool recording = recorder.IsRecording;
        startButton.Enabled = !recording;
        stopButton.Enabled = recording;
        recordingIndicator.Text = recording ? "● KAYIT AÇIK" : "■ KAYIT KAPALI";
        recordingIndicator.BackColor = recording ? Color.FromArgb(220, 38, 38) : Color.DimGray;
        Text = recording ? $"{BaseTitle} [KAYIT AÇIK]" : BaseTitle;
    }

    private void UpdateFocusState(bool active)
    {
        focusLabel.Text = active
            ? "Pencere aktif - tuşlar yakalanabilir"
            : "Pencere aktif değil - tuşlar yakalanmıyor";
    }

    private void UpdateCount()
    {
        countLabel.Text = $"Kayıt: {recorder.Entries.Count}";
    }

    private void FocusInput()
    {
        inputBox.Focus();
    }

    private static CheckBox CreateKindCheck(string text)
    {
        return new CheckBox { Text = text, AutoSize = true, Margin = new Padding(3, 4, 12, 3) };
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
