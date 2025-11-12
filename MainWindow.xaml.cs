using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using WinBootSelfStarting.Models;
using WinBootSelfStarting.Services;
using System.Windows.Controls;

namespace WinBootSelfStarting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<StartupEntry> _allEntries = new();

        public MainWindow()
        {
            InitializeComponent();
            SearchBox.TextChanged += SearchBox_TextChanged;
            LoadEntries();
        }

        private void SetStatus(string text)
        {
            StatusText.Text = text;
        }

        private void LoadEntries()
        {
            try
            {
                _allEntries = StartupManager.ListEntries();
                UpdateGrid();
                SetStatus($"已加载 {_allEntries.Count} 条启动项");
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载启动项失败: " + ex.Message);
            }
        }

        private void UpdateGrid()
        {
            var q = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(q))
            {
                EntriesGrid.ItemsSource = _allEntries.OrderBy(e => e.Name);
            }
            else
            {
                var filtered = _allEntries.Where(e => (e.Name ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (e.Command ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
                EntriesGrid.ItemsSource = filtered.OrderBy(e => e.Name);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateGrid();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEntries();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            if (dlg.ShowDialog(this) == true)
            {
                var path = dlg.FileName;
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                var cmd = '"' + path + '"';
                var ok = StartupManager.AddRegistryEntry(name, cmd);
                if (ok)
                {
                    LoadEntries();
                    SetStatus("已添加启动项: " + name);
                }
                else
                {
                    MessageBox.Show("添加启动项失败");
                }
            }
        }

        private StartupEntry? GetSelected()
        {
            return EntriesGrid.SelectedItem as StartupEntry;
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            var sel = GetSelected();
            if (sel == null) return;
            var ok = StartupManager.EnableEntry(sel);
            if (ok) { LoadEntries(); SetStatus("已启用: " + sel.Name); }
            else MessageBox.Show("启用失败");
        }

        private void DisableButton_Click(object sender, RoutedEventArgs e)
        {
            var sel = GetSelected();
            if (sel == null) return;
            var ok = StartupManager.DisableEntry(sel);
            if (ok) { LoadEntries(); SetStatus("已禁用: " + sel.Name); }
            else MessageBox.Show("禁用失败");
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var sel = GetSelected();
            if (sel == null) return;
            var res = MessageBox.Show($"确认删除启动项 '{sel.Name}' ?", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            var ok = StartupManager.RemoveEntry(sel);
            if (ok) { LoadEntries(); SetStatus("已删除: " + sel.Name); }
            else MessageBox.Show("删除失败");
        }
    }
}