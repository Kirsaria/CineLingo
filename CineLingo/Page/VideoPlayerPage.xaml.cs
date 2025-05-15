using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Threading;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using CineLingo.Models;
using CineLingo.Data;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using System.Windows.Media;

namespace CineLingo.Page
{
    public partial class VideoPlayerPage : System.Windows.Controls.Page
    {
        private class Subtitle
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public string Text { get; set; }
        }

        private bool IsPlaying = false;
        private bool IsUserDraggingSlider = false;
        private bool IsVolumeSliderFocused = false;
        private string _currentSubtitleFile;
        private List<Subtitle> Subtitles = new List<Subtitle>();
        private readonly DispatcherTimer Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
        private DispatcherTimer SubtitleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        private readonly DispatcherTimer PromptTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        private readonly OpenFileDialog MediaOpenDialog = new OpenFileDialog
        {
            Title = "Open a media file",
            Filter = "Media Files (*.mp3,*.mp4)|*.mp3;*.mp4"
        };

        private LibVLC _libVLC;
        private VlcMediaPlayer _mediaPlayer;
        private static readonly HttpClient _httpClient = new HttpClient();
        private ProgressManager _progressManager = new ProgressManager();
        string selectedFilePath;

        public VideoPlayerPage(int movieId)
        {
            InitializeComponent();
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new VlcMediaPlayer(_libVLC);
            Player.MediaPlayer = _mediaPlayer;
            PromptTimer.Tick += PromptTimer_Tick;
            VolumeSlider.GotFocus += (s, e) => IsVolumeSliderFocused = true;
            VolumeSlider.LostFocus += (s, e) => IsVolumeSliderFocused = false;

            SetupMediaPlayerEvents();
            SetupTimers();
            this.KeyDown += VideoPlayerPage_KeyDown;
            this.Unloaded += VideoPlayerPage_Unloaded;
            LoadMovie(movieId);
        }

        private void SetupMediaPlayerEvents()
        {
            _mediaPlayer.TimeChanged += (s, e) => Dispatcher.Invoke(UpdateProgress);
            _mediaPlayer.LengthChanged += (s, e) => Dispatcher.Invoke(() => ProgressSlider.Maximum = _mediaPlayer.Length / 1000.0);
            _mediaPlayer.EndReached += (s, e) => Dispatcher.Invoke(() => { SaveProgress(); IsPlaying = false; });
        }

