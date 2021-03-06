﻿using GoodsReivewsLibrary;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GraphicPart
{
    /// <summary>
    /// Логика взаимодействия для ChooseFile.xaml
    /// </summary>
    public partial class ChooseFile : Window
    {
        private void InitializeListBox()
        {
            ListBox_Fill();
        }
        public ChooseFile()
        {
            InitializeComponent();
            InitializeListBox();

        }

        /// <summary>
        /// Заполняет ListBox именами файлов, хранящихся по пути "..\..\..\Resources\DBS"
        /// </summary>
        private void ListBox_Fill()
        {
            ListBox_OpenExisting.Items.Clear();
            DirectoryInfo dir = new DirectoryInfo(@"..\..\..\Resources\DBS");
            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                string file_name = System.IO.Path.GetFileNameWithoutExtension(@"..\..\..\Resources\DBS\" + files[i].Name);
                ListBox_OpenExisting.Items.Add(file_name);
            }
        }
        
        private void ListBox_OpenExisting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Open.IsEnabled = true;
        }

        private void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            FileName fn = new FileName();
            fn.Show();
            Close();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Fields fields = Methods.Deserialize(ListBox_OpenExisting.SelectedItem.ToString());
                if (!File.Exists(@"..\..\..\Resources\last_pos\last_pos_" + fields.FileName + ".txt")) throw new Exception();
                Action act = new Action(fields);
                act.Show();
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно прочесть данный файл");
            }
        }

        private void ListBox_OpenExisting_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            Open_Click(sender, e);
        }

        private void MenuItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Fields fields = Methods.Deserialize(ListBox_OpenExisting.SelectedItem.ToString());
                MainWindow mwd = new MainWindow(fields);
                mwd.Show();
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно прочесть данный файл");
            }
        }
        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            FileInfo file = new FileInfo(@"..\..\..\Resources\DBS\" + ListBox_OpenExisting.SelectedItem.ToString() + ".dbs");
            if (file.Exists)
            {
                if (MessageBox.Show("Вы действительно хотите удалить данный файл без возможности восстановления?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    file.Delete();

                    FileInfo log_file = new FileInfo(@"..\..\..\Resources\last_pos\last_pos_" + ListBox_OpenExisting.SelectedItem.ToString() + ".txt");
                    if (log_file.Exists)
                        log_file.Delete();
                    ListBox_Fill();
                }
            }
        }

        private void ListBox_OpenExisting_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.SelectedItem == null)
            {
                foreach(MenuItem mi in lb.ContextMenu.Items)
                {
                    mi.IsEnabled = false;
                }
            }
            else
            {
                foreach (MenuItem mi in lb.ContextMenu.Items)
                {
                    mi.IsEnabled = true;
                }
            }
        }

        private void MenuItem_Rename_Click(object sender, RoutedEventArgs e)
        {
            string old_name = ListBox_OpenExisting.SelectedItem.ToString();
            RenameFile rf = new RenameFile(old_name);
            rf.Show();
            Close();
        }
    }
}
