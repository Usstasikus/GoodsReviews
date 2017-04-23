using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GoodsReivewsLibrary;
using System.IO;

namespace GraphicPart
{ 
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Fields _fields;
        public MainWindow()
        {
            InitializeComponent();
            _fields = new Fields();
            TextBoxPath.Focus();
        }


        private void ComboBoxDBFill()
        {
            Combobox_DB.Items.Clear();
            MyMethods.ComboBoxFill(Combobox_DB, MyMethods.GetDBList(TextBoxPath.Text));
        }

        private void TryToFillIn()
        {
            TextBoxPath.Text = _fields.IPAdress;
            Combobox_DB.IsEnabled = true;
            ComboBoxDBFill();
            Combobox_DB.SelectedItem = _fields.DB;
            TextBoxLogin.Text = _fields.Login;

        }
        public MainWindow(Fields fields)
        {
            InitializeComponent();
            _fields = fields;
            TryToFillIn();
            TextBoxPath.Focus();
        }
        private void TextBoxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordBox.Password != "" && TextBoxPath.Text != "" && TextBoxLogin.Text != "")
                Button.IsEnabled = true;
            else
                Button.IsEnabled = false;
            
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordBox.Password != "" && TextBoxPath.Text != "" && TextBoxLogin.Text != "")
                Button.IsEnabled = true;
            else
                Button.IsEnabled = false;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {

            if (PasswordBox.Password != "" && TextBoxPath.Text != "" && TextBoxLogin.Text != "")
                Button.IsEnabled = true;
            else
                Button.IsEnabled = false;
        }

        private void TextBoxPath_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Tab)
            {
                ComboBoxDBFill();
            }
        }

        private void Combobox_DB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBoxLogin.IsEnabled = true;
            PasswordBox.IsEnabled = true;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = String.Format("Server={0};Database={1};Uid={2};Pwd={3};", TextBoxPath.Text, Combobox_DB.SelectedItem, TextBoxLogin.Text, PasswordBox.Password);
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();
                sqlConnection.Close();
                _fields.IPAdress = TextBoxPath.Text;
                _fields.ConnectionString = connectionString;
                _fields.DB = Combobox_DB.SelectedItem.ToString();
                _fields.Login = TextBoxLogin.Text;
                
                WorkingWithDB window = new WorkingWithDB(_fields);
                window.Show();
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Неправильно заполнены поля");
            }
        }
    }
}
