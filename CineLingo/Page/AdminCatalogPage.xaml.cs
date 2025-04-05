using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using CineLingo.Models;
using CineLingo.Views;
using System;

namespace CineLingo.Page
{
    public partial class AdminCatalogPage : System.Windows.Controls.Page
    {
        public AdminCatalogPage()
        {
            InitializeComponent();
            LoadMovies();
        }

        private void LoadMovies()
        {
            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Movies";

                    var allMovies = new List<Movie>();
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allMovies.Add(new Movie
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description_movie = reader.GetString("description_movie"),
                                VideoUrl = reader.GetString("video_url"),
                                PosterUrl = reader.GetString("poster_url"),
                                Level_en = reader.GetString("level_en")
                            });
                        }
                    }

                    A1Movies.ItemsSource = allMovies.Where(m => m.Level_en == "A1").ToList();
                    A2Movies.ItemsSource = allMovies.Where(m => m.Level_en == "A2").ToList();
                    B1Movies.ItemsSource = allMovies.Where(m => m.Level_en == "B1").ToList();
                    B2Movies.ItemsSource = allMovies.Where(m => m.Level_en == "B2").ToList();
                    C1Movies.ItemsSource = allMovies.Where(m => m.Level_en == "C1").ToList();
                    C2Movies.ItemsSource = allMovies.Where(m => m.Level_en == "C2").ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильмов: {ex.Message}");
            }
        }

        private void AddMovieButton_Click(object sender, RoutedEventArgs e)
        {
            var level = (string)((Button)sender).Tag;
            var addMovieWindow = new AddMovieWindow(level);
            if (addMovieWindow.ShowDialog() == true)
            {
                if (SaveMovieWithSubtitles(addMovieWindow.NewMovie, addMovieWindow.NewSubtitle, AuthWindow.CurrentUserId))
                {
                    LoadMovies();
                }
            }
        }

        private bool SaveMovieWithSubtitles(Movie movie, SubtitleInfo subtitle, int userId)
        {
            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Movies (title, description_movie, video_url, poster_url, level_en, added_by) VALUES (@title, @description, @videoUrl, @posterUrl, @level, @addedBy)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@title", movie.Title);
                        command.Parameters.AddWithValue("@description", movie.Description_movie);
                        command.Parameters.AddWithValue("@videoUrl", movie.VideoUrl);
                        command.Parameters.AddWithValue("@posterUrl", movie.PosterUrl);
                        command.Parameters.AddWithValue("@level", movie.Level_en);
                        command.Parameters.AddWithValue("@addedBy", userId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0; 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении фильма: {ex.Message}");
                return false; // Возвращаем false в случае ошибки
            }
        }

        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var grid = sender as Grid;
            var movie = grid.DataContext as Movie; // Получаем объект фильма

            if (movie != null)
            {
                ContextMenu contextMenu = new ContextMenu();

                MenuItem deleteMenuItem = new MenuItem { Header = "Удалить" };
                deleteMenuItem.Click += (s, args) => DeleteMovie(movie.Id);
                contextMenu.Items.Add(deleteMenuItem);

                grid.ContextMenu = contextMenu; // Присваиваем контекстное меню
            }
        }
        private bool DeleteMovie(int movieId)
        {
            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Movies WHERE id = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", movieId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0; 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении фильма: {ex.Message}");
                return false; 
            }
        }
    }
}