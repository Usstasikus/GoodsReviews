﻿using GoodsReivewsLibrary;
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
                && _fields.TableFrom != null && _fields.GoodsNameTableFrom != null)
            {
                ComboBox_TableFrom.SelectedItem = _fields.TableFrom;
                ComboBox_FieldFrom.SelectedItem = _fields.GoodsIDFrom;
                ComboBox_FieldTo.SelectedItem = _fields.GoodsIDTo;
                ComboBox_GoodsName_TableFrom.SelectedItem = _fields.GoodsNameTableFrom;
                ComboBox_GoodsName_FieldFrom.SelectedItem = _fields.GoodsNameFrom;
            }
        }

        public IdModelChoice(List<string[]> not_nullable_fields, Fields fields)
        {
            _not_nullable_fields = not_nullable_fields;
            _connectionString = fields.ConnectionString;
            _fields = fields;
            InitializeComponent();
            MyMethods.ComboBoxFill(ComboBox_TableFrom, MyMethods.GetTablesList(_connectionString));
            MyMethods.ComboBoxFill(ComboBox_FieldTo, MyMethods.GetFieldsList(_connectionString, _fields.Table));
            MyMethods.ComboBoxFill(ComboBox_GoodsName_TableFrom, MyMethods.GetTablesList(_connectionString));
            TryToFillIn();
        }


        private void ComboBox_TableFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyMethods.ComboBoxFill(ComboBox_FieldFrom,
                MyMethods.GetFieldsList(_connectionString, ComboBox_TableFrom.SelectedItem.ToString()));
            ComboBox_FieldFrom.IsEnabled = true;
        }
        private void ComboBox_GoodsName_TableFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyMethods.ComboBoxFill(ComboBox_GoodsName_FieldFrom,
                MyMethods.GetFieldsList(_connectionString, ComboBox_GoodsName_TableFrom.SelectedItem.ToString()));
        }

        private void ComboBox_Fields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_FieldFrom.SelectedItem != null && ComboBox_FieldTo.SelectedItem != null)
                Next.IsEnabled = true;

        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _fields.TableFrom = ComboBox_TableFrom.SelectedItem.ToString();
            _fields.GoodsIDFrom = ComboBox_FieldFrom.SelectedItem.ToString();
            _fields.GoodsIDTo = ComboBox_FieldTo.SelectedItem.ToString();
            _fields.GoodsNameTableFrom = ComboBox_GoodsName_TableFrom.SelectedItem.ToString();
            _fields.GoodsNameFrom = ComboBox_GoodsName_FieldFrom.SelectedItem.ToString();

            if (_not_nullable_fields.Count != 0)
            {
                NotNullableFields nnf = new NotNullableFields(_not_nullable_fields, _fields);
                nnf.Show();
                Close();
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            WorkingWithDB wdb = new WorkingWithDB(_fields);
            wdb.Show();
            Close();
        }
    }
}
