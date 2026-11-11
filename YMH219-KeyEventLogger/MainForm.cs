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
    private readonly Label totalEventsLabel = CreateStatLabel();
    private readonly Label totalKeysLabel = CreateStatLabel();
    private readonly ListBox topKeysList = new()
    {
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", 10f),
        IntegralHeight = false
    };
    private readonly DateTimePicker fromPicker = CreateTimePicker(DateTime.Today);
    private readonly DateTimePicker toPicker = CreateTimePicker(DateTime.Today.AddDays(1).AddSeconds(-1));
    private readonly Button applyFilterButton = CreateButton("Filtrele");
    private readonly Button clearFilterButton = CreateButton("Filtreyi Kaldır");
    private readonly Label filterStateLabel = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
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

    private KeyLogFilter? activeFilter;

    public MainForm()
    {
        Text = BaseTitle;
        ClientSize = new Size(940, 660);
        MinimumSize = new Size(820, 600);
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

        var statsGroup = new GroupBox { Text = "İstatistik", Dock = DockStyle.Top, Height = 240, Padding = new Padding(8) };
        var topKeysTitle = new Label { Text = "En sık basılan 5 tuş:", Dock = DockStyle.Top, Height = 24, TextAlign = ContentAlignment.BottomLeft };
        statsGroup.Controls.Add(topKeysList);
        statsGroup.Controls.Add(topKeysTitle);
        statsGroup.Controls.Add(totalKeysLabel);
        statsGroup.Controls.Add(totalEventsLabel);

        var filterLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
        filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var filterButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        filterButtons.Controls.AddRange(new Control[] { applyFilterButton, clearFilterButton });
        filterLayout.Controls.Add(new Label { Text = "Başlangıç:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        filterLayout.Controls.Add(fromPicker, 1, 0);
        filterLayout.Controls.Add(new Label { Text = "Bitiş:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        filterLayout.Controls.Add(toPicker, 1, 1);
        filterLayout.Controls.Add(filterButtons, 0, 2);
        filterLayout.SetColumnSpan(filterButtons, 2);
        filterLayout.Controls.Add(filterStateLabel, 0, 3);
        filterLayout.SetColumnSpan(filterStateLabel, 2);

        var filterGroup = new GroupBox { Text = "Zaman Filtresi", Dock = DockStyle.Top, Height = 180, Padding = new Padding(8) };
        filterGroup.Controls.Add(filterLayout);

        var sidePanel = new Panel { Dock = DockStyle.Right, Width = 300, Padding = new Padding(0, 0, 8, 0) };
        sidePanel.Controls.Add(filterGroup);
        sidePanel.Controls.Add(statsGroup);

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
        Controls.Add(sidePanel);
        Controls.Add(inputGroup);
        Controls.Add(optionsBar);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);

        startButton.Click += (_, _) => { recorder.Start(); FocusInput(); };
        stopButton.Click += (_, _) => { recorder.Stop(); FocusInput(); };
        clearButton.Click += (_, _) => { recorder.Clear(); FocusInput(); };
        saveButton.Click += (_, _) => { SaveEntries(); FocusInput(); };
        applyFilterButton.Click += (_, _) => ApplyFilter();
        clearFilterButton.Click += (_, _) => { SetFilter(null); FocusInput(); };
        BindKindCheck(keyDownCheck, KeyEventKind.KeyDown);
        BindKindCheck(keyUpCheck, KeyEventKind.KeyUp);
        BindKindCheck(keyPressCheck, KeyEventKind.KeyPress);

        recorder.EntryAdded += Recorder_EntryAdded;
        recorder.EntriesCleared += (_, _) => RefreshLogList();
        recorder.RecordingChanged += (_, _) => UpdateRecordingState();

        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
        KeyPress += MainForm_KeyPress;
        Activated += (_, _) => UpdateFocusState(true);
        Deactivate += (_, _) => UpdateFocusState(false);
        Shown += (_, _) => FocusInput();

        UpdateRecordingState();
        UpdateFocusState(false);
        SetFilter(null);
    }

    private IReadOnlyList<KeyLogEntry> VisibleEntries =>
        activeFilter is null ? recorder.Entries : activeFilter.Apply(recorder.Entries).ToList();

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

    private void SaveEntries()
    {
        IReadOnlyCollection<KeyLogEntry> entries = VisibleEntries;
        if (entries.Count == 0)
        {
            MessageBox.Show(this, "Listede kaydedilecek kayıt yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Kayıtları kaydet",
            Filter = "Metin dosyası (*.txt)|*.txt",
            DefaultExt = "txt",
            AddExtension = true,
            FileName = LogFileWriter.SuggestFileName(DateTime.Now)
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            LogFileWriter.Save(dialog.FileName, entries);
            MessageBox.Show(this, $"{entries.Count} kayıt kaydedildi:\n{dialog.FileName}", "Kaydedildi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Dosya kaydedilemedi:\n{ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
        if (activeFilter is null || activeFilter.Matches(entry))
        {
            logList.Items.Add(entry);
            logList.TopIndex = logList.Items.Count - 1;
        }

        UpdateCount();
        UpdateStatistics();
    }

    private void ApplyFilter()
    {
        try
        {
            SetFilter(KeyLogFilter.FromSeconds(fromPicker.Value, toPicker.Value));
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, ex.Message, "Geçersiz aralık", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        FocusInput();
    }

    private void SetFilter(KeyLogFilter? filter)
    {
        activeFilter = filter;
        clearFilterButton.Enabled = filter is not null;
        filterStateLabel.Text = filter is null
            ? "Filtre kapalı - tüm kayıtlar"
            : $"Filtre açık: {filter.From:HH:mm:ss} - {filter.To:HH:mm:ss}";
        RefreshLogList();
    }

    private void RefreshLogList()
    {
        logList.BeginUpdate();
        logList.Items.Clear();
        foreach (KeyLogEntry entry in VisibleEntries)
        {
            logList.Items.Add(entry);
        }

        logList.EndUpdate();
        if (logList.Items.Count > 0)
        {
            logList.TopIndex = logList.Items.Count - 1;
        }

        UpdateCount();
        UpdateStatistics();
    }

    private void UpdateStatistics()
    {
        KeyStatistics stats = KeyStatistics.Calculate(VisibleEntries);
        totalEventsLabel.Text = $"Toplam olay: {stats.TotalEvents}";
        totalKeysLabel.Text = $"Basılan tuş (KeyDown): {stats.TotalKeyPresses}";

        topKeysList.BeginUpdate();
        topKeysList.Items.Clear();
        foreach (KeyCount key in stats.TopKeys)
        {
            topKeysList.Items.Add($"{key.KeyName,-12} {key.Count,5}");
        }

        topKeysList.EndUpdate();
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
        countLabel.Text = activeFilter is null
            ? $"Kayıt: {recorder.Entries.Count}"
            : $"Gösterilen: {logList.Items.Count} / {recorder.Entries.Count}";
    }

    private void FocusInput()
    {
        inputBox.Focus();
    }

    private static DateTimePicker CreateTimePicker(DateTime value)
    {
        return new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd.MM.yyyy HH:mm:ss",
            ShowUpDown = true,
            Value = value,
            Dock = DockStyle.Fill
        };
    }

    private static Label CreateStatLabel()
    {
        return new Label { Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
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
