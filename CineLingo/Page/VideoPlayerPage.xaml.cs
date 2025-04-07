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
using System.Windows.Media;
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

        private readonly DispatcherTimer Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
        private DispatcherTimer _rewindTimer;

        private List<Subtitle> Subtitles = new List<Subtitle>();
        private DispatcherTimer SubtitleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        private string _currentSubtitleFile;

        private readonly OpenFileDialog MediaOpenDialog = new OpenFileDialog
        {
            Title = "Open a media file",
            Filter = "Media Files (*.mp3,*.mp4)|*.mp3;*.mp4"
        };
        private void OpenSubtitles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open subtitles file",
                Filter = "Subtitle Files (*.srt)|*.srt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadSubtitles(openFileDialog.FileName);
            }
        }

        public VideoPlayerPage(int movieId)
        {
            InitializeComponent();
            Timer.Tick += Timer_Tick;
            Timer.Start();
            this.KeyDown += VideoPlayerPage_KeyDown;
            _rewindTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _rewindTimer.Tick += RewindTimer_Tick;
            Player.MediaOpened += Player_MediaOpened;
            SubtitleTimer.Tick += SubtitleTimer_Tick;
            SubtitleTimer.Start();
            LoadMovie(movieId);
        }
        private void LoadMovie(int movieId)
        {
            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    connection.Open();
                    var query = @"SELECT m.video_url, s.subtitle_file 
                          FROM Movies m
                          LEFT JOIN Subtitles s ON m.id = s.movie_id
                          WHERE m.id = @movieId
                          LIMIT 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@movieId", movieId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string videoUrl = reader.GetString("video_url");
                                Player.Source = new Uri(videoUrl);
                                Player.Play();
                                IsPlaying = true;

                                string subtitlePath = reader.GetString("subtitle_file");
                                LoadSubtitles(subtitlePath);
                            }
                            else
                            {
                                MessageBox.Show("Фильм не найден.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильма: {ex.Message}");
                Debug.WriteLine($"Ошибка загрузки фильма: {ex}");
            }
        }

        private void SubtitleTimer_Tick(object sender, EventArgs e)
        {
            if (Player.Source != null && Player.NaturalDuration.HasTimeSpan)
            {
                var currentTime = Player.Position;
                UpdateSubtitles(currentTime);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(Player.Source != null && Player.NaturalDuration.HasTimeSpan && !IsUserDraggingSlider)
            {
                ProgressSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
                ProgressSlider.Value = Player.Position.TotalSeconds;
            }
        }
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (MediaOpenDialog.ShowDialog() == true)
            {
                Player.Source = new Uri(MediaOpenDialog.FileName);
                TitleLbl.Content = Path.GetFileName(MediaOpenDialog.FileName);

                Player.Play();
                IsPlaying = true;
            }
        }
        #region Media Controls

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Player?.Source != null)
            {
                Player.Play();
                IsPlaying = true;
            }
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
                Player.Pause();
        }

        private void ProgressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            IsUserDraggingSlider = true;
        }

        private async void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            IsUserDraggingSlider = false;
            Player.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
            if (IsPlaying)
            {
                await Task.Delay(100); 
                Player.Play();
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsUserDraggingSlider)
            {
                StatusLbl.Text = TimeSpan.FromSeconds(ProgressSlider.Value).ToString(@"hh\:mm\:ss");
            }
        }
        #endregion
        private void VideoPlayerPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (Player.Source == null || !Player.NaturalDuration.HasTimeSpan)
                return;

            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                _rewindTimer.Start();
                e.Handled = true; 
            }
        }
        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            ProgressSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
        }
        private void RewindTimer_Tick(object sender, EventArgs e)
        {
            _rewindTimer.Stop(); 
            ApplyRewind();
        }
        private void ApplyRewind()
        {
            try
            {

                TimeSpan currentPosition = Player.Position;

                if (Keyboard.IsKeyDown(Key.Left))
                {
                    TimeSpan newPosition = currentPosition.Subtract(TimeSpan.FromSeconds(5));
                    Player.Position = newPosition < TimeSpan.Zero ? TimeSpan.Zero : newPosition;
                }

                if (Keyboard.IsKeyDown(Key.Right))
                {
                    TimeSpan newPosition = currentPosition.Add(TimeSpan.FromSeconds(5));
                    Player.Position = newPosition > Player.NaturalDuration.TimeSpan ? Player.NaturalDuration.TimeSpan : newPosition;
                }

                ProgressSlider.Value = Player.Position.TotalSeconds;
                StatusLbl.Text = Player.Position.ToString(@"hh\:mm\:ss");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during rewind: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadSubtitles(string subtitleFilePath)
        {
            _currentSubtitleFile = Path.GetFileName(subtitleFilePath);
            Subtitles.Clear();
            LoadWordsForCurrentSubtitles();
            var lines = File.ReadAllLines(subtitleFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;
                if (int.TryParse(lines[i], out int subtitleNumber))
                {
                    i++;

                    var timeParts = lines[i].Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);
                    if (timeParts.Length == 2 && TimeSpan.TryParse(timeParts[0].Trim(), out var startTime) && TimeSpan.TryParse(timeParts[1].Trim(), out var endTime))
                    {
                        i++;

                        var subtitleText = new StringBuilder();
                        while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                        {
                            subtitleText.AppendLine(lines[i]);
                            i++;
                        }
                        string textWithTags = subtitleText.ToString().Trim();

                        Subtitles.Add(new Subtitle
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            Text = textWithTags
                        });
                    }
                }
            }
        }
        private string RemoveTags(string text)
        {
            string cleanedText = System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]*>", string.Empty);
            return cleanedText;
        }
        private void UpdateSubtitles(TimeSpan currentTime)
        {
            if (Player.Source != null && Player.NaturalDuration.HasTimeSpan)
            {
                var currentSubtitle = Subtitles.FirstOrDefault(s => s.StartTime <= currentTime && s.EndTime >= currentTime);
                if (currentSubtitle != null)
                {
                    string cleanText = RemoveTags(currentSubtitle.Text);
                    if (SubtitlesTextBox.Text != cleanText)
                    {
                        SubtitlesTextBox.Text = cleanText;
                    }
                }
                else
                {
                    SubtitlesTextBox.Text = string.Empty;
                }
            }
        }
        private async void SubtitlesTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || string.IsNullOrEmpty(textBox.SelectedText)) return;

            if (IsPlaying)
            {
                Player.Pause();
                IsPlaying = false;
            }

            string selectedText = textBox.SelectedText.Length > 500
                ? textBox.SelectedText.Substring(0, 500) + "..."
                : textBox.SelectedText;

            try
            {
                TranslationLabel.Content = "Перевод...";

                string translatedText = await TranslateTextAsync(selectedText);

                if (!string.IsNullOrEmpty(translatedText))
                {
                    TranslationLabel.Content = $"Перевод:\n{translatedText}";
                }
            }
            catch (Exception ex)
            {
                TranslationLabel.Content = "Ошибка перевода";
                Debug.WriteLine($"Translation error: {ex.Message}");
            }
            CommandManager.InvalidateRequerySuggested();
        }
        private static readonly HttpClient _httpClient = new HttpClient();
        private async Task<string> TranslateTextAsync(string text, string targetLang = "ru")
        {
            try
            {
                string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=en|ru";
                var response = await _httpClient.GetStringAsync(url);
                dynamic json = JsonConvert.DeserializeObject(response);
                return json.responseData.translatedText;
            }
            catch
            {
                return "Ошибка перевода";
            }
        }
        private void SubtitlesTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SubtitlesTextBox.SelectedText))
            {
                e.Handled = true;
            }
        }
        private async void SaveToDictionaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SubtitlesTextBox.SelectedText))
                return;

            string selectedText = SubtitlesTextBox.SelectedText;
            string fullSentence = GetCurrentFullSentence();

            if (string.IsNullOrEmpty(fullSentence))
            {
                MessageBox.Show("Не удалось определить контекст предложения");
                return;
            }

            await SaveToDictionary(selectedText, fullSentence);
        }
        private string GetCurrentFullSentence()
        {
            if (Player.Source == null || !Player.NaturalDuration.HasTimeSpan)
                return null;

            var currentTime = Player.Position;
            var currentSubtitle = Subtitles.FirstOrDefault(s => s.StartTime <= currentTime && s.EndTime >= currentTime);

            return currentSubtitle?.Text;
        }
        private async Task SaveToDictionary(string wordOrPhrase, string fullSentence)
        {
            if (AuthWindow.CurrentUserId == 0)
            {
                MessageBox.Show("Пожалуйста, войдите в систему, чтобы сохранять слова");
                return;
            }

            string translation = await TranslateTextAsync(wordOrPhrase);

            if (string.IsNullOrEmpty(translation))
            {
                MessageBox.Show("Не удалось получить перевод");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO DictionaryItem 
                          (userId, WordOrPhrase, fullsentence, translation, subtitleFile) 
                          VALUES (@userId, @word, @sentence, @translation, @subtitle)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);
                        command.Parameters.AddWithValue("@word", wordOrPhrase);
                        command.Parameters.AddWithValue("@sentence", fullSentence);
                        command.Parameters.AddWithValue("@translation", translation);
                        command.Parameters.AddWithValue("@subtitle", _currentSubtitleFile);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
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
            if (AuthWindow.CurrentUserId == 0 || string.IsNullOrEmpty(_currentSubtitleFile))
                return;

            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT WordOrPhrase, translation 
                              FROM DictionaryItem 
                              WHERE userId = @userId AND subtitleFile = @currentFile
                              ORDER BY addedDate DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);
                        command.Parameters.AddWithValue("@currentFile", _currentSubtitleFile);

                        var items = new List<DictionaryItem>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new DictionaryItem
                                {
                                    WordOrPhrase = reader["WordOrPhrase"].ToString(),
                                    Translation = reader["translation"].ToString()
                                });
                            }
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

    }
}
