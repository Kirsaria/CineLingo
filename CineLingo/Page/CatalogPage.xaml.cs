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
        public CatalogPage()
        {
            InitializeComponent();
        }
        private void MovieButton_Click(object sender, RoutedEventArgs e)
        {
            var movieId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new VideoPlayerPage(movieId));
        }
    }
}
