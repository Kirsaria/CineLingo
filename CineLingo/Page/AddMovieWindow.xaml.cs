using System.Windows;
using Microsoft.Win32;
using CineLingo.Models;
using System.Windows.Controls;
using CineLingo.Page;
using MySql.Data.MySqlClient;
using System;
using System.Linq;

namespace CineLingo.Views
{
    public partial class AddMovieWindow : Window
    {
        public Movie NewMovie { get; private set; }
        public SubtitleInfo NewSubtitle { get; private set; }

        public AddMovieWindow(string level)
        {
            InitializeComponent();
            LevelComboBox.SelectedItem = LevelComboBox.Items
            .Cast<ComboBoxItem>()
            .FirstOrDefault(item => item.Content.ToString() == level);
        }

        private void BrowseSubtitleButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Файлы субтитров (*.srt)|*.srt",
                Title = "Выберите файл субтитров"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SubtitleFileTextBox.Text = openFileDialog.FileName;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название фильма!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (LevelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите уровень английского!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(VideoUrlTextBox.Text))
            {
                MessageBox.Show("Введите ссылку на видео!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SubtitleLanguageTextBox.Text) ||
                string.IsNullOrWhiteSpace(SubtitleFileTextBox.Text))
            {
                MessageBox.Show("Заполните информацию о субтитрах!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewMovie = new Movie
            {
                Title = TitleTextBox.Text,
                Description_movie = DescriptionTextBox.Text,
                VideoUrl = VideoUrlTextBox.Text,
                PosterUrl = PosterUrlTextBox.Text,
                Level_en = ((ComboBoxItem)LevelComboBox.SelectedItem).Content.ToString()
            };

            NewSubtitle = new SubtitleInfo
            {
                Language = SubtitleLanguageTextBox.Text,
                FilePath = SubtitleFileTextBox.Text
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool SaveMovieWithSubtitles(Movie movie, SubtitleInfo subtitle)
        {
            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var movieQuery = @"
                        INSERT INTO Movies 
                        (title, description_movie, video_url, poster_url, level_en, added_by) 
                        VALUES (@title, @description, @videoUrl, @posterUrl, @level, @userId);
                        SELECT LAST_INSERT_ID();";

                            int movieId;
                            using (var movieCommand = new MySqlCommand(movieQuery, connection, transaction))
                            {
                                movieCommand.Parameters.AddWithValue("@title", movie.Title);
                                movieCommand.Parameters.AddWithValue("@description", movie.Description_movie);
                                movieCommand.Parameters.AddWithValue("@videoUrl", movie.VideoUrl);
                                movieCommand.Parameters.AddWithValue("@posterUrl", movie.PosterUrl);
                                movieCommand.Parameters.AddWithValue("@level", movie.Level_en);
                                movieCommand.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);

                                movieId = Convert.ToInt32(movieCommand.ExecuteScalar());
                            }

                            var subtitleQuery = @"
                        INSERT INTO Subtitles 
                        (movie_id, language_sub, subtitle_file, added_by) 
                        VALUES (@movieId, @language, @subtitleFile, @userId)";

                            using (var subtitleCommand = new MySqlCommand(subtitleQuery, connection, transaction))
                            {
                                subtitleCommand.Parameters.AddWithValue("@movieId", movieId);
                                subtitleCommand.Parameters.AddWithValue("@language", subtitle.Language);
                                subtitleCommand.Parameters.AddWithValue("@subtitleFile", subtitle.FilePath);
                                subtitleCommand.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);

                                subtitleCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

    public class SubtitleInfo
    {
        public string Language { get; set; }
        public string FilePath { get; set; }
    }
}