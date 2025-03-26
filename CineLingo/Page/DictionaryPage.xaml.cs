using CineLingo.Models;
using MySql.Data.MySqlClient;
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
    public partial class DictionaryPage : System.Windows.Controls.Page
    {
        public DictionaryPage()
        {
            InitializeComponent();
            Loaded += DictionaryPage_Loaded;
        }
        private void DictionaryPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDictionaryItems();
        }
        public async void LoadDictionaryItems()
        {
            if (AuthWindow.CurrentUserId == 0)
            {
                MessageBox.Show("Пожалуйста, войдите в систему");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(AuthWindow.ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT WordOrPhrase, fullsentence, translation, subtitleFile, addedDate 
                                  FROM DictionaryItem 
                                  WHERE userId = @userId 
                                  ORDER BY addedDate DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", AuthWindow.CurrentUserId);

                        var items = new List<DictionaryItem>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new DictionaryItem
                                {
                                    WordOrPhrase = reader["WordOrPhrase"].ToString(),
                                    Fullsentence = reader["fullsentence"].ToString(),
                                    Translation = reader["translation"].ToString(),
                                    SubtitleFile = reader["subtitleFile"].ToString(),
                                    AddedDate = Convert.ToDateTime(reader["addedDate"])
                                });
                            }
                        }

                        DictionaryListView.ItemsSource = items;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке словаря: {ex.Message}");
            }

        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DictionaryListView.ItemsSource is IEnumerable<DictionaryItem> items)
            {
                var searchText = SearchBox.Text.ToLower();
                DictionaryListView.ItemsSource = items.Where(i =>
                    i.WordOrPhrase.ToLower().Contains(searchText) ||
                    i.Translation.ToLower().Contains(searchText) ||
                    i.Fullsentence.ToLower().Contains(searchText));
            }
        }
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DictionaryListView?.ItemsSource == null || FilterComboBox?.SelectedIndex == null)
                    return;

                var items = DictionaryListView.ItemsSource.Cast<DictionaryItem>().ToList();

                switch (FilterComboBox.SelectedIndex)
                {
                    case 0: items = items.OrderByDescending(i => i.AddedDate).ToList(); break;
                    case 1: items = items.OrderByDescending(i => i.AddedDate).Take(50).ToList(); break;
                    case 2: items = items.OrderBy(i => i.WordOrPhrase).ToList(); break;
                    case 3: items = items.OrderBy(i => i.SubtitleFile).ToList(); break;
                }

                DictionaryListView.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDictionaryItems();
        }
    }
}
