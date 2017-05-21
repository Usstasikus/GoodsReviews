using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls; 
using GoodsReivewsLibrary;

namespace GraphicPart
{
    
    /// <summary>
    /// Логика взаимодействия для WorkingWithDB.xaml
    /// </summary>
    public partial class WorkingWithDB : Window
    {
        string _connectionString;
        List<string> added_fields;
        List<string> unfilled_comboboxes;
        Dictionary<string, string> field_types;
        List<string> _used_columns;
        Fields _fields;
        public WorkingWithDB()
        {
            InitializeComponent();
            ComboBox_Table.Focus();
        }


        /// <summary>
        /// Заполнение соответствующих полей при редактировании
        /// </summary>
        private void TryToFillIn()
        {
            ComboBox_Table.SelectedItem = _fields.Table;
            for(int i = 0; i<_fields.ya_fields.Count; i++)
            {
                if (_fields.ya_fields[i] != null)
                {
                    Methods.GetCheckBoxByName(StackPanel_Fields, _fields.ya_fields[i].YandexElementName).IsChecked = true;
                    Methods.GetComboBoxByName(StackPanel_DBFields, "comboBox_" + _fields.ya_fields[i].YandexElementName).SelectedItem
                        = _fields.ya_fields[i].FieldName;
                }
            }
        }
        
        /// <summary>
        /// Конструктор, использующийся для редактирования настроек
        /// </summary>
        /// <param name="fields"></param>
        public WorkingWithDB(Fields fields)
        {
            InitializeComponent();
            _fields = fields;
            _connectionString = _fields.ConnectionString;
            unfilled_comboboxes = new List<string>();
            _used_columns = new List<string>();
            field_types = new Dictionary<string, string>();
            Methods.ComboBoxFill(ComboBox_Table, Methods.GetTablesList(_connectionString));
            TryToFillIn();
            ComboBox_Table.Focus();
        }

        /// <summary>
        /// Поиск автоинкрементированных полей
        /// </summary>
        /// <returns></returns>
        private List<string> AutoIncrementedFields()
        {
            List<string> auto_incremented_fields = new List<string>();
            SqlConnection sqlConnection = new SqlConnection(_connectionString);
            sqlConnection.Open();
            string queryString = String.Format(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu,
                                                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                                                    WHERE CONSTRAINT_TYPE = 'PRIMARY KEY'
                                                    and cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                                                    AND cu.TABLE_NAME = '{0}' ORDER BY ORDINAL_POSITION", ComboBox_Table.SelectedItem);
            SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
            SqlDataReader rd = sqlCommand.ExecuteReader();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    auto_incremented_fields.Add(rd.GetString(0));
                }
                rd.Close();
            }
            sqlConnection.Close();
            return auto_incremented_fields;
        }

        /// <summary>
        /// Дезактивация ComboBox
        /// </summary>
        /// <param name="combobox"></param>
        private void ComboboxUncheck(ComboBox combobox)
        {
            combobox.Items.Clear();
            combobox.IsEnabled = false;

            unfilled_comboboxes.Remove(combobox.Name);

            if (unfilled_comboboxes.Count == 0)
                Next.IsEnabled = true;

        }


        /// <summary>
        /// Добавление к списку незаполненных ComboBox имени данного элемента
        /// </summary>
        /// <param name="cmb"></param>
        private void AddToUnfilledList(ComboBox cmb)
        {
            unfilled_comboboxes.Add(cmb.Name);
            if (unfilled_comboboxes.Count > 0)
                Next.IsEnabled = false;
        }

        private void GroupBoxReboot()
        {
            for(int i = 0; i<StackPanel_Fields.Children.Count; i++)
            {
                ((CheckBox)(StackPanel_Fields.Children[i])).IsChecked = false;
            }
        }

