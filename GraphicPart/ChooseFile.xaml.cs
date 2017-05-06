using GoodsReivewsLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        /// <summary>
        /// Десериализует файл, выбранный в ListBox_OpenExisting
        /// </summary>
        /// <returns></returns>
        private Fields Deserialize()
        {
            BinaryFormatter bin_form = new BinaryFormatter();
            Fields fields;
            using (FileStream fs = new FileStream(@"..\..\..\Resources\DBS\" + ListBox_OpenExisting.SelectedItem.ToString() + ".dbs", FileMode.Open))
            {
                fields = (Fields)bin_form.Deserialize(fs);
            }
            return fields;
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
                Fields fields = Deserialize();
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
                Fields fields = Deserialize();
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
        
    }
}
