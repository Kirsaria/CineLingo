using MySql.Data.MySqlClient;
using CineLingo.Models;
using System.Collections.Generic;
using System.Windows;
using System;

namespace CineLingo.Data
{
    internal class MovieService
    {
        private string _connectionString;

        public MovieService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Movie> LoadMovies()
        {
            var allMovies = new List<Movie>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Movies";

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильмов: {ex.Message}");
            }

            return allMovies;
        }

        public bool SaveMovieWithSubtitles(Movie movie, SubtitleInfo subtitle, int userId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        int movieId = AddMovie(movie, userId, connection, transaction);
                        AddSubtitle(movieId, subtitle, userId, connection, transaction);
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private int AddMovie(Movie movie, int userId, MySqlConnection connection, MySqlTransaction transaction)
        {
            var movieQuery = @"
                INSERT INTO Movies (title, description_movie, video_url, poster_url, level_en, added_by) 
                VALUES (@title, @description, @videoUrl, @posterUrl, @level, @addedBy);
                SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(movieQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@title", movie.Title);
                command.Parameters.AddWithValue("@description", movie.Description_movie);
                command.Parameters.AddWithValue("@videoUrl", movie.VideoUrl);
                command.Parameters.AddWithValue("@posterUrl", movie.PosterUrl);
                command.Parameters.AddWithValue("@level", movie.Level_en);
                command.Parameters.AddWithValue("@addedBy", userId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private void AddSubtitle(int movieId, SubtitleInfo subtitle, int userId, MySqlConnection connection, MySqlTransaction transaction)
        {
            var subtitleQuery = @"
                INSERT INTO Subtitles (movie_id, language_sub, subtitle_file, added_by) 
                VALUES (@movieId, @language, @subtitleFile, @addedBy)";

            using (var command = new MySqlCommand(subtitleQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@movieId", movieId);
                command.Parameters.AddWithValue("@language", subtitle.Language);
                command.Parameters.AddWithValue("@subtitleFile", subtitle.FilePath);
                command.Parameters.AddWithValue("@addedBy", userId);

                command.ExecuteNonQuery();
            }
        }

        public bool DeleteMovie(int movieId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        DeleteSubtitles(movieId, connection, transaction);
                        DeleteMovieById(movieId, connection, transaction);
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении фильма: {ex.Message}");
                return false;
            }
        }

        private void DeleteSubtitles(int movieId, MySqlConnection connection, MySqlTransaction transaction)
        {
            var deleteSubtitlesQuery = "DELETE FROM Subtitles WHERE movie_id = @movieId";
            using (var command = new MySqlCommand(deleteSubtitlesQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@movieId", movieId);
                command.ExecuteNonQuery();
            }
        }

        private void DeleteMovieById(int movieId, MySqlConnection connection, MySqlTransaction transaction)
        {
            var deleteMovieQuery = "DELETE FROM Movies WHERE id = @id";
            using (var command = new MySqlCommand(deleteMovieQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@id", movieId);
                command.ExecuteNonQuery();
            }
        }

        public Movie GetMovieById(int movieId)
        {
            Movie movie = null;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Movies WHERE id = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", movieId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                movie = new Movie
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Description_movie = reader.GetString("description_movie"),
                                    VideoUrl = reader.GetString("video_url"),
                                    PosterUrl = reader.GetString("poster_url"),
                                    Level_en = reader.GetString("level_en")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения фильма: {ex.Message}");
            }

            return movie;
        }

        public SubtitleInfo GetSubtitleByMovieId(int movieId)
        {
            SubtitleInfo subtitle = null;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Subtitles WHERE movie_id = @movie_id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@movie_id", movieId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                subtitle = new SubtitleInfo
                                {
                                    Language = reader.GetString("language_sub"),
                                    FilePath = reader.GetString("subtitle_file")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения субтитров: {ex.Message}");
            }

            return subtitle;
        }

        public bool UpdateMovie(Movie movie)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "UPDATE Movies SET title = @title, description_movie = @description, video_url = @videoUrl, poster_url = @posterUrl, level_en = @level WHERE id = @id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@title", movie.Title);
                        command.Parameters.AddWithValue("@description", movie.Description_movie);
                        command.Parameters.AddWithValue("@videoUrl", movie.VideoUrl);
                        command.Parameters.AddWithValue("@posterUrl", movie.PosterUrl);
                        command.Parameters.AddWithValue("@level", movie.Level_en);
                        command.Parameters.AddWithValue("@id", movie.Id);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении фильма: {ex.Message}");
                return false;
            }
        }

        public bool UpdateSubtitle(int movieId, SubtitleInfo subtitle)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Subtitles SET language_sub = @language, subtitle_file = @subtitleFile WHERE movie_id = @movieId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@language", subtitle.Language);
                        command.Parameters.AddWithValue("@subtitleFile", subtitle.FilePath);
                        command.Parameters.AddWithValue("@movieId", movieId);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении субтитров: {ex.Message}");
                return false;
            }
        }
    }
}