        private void ComboBox_Table_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            groupBox.IsEnabled = true;
            _fields.Table = ComboBox_Table.SelectedItem.ToString();
            GroupBoxReboot();
            field_types = Methods.GetFieldsType(_connectionString, _fields.Table);
        }
    

        #region Логика работы с Чекерами

        private void Grade_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Grade);
            Methods.ComboBoxFill(comboBox_Grade, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void Agree_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Agree);
            Methods.ComboBoxFill(comboBox_Agree, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void Reject_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Reject);
            Methods.ComboBoxFill(comboBox_Reject, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void Date_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Date);
            Methods.ComboBoxFill(comboBox_Date, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void Name_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Name);
            Methods.ComboBoxFill(comboBox_Name, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void Text_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Text);
            Methods.ComboBoxFill(comboBox_Text, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void Pro_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Pro);
            Methods.ComboBoxFill(comboBox_Pro, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void Contra_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_Contra);
            Methods.ComboBoxFill(comboBox_Contra, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }

        private void UsageTime_Checked(object sender, RoutedEventArgs e)
        {
            AddToUnfilledList(comboBox_UsageTime);
            Methods.ComboBoxFill(comboBox_UsageTime, Methods.GetFieldsList(_connectionString, ComboBox_Table.SelectedItem.ToString()));
        }
        private void Grade_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Grade);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void Agree_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Agree);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void Reject_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Reject);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void Date_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Date);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void Name_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Name);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void Text_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Text);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void Pro_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Pro);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void Contra_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_Contra);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        private void UsageTime_Unchecked(object sender, RoutedEventArgs e)
        {
            ComboboxUncheck(comboBox_UsageTime);
            Methods.RemoveByFirstValue(_fields.ya_fields, ((CheckBox)sender).Name);
        }

        #endregion

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> not_nullable_fields = Methods.GetNotNullableFields(_connectionString, ComboBox_Table.SelectedItem.ToString());

            FillYaFields();

            added_fields = new List<string>();
            
            for(int i = 0; i<StackPanel_DBFields.Children.Count && StackPanel_DBFields.Children[i] is ComboBox 
                && ((ComboBox)StackPanel_DBFields.Children[i]).IsEnabled == true; i++)
            {
                ComboBox combobox = (ComboBox)StackPanel_DBFields.Children[i];
                added_fields.Add(combobox.SelectedItem.ToString());
            }

            for(int i = 0; i<added_fields.Count; i++)//удаление полей, принимающих значение NULL, из списка полей, которые надо заполнить вручную
            {
                Methods.RemoveByFirstValue(not_nullable_fields, added_fields[i]);
            }

            List<string> auto_incremented_fields = AutoIncrementedFields(); //удаление автоинкрементированных полей из списка полей, которые надо заполнить вручную
            for (int i = 0; i<auto_incremented_fields.Count; i++)
            {
                Methods.RemoveByFirstValue(not_nullable_fields, auto_incremented_fields[i]);
            }
            
            IdModelChoice nnf = new IdModelChoice(not_nullable_fields, _fields);
            nnf.Show();
            Close();

        }

        /// <summary>
        /// Заполнение полея, связанных с параметрами Яндекс Маркета
        /// </summary>
        private void FillYaFields()
        {
            _fields.ya_fields.Clear();
            for (int i = 0; i < StackPanel_DBFields.Children.Count; i++)
            {
                if (((ComboBox)StackPanel_DBFields.Children[i]).IsEnabled)
                {
                    ComboBox cmb = (ComboBox)StackPanel_DBFields.Children[i];
                    try
                    {
                        switch (cmb.Name)
                        {
                            case "comboBox_Grade":
                                _fields.ya_fields.Add(new KnownField("Grade", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_Agree":
                                _fields.ya_fields.Add(new KnownField("Agree", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_Reject":
                                _fields.ya_fields.Add(new KnownField("Reject", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_Date":
                                _fields.ya_fields.Add(new KnownField("Date", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_Name":
                                _fields.ya_fields.Add(new KnownField("Name", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_Text":
                                _fields.ya_fields.Add(new KnownField("Text", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_Pro":
                                _fields.ya_fields.Add(new KnownField("Pro", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_Contra":
                                _fields.ya_fields.Add(new KnownField("Contra", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            case "comboBox_UsageTime":
                                _fields.ya_fields.Add(new KnownField("UsageTime", cmb.SelectedItem.ToString(),
                                    field_types[cmb.SelectedItem.ToString()]));
                                break;

                            default:
                                MessageBox.Show("Неизвестная ошибка");
                                break;
                        }
                    }
                    catch (NullReferenceException)
                    { }
                    catch (Exception)
                    {
                        MessageBox.Show("Неизвестная ошибка");
                    }
                }
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            unfilled_comboboxes.Remove(((ComboBox)sender).Name);
            _used_columns.Add(((ComboBox)sender).Name);
            if (unfilled_comboboxes.Count == 0)
                Next.IsEnabled = true;
            
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mwd = new MainWindow(_fields);
            mwd.Show();
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
