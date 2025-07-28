using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using WpfBankProject.Models;
using WpfBankProject.Services;

namespace WpfBankProject.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Mt940RawLine> Lines { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        // manage the clik button
        private void ImportRawLines_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MT940 Files (*.sta;*.txt)|*.sta;*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                List<Mt940RawLine> lines = Mt940Parser.ParseRawLines(openFileDialog.FileName);
                RawLinesGrid.ItemsSource = lines;
            }
        }
    }
}