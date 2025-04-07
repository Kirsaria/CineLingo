using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using CineLingo.Models;
using CineLingo.Data;
using CineLingo.Views;

namespace CineLingo.Page
{
    public partial class AdminCatalogPage : System.Windows.Controls.Page
    {
        private MovieService _movieService;

        public AdminCatalogPage()
        {
            InitializeComponent();
            _movieService = new MovieService(AuthWindow.ConnectionString);
            LoadMovies();
        }

        private void LoadMovies()
        {
            var allMovies = _movieService.LoadMovies();
            A1Movies.ItemsSource = allMovies.Where(m => m.Level_en == "A1").ToList();
            A2Movies.ItemsSource = allMovies.Where(m => m.Level_en == "A2").ToList();
            B1Movies.ItemsSource = allMovies.Where(m => m.Level_en == "B1").ToList();
            B2Movies.ItemsSource = allMovies.Where(m => m.Level_en == "B2").ToList();
            C1Movies.ItemsSource = allMovies.Where(m => m.Level_en == "C1").ToList();
            C2Movies.ItemsSource = allMovies.Where(m => m.Level_en == "C2").ToList();
        }

        private void AddMovieButton_Click(object sender, RoutedEventArgs e)
        {
            var level = (string)((Button)sender).Tag;
            var addMovieWindow = new AddMovieWindow(level);
            if (addMovieWindow.ShowDialog() == true)
            {
                if (_movieService.SaveMovieWithSubtitles(addMovieWindow.NewMovie, addMovieWindow.NewSubtitle, AuthWindow.CurrentUserId))
                {
                    LoadMovies();
                }
            }
        }

        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var grid = sender as Grid;
            var movie = grid.DataContext as Movie;

            if (movie != null)
            {
                ContextMenu contextMenu = new ContextMenu();

                MenuItem deleteMenuItem = new MenuItem { Header = "Удалить" };
                deleteMenuItem.Click += (s, args) => DeleteMovie(movie.Id);
                contextMenu.Items.Add(deleteMenuItem);

                MenuItem editMenuItem = new MenuItem { Header = "Изменить" };
                editMenuItem.Click += (s, args) => EditMovie(movie.Id);
                contextMenu.Items.Add(editMenuItem);

                grid.ContextMenu = contextMenu;
            }
        }

        private void DeleteMovie(int movieId)
        {
            if (_movieService.DeleteMovie(movieId))
            {
                LoadMovies();
            }
        }

        private void EditMovie(int movieId)
        {
            Movie movie = _movieService.GetMovieById(movieId);
            SubtitleInfo subtitle = _movieService.GetSubtitleByMovieId(movieId);

            var editMovieWindow = new AddMovieWindow(movie.Level_en, true, movieId)
            {
                TitleTextBox = { Text = movie.Title },
                DescriptionTextBox = { Text = movie.Description_movie },
                VideoUrlTextBox = { Text = movie.VideoUrl },
                PosterUrlTextBox = { Text = movie.PosterUrl },
                SubtitleLanguageTextBox = { Text = subtitle?.Language ?? string.Empty },
                SubtitleFileTextBox = { Text = subtitle?.FilePath ?? string.Empty }
            };

            if (editMovieWindow.ShowDialog() == true)
            {
                movie.Title = editMovieWindow.NewMovie.Title;
                movie.Description_movie = editMovieWindow.NewMovie.Description_movie;
                movie.VideoUrl = editMovieWindow.NewMovie.VideoUrl;
                movie.PosterUrl = editMovieWindow.NewMovie.PosterUrl;
                movie.Level_en = editMovieWindow.NewMovie.Level_en;

                if (subtitle != null)
                {
                    subtitle.Language = editMovieWindow.NewSubtitle.Language;
                    subtitle.FilePath = editMovieWindow.NewSubtitle.FilePath;
                    _movieService.UpdateSubtitle(movie.Id, subtitle);
                }
                else
                {
                    _movieService.SaveMovieWithSubtitles(movie, editMovieWindow.NewSubtitle, AuthWindow.CurrentUserId);
                }

                _movieService.UpdateMovie(movie);
                LoadMovies();
            }
        }
    }
}