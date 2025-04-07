using CineLingo.Data;
using CineLingo.Models;
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
using System.Windows.Shapes;

namespace CineLingo.Page
{
    public partial class CatalogPage : System.Windows.Controls.Page
    {
        private MovieService _movieService;
        public CatalogPage()
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
        private void OnMovieDoubleClick(object sender, RoutedEventArgs e)
        {
            var movie = (Movie)((FrameworkElement)sender).DataContext; 
            if (movie != null)
            {
                NavigationService.Navigate(new VideoPlayerPage(movie.Id));
            }
        }
    }
}
