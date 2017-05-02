using GoodsReivewsLibrary;
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
using System.Windows.Shapes;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GraphicPart
{
    /// <summary>
    /// Логика взаимодействия для NotNullableFields.xaml
    /// </summary>
    public partial class NotNullableFields : Window
    {
        List<string[]> _not_nullable_fields;
        List<TextBox> _not_filled;
        Dictionary<string, string> _field_types;
        Fields _fields;
        List<string> _ya_fields;

        /// <summary>
        /// Заполнение GroupBox полями для ввода значений
        /// </summary>
        private void FillingFields()
        {
            for (int i = 0; i < _not_nullable_fields.Count; i++)
            {
                TextBlock field = new TextBlock();
                TextBox value = new TextBox();
                CheckBox chb = new CheckBox();
                ComboBox cmb = new ComboBox();

                value.Name = String.Format("TextBox_{0}", i);
                value.KeyDown += TextBox_KeyDown;
                value.LostFocus += TextBox_LostFocus;

                field.Text = _not_nullable_fields[i][0];
                
                chb.Name = String.Format("CheckBox_{0}", i);
                chb.IsChecked = false;
                chb.Checked += CheckBox_Checked;
                chb.Unchecked += CheckBox_Unchecked;
                chb.ToolTip = "Нажмите, чтобы привязать данное поле к параметру, предоставляемым Яндекс Маркетом";

                cmb.Name = String.Format("ComboBox_{0}", i);
                cmb.Visibility = Visibility.Hidden;
                cmb.SelectionChanged += ComboBox_SelectionChanged;

                StackPanel_Fields.Children.Add(field);
                StackPanel_Values.Children.Add(value);
                StackPanel_Triggers.Children.Add(chb);
                StackPanel_ComboBox_Values.Children.Add(cmb);
            }
            MyMethods.GetTextBoxByName(StackPanel_Values, "TextBox_0").Focus();
        }

        /// <summary>
        /// Заполнение полей формы, тем, что уже были заполнены ранее (перед редактированием)
        /// </summary>
        private void FillFieldsFromUnknownFields()
        {
            TextBox value;
            ComboBox cmb;
            CheckBox chb;
            for (int i = 0; i < StackPanel_Triggers.Children.Count; i++)
            {
                value = (TextBox)StackPanel_Values.Children[i];
                cmb = (ComboBox)StackPanel_ComboBox_Values.Children[i];
                chb = (CheckBox)StackPanel_Triggers.Children[i];
                UnknownField unknown_field;
                if (_fields.unknown_fields.Count != 0)
                {
                    unknown_field = _fields.unknown_fields[i];
                    if (unknown_field.Dependency == null)
                        value.Text = unknown_field.Value;
                    else
                    {
                        chb.IsChecked = true;
                        MyMethods.ComboBoxFill(cmb, _ya_fields);
                        cmb.SelectedItem = MyMethods.TurnEnglishYandexFieldNameToRussian(unknown_field.Dependency);
                    }
                }
            }
        }
        

        /// <summary>
        /// Возвращает true, если возможна запись введенного в TextBox значение в соответствующее поле
        /// </summary>
        /// <param name="tb"></param>
        /// <returns></returns>
        private bool IsValid(TextBox tb)
        {
            bool is_written = true;
            int index_of_TextBox;
            int.TryParse(MyMethods.GetElementID(tb), out index_of_TextBox);
            string type = _not_nullable_fields[index_of_TextBox][1];
            if (type == "int")
            {
                int value;
                if (!int.TryParse(tb.Text, out value))
                    is_written = false;
            }
            return is_written;
        }


        /// <summary>
        /// Возвращает true, если есть хотя бы 1 незаполненный TextBox
        /// </summary>
        /// <returns></returns>
        private bool IsEmptyLeft()
        {
            for (int i = 0; i < _not_nullable_fields.Count; i++)
            {
                if ((bool)((CheckBox)StackPanel_Triggers.Children[i]).IsChecked)
                {
                    if (((ComboBox)StackPanel_ComboBox_Values.Children[i]).SelectedItem.ToString() == "")
                        return true;
                }
                else if (((TextBox)StackPanel_Values.Children[i]).Text == "")
                    return true;
            }
            return false;
        }

       
        /// <summary>
        /// Заполнение полей, неизвестных Яндекс Маркету
        /// </summary>
        private void FillUnknownFields()
        {
            _fields.unknown_fields.Clear();
            TextBox value;
            TextBlock name;
            CheckBox chb;
            for (int i = 0; i < StackPanel_Values.Children.Count; i++)
            {
                name = (TextBlock)StackPanel_Fields.Children[i];
                value = (TextBox)StackPanel_Values.Children[i];
                chb = (CheckBox)StackPanel_Triggers.Children[i];
                if ((bool)chb.IsChecked)
                {
                    ComboBox cmb = (ComboBox)StackPanel_ComboBox_Values.Children[i];
                    string yandex_english_param = MyMethods.TurnRussianYandexFieldNameToEnglish(cmb.SelectedItem.ToString());
                    _fields.unknown_fields.Add(new UnknownField(name.Text, value.Text, _not_nullable_fields[i][1],yandex_english_param));
                }
                else
                    _fields.unknown_fields.Add(new UnknownField(name.Text, value.Text, _not_nullable_fields[i][1]));
            }
        }
        public NotNullableFields()
        {
            InitializeComponent();
        }
        public NotNullableFields(List<string[]> not_nullable_fields, Fields fields)
        {
            InitializeComponent();
            _fields = fields;
            _not_nullable_fields = not_nullable_fields;
            _not_filled = new List<TextBox>();
            _field_types = MyMethods.GetFieldsType(_fields.ConnectionString, _fields.Table);
            _ya_fields = MyMethods.GetYandexFields();

            FillingFields();
            FillFieldsFromUnknownFields();

            if (!IsEmptyLeft())
                Next.IsEnabled = true;
        }

        public void ComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!IsEmptyLeft())
                Next.IsEnabled = true;
            else
                Next.IsEnabled = false;
        }

        public void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsEmptyLeft())
                Next.IsEnabled = true;
            else
                Next.IsEnabled = false;
        }

        public void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (!IsEmptyLeft())
                    Next.IsEnabled = true;
                else
                    Next.IsEnabled = false;

                TextBox tb = (TextBox)sender;
                if (!IsValid(tb))
                {
                    tb.Text = "";
                    MessageBox.Show("Невозможно записать данное значение в данное поле");
                }

                int index_of_TextBox;
                int.TryParse(MyMethods.GetElementID(tb), out index_of_TextBox);

                if (index_of_TextBox < _not_nullable_fields.Count - 1)
                    StackPanel_Values.Children[index_of_TextBox + 1].Focus();
                else if(Next.IsEnabled)
                    Next.Focus();
            }
        }
        

        /// <summary>
        /// Возвращает список тех полей, которые были привязаны к полям Яндекса
        /// </summary>
        /// <returns></returns>
        private List<string> GetFilledList()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < _fields.ya_fields.Count; i++)
            {
                list.Add(_fields.ya_fields[i].FieldName);
            }
            return list;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string index = MyMethods.GetElementID((CheckBox)sender);
            MyMethods.GetTextBoxByName(StackPanel_Values, "TextBox_" + index).Visibility = Visibility.Hidden;
            ComboBox cmb = MyMethods.GetComboBoxByName(StackPanel_ComboBox_Values, "ComboBox_" + index);
            cmb.Visibility = Visibility.Visible;

            MyMethods.ComboBoxFill(cmb, _ya_fields); //заполнение соответсвующего ComboBox
        }

        
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string index = MyMethods.GetElementID((CheckBox)sender);
            MyMethods.GetTextBoxByName(StackPanel_Values, "TextBox_" + index).Visibility = Visibility.Visible;
            ComboBox cmb = MyMethods.GetComboBoxByName(StackPanel_ComboBox_Values, "ComboBox_" + index);
            cmb.Visibility = Visibility.Hidden;
            
        }
        

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            FillUnknownFields();

            Action act = new Action(_fields);

            BinaryFormatter bin_formatter = new BinaryFormatter();
            
            using(FileStream fs = new FileStream(@"..\..\..\Resources\DBS\" + _fields.FileName + ".dbs", FileMode.Create))
            {
                bin_formatter.Serialize(fs, _fields);
            }
            File.Create(@"..\..\..\Resources\last_pos_" + _fields.FileName + ".txt");
            act.Show();
            Close();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            IdModelChoice imc = new IdModelChoice(_not_nullable_fields, _fields);
            imc.Show();
            Close();
        }
    }
}
