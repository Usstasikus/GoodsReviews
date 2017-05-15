using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file_name">Имя файла настройки</param>
        public MainWindow(string file_name)
        {
            InitializeComponent();
            _fields = new Fields();
            _fields.FileName = file_name;
            TextBoxPath.Focus();
        }


        private void ComboBoxDBFill()
        {
            Combobox_DB.Items.Clear();
            TextBoxLogin.Clear();
            TextBoxLogin.IsEnabled = false;
            PasswordBox.Clear();
            PasswordBox.IsEnabled = false;
            
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields">Объект настройки</param>
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
        
        private async void TextBoxPath_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Tab)
            {
                string adress = TextBoxPath.Text;
                ProgressBarWindow pbw = new ProgressBarWindow("Происходит подключение к серверу");
                Dispatcher.BeginInvoke((System.Action)(() => pbw.ShowDialog()));
                var is_correct = await Task.Run(() =>
                {
                    return (MyMethods.GetDBList(adress) != null);
                });
                pbw.Close();

                if (is_correct)
                    ComboBoxDBFill();
                else
                    Combobox_DB.IsEnabled = false;

            }
        }

        private void Combobox_DB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBoxLogin.IsEnabled = true;
            PasswordBox.IsEnabled = true;
        }

        /// <summary>
        /// Проверяет, верно ли задан
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private bool IsAdrresCorrect(string connectionString)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = String.Format("Server={0};Database={1};Uid={2};Pwd={3};", TextBoxPath.Text, Combobox_DB.SelectedItem, TextBoxLogin.Text, PasswordBox.Password);
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            try
            {
                ProgressBarWindow pbw = new ProgressBarWindow();
                Dispatcher.BeginInvoke((System.Action)(() => pbw.ShowDialog()));
                bool is_correct = await Task<bool>.Run(()=> IsAdrresCorrect(connectionString));
                pbw.Close();
                if (!is_correct)
                    throw new Exception();
                IsEnabled = true;
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
                MessageBox.Show("Неправильный логин или пароль");
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
