using System;
using System.Collections.Generic;
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GraphicPart
{
    /// <summary>
    /// Логика взаимодействия для RenameFile.xaml
    /// </summary>
    public partial class RenameFile : Window
    {
        string _old_name;
        public RenameFile(string old_name)
        {
            InitializeComponent();
            _old_name = old_name;
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

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            string new_name;
            if (Methods.IsNameExist(TextBox_FileName.Text))
            {
                MessageBox.Show("Файл с таким именем уже существует. Пожалуйста, выберите другое имя.");
                TextBox_FileName.Focus();
            }
            else
            {
                new_name = TextBox_FileName.Text;
                Fields fields = Methods.Deserialize(_old_name);
                fields.FileName = new_name;

                BinaryFormatter bin_formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream(@"..\..\..\Resources\DBS\" + new_name + ".dbs", FileMode.Create))
                {
                    bin_formatter.Serialize(fs, fields);
                }
                File.Delete(@"..\..\..\Resources\DBS\" + _old_name + ".dbs");
                File.Move(@"..\..\..\Resources\last_pos\last_pos_" + _old_name + ".txt", @"..\..\..\Resources\last_pos\last_pos_" + new_name + ".txt");

                ChooseFile chf = new ChooseFile();
                chf.Show();
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Все изменения будут потеряны. Вы действительно хотите возвратиться на стартовую страницу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ChooseFile chf = new ChooseFile();
                chf.Show();
                Close();
            }
        }
    }
}
