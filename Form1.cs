namespace filedownloader;

public partial class Form1 : Form
{
    private const string AppName = "Fieldnote | File Downloader";
    private const string AppVersion = "1.0.0";
    private static readonly HttpClient HttpClient = new();
    private readonly TextBox _urlTextBox = new();
    private readonly TextBox _destinationTextBox = new();
    private readonly Button _downloadButton = new();
    private readonly Button _cancelButton = new();
    private readonly ProgressBar _progressBar = new();
    private readonly Label _statusLabel = new();
    private readonly Label _progressLabel = new();
    private CancellationTokenSource? _downloadCancellation;
    private bool _destinationChosen;

    public Form1()
    {
        InitializeComponent();
        Text = "Fieldnote | File Downloader";
        ClientSize = new Size(760, 500);
        MinimumSize = new Size(620, 460);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 247, 246);
        Font = new Font("Segoe UI", 10F);

        BuildInterface();
        FormClosing += (_, _) => _downloadCancellation?.Cancel();
    }

    private void BuildInterface()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 142,
            BackColor = Color.FromArgb(22, 75, 67),
            Padding = new Padding(34, 24, 34, 20)
        };

        var eyebrow = new Label
        {
            Text = "FIELDNOTE  /  TRANSFER",
            ForeColor = Color.FromArgb(173, 211, 194),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(34, 20)
        };
        var title = new Label
        {
            Text = "File downloader",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 23F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(31, 43)
        };
        var subtitle = new Label
        {
            Text = "Bring a file from the web straight to your computer.",
            ForeColor = Color.FromArgb(214, 230, 222),
            Font = new Font("Segoe UI", 10F),
            AutoSize = true,
            Location = new Point(34, 94)
        };
        header.Controls.AddRange([eyebrow, title, subtitle]);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(34, 26, 34, 28),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = BackColor
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var urlLabel = CreateSectionLabel("DOWNLOAD LINK");
        _urlTextBox.Dock = DockStyle.Fill;
        _urlTextBox.PlaceholderText = "https://example.com/archive.zip";
        _urlTextBox.BorderStyle = BorderStyle.FixedSingle;
        _urlTextBox.Font = new Font("Segoe UI", 11F);
        _urlTextBox.Margin = new Padding(0, 0, 0, 8);
        _urlTextBox.TextChanged += (_, _) => SuggestFileName();

        var saveLabel = CreateSectionLabel("SAVE TO");
        var destinationRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        destinationRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        destinationRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        _destinationTextBox.Dock = DockStyle.Fill;
        _destinationTextBox.ReadOnly = true;
        _destinationTextBox.BorderStyle = BorderStyle.FixedSingle;
        _destinationTextBox.BackColor = Color.White;
        _destinationTextBox.Margin = new Padding(0, 0, 10, 0);
        var downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        _destinationTextBox.Text = Path.Combine(downloadsFolder, "download");

        var browseButton = new Button
        {
            Text = "Choose file...",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(22, 75, 67),
            Cursor = Cursors.Hand,
            Margin = new Padding(0)
        };
        browseButton.FlatAppearance.BorderColor = Color.FromArgb(190, 204, 198);
        browseButton.Click += ChooseDestination;
        destinationRow.Controls.Add(_destinationTextBox, 0, 0);
        destinationRow.Controls.Add(browseButton, 1, 0);

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Margin = new Padding(0, 18, 0, 0)
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        _statusLabel.Text = "Ready when you are.";
        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.ForeColor = Color.FromArgb(70, 83, 78);
        _progressLabel.Text = "";
        _progressLabel.Dock = DockStyle.Fill;
        _progressLabel.TextAlign = ContentAlignment.MiddleRight;
        _progressLabel.ForeColor = Color.FromArgb(70, 83, 78);
        _progressLabel.Font = new Font("Segoe UI", 9F);
        _progressBar.Dock = DockStyle.Fill;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = 100;
        _progressBar.Style = ProgressBarStyle.Continuous;
        _progressBar.Margin = new Padding(0, 0, 0, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 0)
        };
        _downloadButton.Text = "Download";
        _downloadButton.Size = new Size(120, 38);
        _downloadButton.FlatStyle = FlatStyle.Flat;
        _downloadButton.FlatAppearance.BorderSize = 0;
        _downloadButton.BackColor = Color.FromArgb(22, 75, 67);
        _downloadButton.ForeColor = Color.White;
        _downloadButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _downloadButton.Cursor = Cursors.Hand;
        _downloadButton.Click += async (_, _) => await StartDownloadAsync();

        _cancelButton.Text = "Cancel";
        _cancelButton.Size = new Size(86, 38);
        _cancelButton.FlatStyle = FlatStyle.Flat;
        _cancelButton.BackColor = Color.White;
        _cancelButton.ForeColor = Color.FromArgb(74, 84, 80);
        _cancelButton.Cursor = Cursors.Hand;
        _cancelButton.Enabled = false;
        _cancelButton.Click += (_, _) => _downloadCancellation?.Cancel();
        actions.Controls.Add(_downloadButton);
        actions.Controls.Add(_cancelButton);

        bottom.Controls.Add(_statusLabel, 0, 0);
        bottom.Controls.Add(_progressLabel, 1, 0);
        bottom.Controls.Add(_progressBar, 0, 1);
        bottom.SetColumnSpan(_progressBar, 2);
        bottom.Controls.Add(actions, 0, 2);
        bottom.SetColumnSpan(actions, 2);

        content.Controls.Add(urlLabel, 0, 0);
        content.Controls.Add(_urlTextBox, 0, 1);
        content.Controls.Add(saveLabel, 0, 2);
        content.Controls.Add(destinationRow, 0, 3);
        content.Controls.Add(bottom, 0, 4);

        Controls.Add(content);
        Controls.Add(header);
    }

    private static Label CreateSectionLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        ForeColor = Color.FromArgb(22, 75, 67),
        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0)
    };

    private void ChooseDestination(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Choose download destination",
            FileName = Path.GetFileName(_destinationTextBox.Text),
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(_destinationTextBox.Text))
                ? Path.GetDirectoryName(_destinationTextBox.Text)
                : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _destinationTextBox.Text = dialog.FileName;
            _destinationChosen = true;
        }
    }

    private void SuggestFileName()
    {
        if (_destinationChosen || !Uri.TryCreate(_urlTextBox.Text.Trim(), UriKind.Absolute, out var uri))
        {
            return;
        }

        var fileName = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "download";
        }

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidCharacter, '_');
        }

        _destinationTextBox.Text = Path.Combine(
            Path.GetDirectoryName(_destinationTextBox.Text) ?? string.Empty, fileName);
    }

    private async Task StartDownloadAsync()
    {
        if (!Uri.TryCreate(_urlTextBox.Text.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            SetStatus("Enter a valid HTTP or HTTPS link.", Color.FromArgb(166, 69, 54));
            _urlTextBox.Focus();
            return;
        }

        var destination = _destinationTextBox.Text;
        var directory = Path.GetDirectoryName(destination);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            SetStatus("Choose a folder that exists on this computer.", Color.FromArgb(166, 69, 54));
            return;
        }

        if (File.Exists(destination) &&
            MessageBox.Show(this, "This file already exists. Replace it?", "Confirm overwrite",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        _downloadCancellation = new CancellationTokenSource();
        var token = _downloadCancellation.Token;
        _downloadButton.Enabled = false;
        _cancelButton.Enabled = true;
        _progressBar.Value = 0;
        _progressBar.Style = ProgressBarStyle.Continuous;
        _progressLabel.Text = "";
        SetStatus("Connecting...", Color.FromArgb(70, 83, 78));

        var temporaryFile = Path.Combine(directory, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.download");
        var completed = false;
        try
        {
            using var response = await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            long downloadedBytes = 0;
            await using var input = await response.Content.ReadAsStreamAsync(token);
            await using (var output = new FileStream(temporaryFile, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 81920, useAsync: true))
            {
                var buffer = new byte[81920];
                int bytesRead;

                SetStatus("Downloading...", Color.FromArgb(22, 75, 67));
                if (totalBytes is null)
                {
                    _progressBar.Style = ProgressBarStyle.Marquee;
                }

                while ((bytesRead = await input.ReadAsync(buffer, token)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                    downloadedBytes += bytesRead;

                    if (totalBytes is > 0)
                    {
                        _progressBar.Value = (int)Math.Clamp(downloadedBytes * 100d / totalBytes.Value, 0, 100);
                        _progressLabel.Text = $"{FormatBytes(downloadedBytes)} / {FormatBytes(totalBytes.Value)}";
                    }
                    else
                    {
                        _progressLabel.Text = FormatBytes(downloadedBytes);
                    }
                }
            }

            File.Move(temporaryFile, destination, overwrite: true);
            completed = true;
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 100;
            SetStatus("Download complete.", Color.FromArgb(22, 108, 78));
            _progressLabel.Text = FormatBytes(downloadedBytes);
        }
        catch (OperationCanceledException)
        {
            SetStatus("Download cancelled.", Color.FromArgb(122, 91, 48));
        }
        catch (HttpRequestException exception)
        {
            SetStatus($"Download failed: {exception.Message}", Color.FromArgb(166, 69, 54));
        }
        catch (IOException exception)
        {
            SetStatus($"Could not save the file: {exception.Message}", Color.FromArgb(166, 69, 54));
        }
        catch (UnauthorizedAccessException exception)
        {
            SetStatus($"Could not save the file: {exception.Message}", Color.FromArgb(166, 69, 54));
        }
        finally
        {
            if (!completed && File.Exists(temporaryFile))
            {
                try
                {
                    File.Delete(temporaryFile);
                }
                catch (IOException)
                {
                    SetStatus("Download stopped, but the partial file could not be removed.",
                        Color.FromArgb(166, 69, 54));
                }
            }

            _downloadCancellation?.Dispose();
            _downloadCancellation = null;
            _downloadButton.Enabled = true;
            _cancelButton.Enabled = false;
            if (_progressBar.Style == ProgressBarStyle.Marquee)
            {
                _progressBar.Style = ProgressBarStyle.Continuous;
            }
        }
    }

    private void SetStatus(string text, Color color)
    {
        _statusLabel.Text = text;
        _statusLabel.ForeColor = color;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}
