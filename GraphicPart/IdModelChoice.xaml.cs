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
    /// Логика взаимодействия для IdModelChoice.xaml
    /// </summary>
    public partial class IdModelChoice : Window
    {
        List<string[]> _not_nullable_fields;
        string _connectionString;
        Fields _fields;
        public IdModelChoice()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Заполнение элементов соответствующими значения для редактирования
        /// </summary>
        private void TryToFillIn()
        {
            if (_fields.GoodsIDFrom != null && _fields.GoodsIDTo != null && _fields.GoodsNameFrom != null 
                && _fields.TableFrom != null)
            {
                ComboBox_TableFrom.SelectedItem = _fields.TableFrom;
                ComboBox_FieldFrom.SelectedItem = _fields.GoodsIDFrom;
                ComboBox_FieldTo.SelectedItem = _fields.GoodsIDTo;
                ComboBox_GoodsName_FieldFrom.SelectedItem = _fields.GoodsNameFrom;
            }
        }

        public IdModelChoice(List<string[]> not_nullable_fields, Fields fields)
        {
            _not_nullable_fields = not_nullable_fields;
            _connectionString = fields.ConnectionString;
            _fields = fields;
            InitializeComponent();
            Methods.ComboBoxFill(ComboBox_TableFrom, Methods.GetTablesList(_connectionString));
            Methods.ComboBoxFill(ComboBox_FieldTo, Methods.GetFieldsList(_connectionString, _fields.Table));
            TryToFillIn();
        }


        private void ComboBox_TableFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Methods.ComboBoxFill(ComboBox_FieldFrom,
                Methods.GetFieldsList(_connectionString, ComboBox_TableFrom.SelectedItem.ToString()));
            ComboBox_FieldFrom.IsEnabled = true;

            Methods.ComboBoxFill(ComboBox_GoodsName_FieldFrom,
                Methods.GetFieldsList(_connectionString, ComboBox_TableFrom.SelectedItem.ToString()));
            ComboBox_GoodsName_FieldFrom.IsEnabled = true;
        }

        private void ComboBox_Fields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_FieldFrom.SelectedItem != null && ComboBox_FieldTo.SelectedItem != null && ComboBox_GoodsName_FieldFrom.SelectedItem != null)
                Next.IsEnabled = true;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _fields.TableFrom = ComboBox_TableFrom.SelectedItem.ToString();
            _fields.GoodsIDFrom = ComboBox_FieldFrom.SelectedItem.ToString();
            _fields.GoodsIDTo = ComboBox_FieldTo.SelectedItem.ToString();
            _fields.GoodsNameFrom = ComboBox_GoodsName_FieldFrom.SelectedItem.ToString();

            if (_not_nullable_fields.Count != 0)
            {
                NotNullableFields nnf = new NotNullableFields(_not_nullable_fields, _fields);
                nnf.Show();
                Close();
            }
            else
            {
                try
                {
                    BinaryFormatter bin_formatter = new BinaryFormatter();
                    using (FileStream fs = new FileStream(@"..\..\..\Resources\DBS\" + _fields.FileName + ".dbs", FileMode.Create))
                    {
                        bin_formatter.Serialize(fs, _fields);
                    }
                    using (FileStream fs = new FileStream(@"..\..\..\Resources\last_pos\last_pos_" + _fields.FileName + ".txt", FileMode.Create))
                    {
                    }
                    Action act = new Action(_fields);
                    act.Show();
                    Close();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Отсутствуют права для записи/редактирования файлов в директории программы.\nПереустановите программу в папку, свободной для редактирования.");
                    Close();
                }
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            WorkingWithDB wdb = new WorkingWithDB(_fields);
            wdb.Show();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Все изменения будут потеряны. Вы действительно хотите возвратиться на стартовую страницу?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ChooseFile chf = new ChooseFile();
                chf.Show();
                Close();
            }
        }
    }
}
