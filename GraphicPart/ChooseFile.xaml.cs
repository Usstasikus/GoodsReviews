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
            DirectoryInfo dir= new DirectoryInfo("DBS");
            FileInfo[] files = dir.GetFiles();
            for(int i = 0; i<files.Length; i++)
            {
                ListBox_OpenExisting.Items.Add(files[i].Name);
            }
        }
        public ChooseFile()
        {
            InitializeComponent();
            InitializeListBox();

        }

        /// <summary>
        /// Десериализует файл, выбранный в ListBox_OpenExisting
        /// </summary>
        /// <returns></returns>
        private Fields Deserialize()
        {
            BinaryFormatter bin_form = new BinaryFormatter();
            Fields fields;
            using (FileStream fs = new FileStream("DBS\\" + ListBox_OpenExisting.SelectedItem.ToString(), FileMode.Open))
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
            MainWindow main = new MainWindow();
            main.Show();
            Close();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Fields fields = Deserialize();
            Action act = new Action(fields);
            act.Show();
            Close();
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
            Fields fields = Deserialize();
            MainWindow mwd = new MainWindow(fields);
            mwd.Show();
            Close();
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