        private void SetupTimers()
        {
            Timer.Tick += Timer_Tick;
            SubtitleTimer.Tick += SubtitleTimer_Tick;
            Timer.Start();
            SubtitleTimer.Start();
        }
        private void PromptTimer_Tick(object sender, EventArgs e)
        {
            PromptTimer.Stop();
            if (AuthWindow.CurrentUserId == 0 || string.IsNullOrWhiteSpace(selectedFilePath))
                return;

            var savedPos = _progressManager.GetSavedPosition(AuthWindow.CurrentUserId.ToString(), selectedFilePath);
            if (savedPos.HasValue && savedPos.Value.TotalSeconds > 0)
            {
                var result = MessageBox.Show(
                    $"Вы хотите перемотать к месту, где остановились в прошлый раз ({savedPos.Value:hh\\:mm\\:ss})?",
                    "Перемотка",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _mediaPlayer.Time = (long)savedPos.Value.TotalMilliseconds;
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_mediaPlayer.Media == null) return;

            if (IsVolumeSliderFocused)
            {
                if (Keyboard.IsKeyDown(Key.Left))
                    VolumeSlider.Value = Math.Max(VolumeSlider.Minimum, VolumeSlider.Value - 0.05);
                else if (Keyboard.IsKeyDown(Key.Right))
                    VolumeSlider.Value = Math.Min(VolumeSlider.Maximum, VolumeSlider.Value + 0.05);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.Left)) Rewind(-5000);
                else if (Keyboard.IsKeyDown(Key.Right)) Rewind(5000);
            }
        }

        private void SubtitleTimer_Tick(object sender, EventArgs e)
        {
            if (_mediaPlayer.Media != null)
                UpdateSubtitles(TimeSpan.FromMilliseconds(_mediaPlayer.Time));
        }

        private void Rewind(long milliseconds)
        {
            var newTime = Math.Min(Math.Max(_mediaPlayer.Time + milliseconds, 0), _mediaPlayer.Length);
            _mediaPlayer.Time = newTime;
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (!IsUserDraggingSlider)
            {
                ProgressSlider.Value = _mediaPlayer.Time / 1000.0;
                StatusLbl.Text = TimeSpan.FromMilliseconds(_mediaPlayer.Time).ToString(@"hh\:mm\:ss");
            }
        }

        private void LoadMovie(int movieId)
        {
            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    connection.Open();
                    var query = @"SELECT m.video_url, s.subtitle_file FROM Movies m LEFT JOIN Subtitles s ON m.id = s.movie_id WHERE m.id = @movieId LIMIT 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@movieId", movieId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string videoUrl = reader["video_url"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(videoUrl))
                                {
                                    _mediaPlayer.Media = new Media(_libVLC, new Uri(videoUrl));
                                    _mediaPlayer.Play();
                                    IsPlaying = true;
                                }

                                string subtitlePath = reader["subtitle_file"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(subtitlePath))
                                    LoadSubtitles(subtitlePath);

                                if (AuthWindow.CurrentUserId > 0)
                                    LoadWordsForCurrentSubtitles();
                            }
                            else MessageBox.Show("Фильм не найден.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильма: {ex.Message}");
                Debug.WriteLine(ex);
            }
        }

        private void SaveProgress()
        {
            if (_mediaPlayer.Media == null) return;
            string username = AuthWindow.CurrentUserId.ToString();
            var currentTime = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
            _progressManager.SaveUserProgress(username, selectedFilePath, currentTime);
        }

        private void VideoPlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveProgress();
        }

        private void LoadSubtitles(string subtitleFilePath)
        {
            _currentSubtitleFile = Path.GetFileName(subtitleFilePath);
            Subtitles.Clear();
            LoadWordsForCurrentSubtitles();

            var lines = File.ReadAllLines(subtitleFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                if (int.TryParse(lines[i], out _))
                {
                    i++;
                    var timeParts = lines[i].Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);
                    if (timeParts.Length == 2 && TimeSpan.TryParse(timeParts[0].Trim(), out var startTime) && TimeSpan.TryParse(timeParts[1].Trim(), out var endTime))
                    {
                        i++;
                        var sb = new StringBuilder();
                        while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                            sb.AppendLine(lines[i++]);
                        Subtitles.Add(new Subtitle { StartTime = startTime, EndTime = endTime, Text = sb.ToString().Trim() });
                    }
                }
            }
        }

        private void UpdateSubtitles(TimeSpan current)
        {
            var subtitle = Subtitles.FirstOrDefault(s => s.StartTime <= current && s.EndTime >= current);
            SubtitlesTextBox.Text = subtitle != null ? RemoveTags(subtitle.Text) : string.Empty;
        }

        private string RemoveTags(string text) => System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", string.Empty);

        private async void SubtitlesTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SubtitlesTextBox.SelectedText)) return;
            if (IsPlaying) { _mediaPlayer.Pause(); IsPlaying = false; }
            var selected = SubtitlesTextBox.SelectedText.Length > 500 ? SubtitlesTextBox.SelectedText.Substring(0, 500) + "..." : SubtitlesTextBox.SelectedText;
            TranslationLabel.Content = "Перевод...";
            var translation = await TranslateTextAsync(selected);
            TranslationLabel.Content = $"Перевод:\n{translation}";
        }

        private async Task<string> TranslateTextAsync(string text, string lang = "ru")
        {
            try
            {
                var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=en|{lang}";
                var resp = await _httpClient.GetStringAsync(url);
                dynamic json = JsonConvert.DeserializeObject(resp);
                return json.responseData.translatedText;
            }
            catch { return "Ошибка перевода"; }
        }

        private void VideoPlayerPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
                e.Handled = true;
        }

        private void OpenSubtitles_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Title = "Открыть субтитры", Filter = "Subtitle Files (*.srt)|*.srt" };
            if (dlg.ShowDialog() == true)
                LoadSubtitles(dlg.FileName);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (MediaOpenDialog.ShowDialog() == true)
            {
                selectedFilePath = MediaOpenDialog.FileName;
                _mediaPlayer.Media = new Media(_libVLC, new Uri(MediaOpenDialog.FileName));
                TitleLbl.Content = Path.GetFileName(MediaOpenDialog.FileName);
                _mediaPlayer.Play();
                IsPlaying = true;
                PromptTimer.Start();
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer.Media != null) { _mediaPlayer.Play(); IsPlaying = true; }
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e) => _mediaPlayer.Pause();

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Volume = (int)(VolumeSlider.Value * 100);
        }

        private void ProgressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) => IsUserDraggingSlider = true;

        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            IsUserDraggingSlider = false;
            _mediaPlayer.Time = (long)(ProgressSlider.Value * 1000);
            if (IsPlaying) _mediaPlayer.Play();
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsUserDraggingSlider)
                StatusLbl.Text = TimeSpan.FromSeconds(ProgressSlider.Value).ToString(@"hh\:mm\:ss");
        }
        private async void SaveToDictionaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SubtitlesTextBox.SelectedText)) return;
            var word = SubtitlesTextBox.SelectedText;
            var sentence = GetCurrentFullSentence();
            if (string.IsNullOrEmpty(sentence)) { MessageBox.Show("Не удалось определить контекст."); return; }
            await SaveWord(word, sentence);
        }

        private string GetCurrentFullSentence()
        {
            var time = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
            var sub = Subtitles.FirstOrDefault(s => s.StartTime <= time && s.EndTime >= time);
            return sub?.Text;
        }

        private async Task SaveWord(string wordOrPhrase, string fullSentence)
        {
            if (AuthWindow.CurrentUserId == 0)
            {
                MessageBox.Show("Пожалуйста, войдите в систему, чтобы сохранять слова");
                return;
            }

            wordOrPhrase = System.Text.RegularExpressions.Regex.Replace(wordOrPhrase, @"[^a-zA-Zа-яА-ЯёЁ\s]", "").Trim();
            if (string.IsNullOrWhiteSpace(wordOrPhrase))
            {
                MessageBox.Show("Выбранный текст не содержит допустимых слов для сохранения.");
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    await conn.OpenAsync();

                    // Проверка, существует ли уже такое слово у текущего пользователя с этим subtitle-файлом
                    string checkQuery = @"SELECT COUNT(*) FROM DictionaryItem 
                                  WHERE userId = @userId AND WordOrPhrase = @word";
                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);
                        checkCmd.Parameters.AddWithValue("@word", wordOrPhrase);

                        var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            MessageBox.Show("Это слово уже добавлено в ваш словарь.");
                            return;
                        }
                    }

                    var translation = await TranslateTextAsync(wordOrPhrase);
                    if (string.IsNullOrEmpty(translation))
                    {
                        MessageBox.Show("Не удалось получить перевод");
                        return;
                    }

                    string insertQuery = @"INSERT INTO DictionaryItem (userId, WordOrPhrase, fullsentence, translation, subtitleFile)
                                   VALUES (@userId, @word, @sentence, @translation, @subtitle)";
                    using (var cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);
                        cmd.Parameters.AddWithValue("@word", wordOrPhrase);
                        cmd.Parameters.AddWithValue("@sentence", fullSentence);
                        cmd.Parameters.AddWithValue("@translation", translation);
                        cmd.Parameters.AddWithValue("@subtitle", _currentSubtitleFile);

                        if (await cmd.ExecuteNonQueryAsync() > 0)
                        {
                            MessageBox.Show("Слово сохранено в словарь");
                            LoadWordsForCurrentSubtitles();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }


        private async void LoadWordsForCurrentSubtitles()
        {
            if (AuthWindow.CurrentUserId == 0 || string.IsNullOrEmpty(_currentSubtitleFile)) return;
            try
            {
                using (var conn = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    await conn.OpenAsync();
                    var query = @"SELECT WordOrPhrase, translation FROM DictionaryItem WHERE userId = @userId AND subtitleFile = @currentFile ORDER BY addedDate DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);
                        cmd.Parameters.AddWithValue("@currentFile", _currentSubtitleFile);
                        var items = new List<DictionaryItem>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                items.Add(new DictionaryItem { WordOrPhrase = reader["WordOrPhrase"].ToString(), Translation = reader["translation"].ToString() });
                        }
                        DictionaryList.ItemsSource = items;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки слов: {ex.Message}");
            }
        }

        private void SubtitlesTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SubtitlesTextBox.SelectedText)) e.Handled = true;
        }
        private bool isFullscreen = false;
        private void FullscreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isFullscreen)
            {
                TitleLbl.Visibility = Visibility.Collapsed;
                SubtitlePanel.Visibility = Visibility.Collapsed;
                DictionaryPanel.Visibility = Visibility.Collapsed;

                Grid.SetRowSpan(PlayerContainer, 3);         
                Grid.SetColumnSpan(PlayerContainer, 2);     
                PlayerContainer.Height = Double.NaN;

                isFullscreen = true;
            }
            else
            {
                TitleLbl.Visibility = Visibility.Visible;
                SubtitlePanel.Visibility = Visibility.Visible;
                DictionaryPanel.Visibility = Visibility.Visible;

                Grid.SetRowSpan(PlayerContainer, 1);         
                Grid.SetColumnSpan(PlayerContainer, 1);     
                PlayerContainer.Height = 400;

                isFullscreen = false;
            }
        }

    }
}