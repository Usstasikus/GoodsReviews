using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GoodsReivewsLibrary;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraphicPart
{
    /// <summary>
    /// Логика взаимодействия для FileName.xaml
    /// </summary>
    public partial class FileName : Window
    {
        public FileName()
        {
            InitializeComponent();
            TextBox_FileName.Focus();
        }

        private void TextBox_FileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tb = TextBox_FileName.Text;
            if (TextBox_FileName.Text == "")
                Next.IsEnabled = false;
            else
                Next.IsEnabled = true;
        }


        /// <summary>
        /// Проверяет наличие файла с таким именем
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsNameExist(string name)
        {
            DirectoryInfo dir = new DirectoryInfo("DBS");
            FileInfo[] files = dir.GetFiles();
            List<string> names = new List<string>();
            for (int i = 0; i < files.Length; i++)
                names.Add(files[i].Name);
            
            if (names.IndexOf(name) == -1)
            {
                return false;
            }
            return true;

        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            string file_name;
            if (IsNameExist(TextBox_FileName.Text))
            {
                MessageBox.Show("Файл с таким именем уже существует. Пожалуйста, выберите другое имя.");
                TextBox_FileName.Focus();
            }
            else
            {
                file_name = TextBox_FileName.Text;
                MainWindow mwd = new MainWindow(file_name);
                mwd.Show();
                Close();
            }
        }
        
    }
}
