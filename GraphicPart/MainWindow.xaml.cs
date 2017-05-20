using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GoodsReivewsLibrary;
using System.Windows.Threading;

namespace GraphicPart
{ 
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Fields _fields;
        bool was_button_pressed;
        public MainWindow()
        {
            InitializeComponent();
            _fields = new Fields();
            TextBoxPath.Focus();
            was_button_pressed = false;
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
        

        /// <summary>
        /// Заполняет ComboBox
        /// </summary>
        private void ComboBoxDBFill()
        {
            Combobox_DB.Items.Clear();
            Methods.ComboBoxFill(Combobox_DB, Methods.GetDBList(TextBoxPath.Text, TextBoxLogin.Text, PasswordBox.Password));
        }

        private void TryToFillIn()
        {
            TextBoxPath.Text = _fields.Adress;
            TextBoxLogin.Text = _fields.Login;
            PasswordBox.Password = _fields.Password;
            Combobox_DB.IsEnabled = true;
            ComboBoxDBFill();
            Combobox_DB.SelectedItem = _fields.DB;

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
            if (Combobox_DB.IsEnabled)
            {
                Combobox_DB.Items.Clear();
                Combobox_DB.IsEnabled = false;
                Button.IsEnabled = false;
            }
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Combobox_DB.IsEnabled)
            {
                Combobox_DB.Items.Clear();
                Combobox_DB.IsEnabled = false;
                Button.IsEnabled = false;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Combobox_DB.IsEnabled)
            {
                Combobox_DB.Items.Clear();
                Combobox_DB.IsEnabled = false;
                Button.IsEnabled = false;
            }
        }
        private void TextBoxPath_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
                TextBoxLogin.Focus();
        }

        private void TextBoxLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PasswordBox.Focus();
        }

        private async void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                was_button_pressed = true;
                string adress = TextBoxPath.Text;
                string user_id = TextBoxLogin.Text;
                string password = PasswordBox.Password;
                ProgressBarWindow pbw = new ProgressBarWindow("Происходит подключение к серверу");
                Dispatcher.BeginInvoke((System.Action)(() => pbw.ShowDialog()));
                var is_correct = await Task.Run(() =>
                {
                    return (Methods.GetDBList(adress, user_id, password) != null);
                });

                if (pbw.IsActive && !is_correct)
                {
                    MessageBox.Show("Невозможно подключиться к данному серверу.\nПроверьте корректность введенных данных.");
                    Combobox_DB.Items.Clear();
                    Combobox_DB.IsEnabled = false;
                }

                pbw.Close();

                if (is_correct)
                    ComboBoxDBFill();
                Combobox_DB.Focus();
            }
        }

        private void Combobox_DB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Button.IsEnabled = true;
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
            string connectionString = String.Format("Data Source={0};Initial Catalog={1};User id={2};Password={3};", TextBoxPath.Text, Combobox_DB.SelectedItem, TextBoxLogin.Text, PasswordBox.Password);
           
            _fields.Adress = TextBoxPath.Text;
            _fields.ConnectionString = connectionString;
            _fields.DB = Combobox_DB.SelectedItem.ToString();
            _fields.Login = TextBoxLogin.Text;
            _fields.Password = PasswordBox.Password;
            
            WorkingWithDB window = new WorkingWithDB(_fields);
            window.Show();
            Close();
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
