using System.Windows;
using Microsoft.Win32;
using CineLingo.Models;
using CineLingo.Data;
using System.Windows.Controls;
using System;
using System.Linq;
using CineLingo.Page;

namespace CineLingo.Views
{
    public partial class AddMovieWindow : Window
    {
        public Movie NewMovie { get; private set; }
        public SubtitleInfo NewSubtitle { get; private set; }
        private bool _isEditMode;
        private int _existingMovieId;
        private MovieService _movieService;

        public AddMovieWindow(string level, bool isEditMode = false, int existingMovieId = -1)
        {
            InitializeComponent();
            LevelComboBox.SelectedItem = LevelComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == level);

            _isEditMode = isEditMode;
            _existingMovieId = existingMovieId;
            _movieService = new MovieService(AuthWindow.ConnectionString);

            if (_isEditMode)
            {
                Title = "Редактировать фильм";
                AddButton.Content = "Сохранить";
            }
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
            if (ValidateInputs())
            {
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

                if (_isEditMode)
                {
                    NewMovie.Id = _existingMovieId;
                    DialogResult = true;
                }
                else
                {
                    DialogResult = true;
                }
                Close();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название фильма!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (LevelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите уровень английского!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(VideoUrlTextBox.Text))
            {
                MessageBox.Show("Введите ссылку на видео!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(SubtitleLanguageTextBox.Text) ||
                string.IsNullOrWhiteSpace(SubtitleFileTextBox.Text))
            {
                MessageBox.Show("Заполните информацию о субтитрах!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}