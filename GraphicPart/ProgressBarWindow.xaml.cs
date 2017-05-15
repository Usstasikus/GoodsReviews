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

namespace GraphicPart
{
    public delegate void Login(string connectionString);
    public delegate bool ConnectToDB(string connectionString);
    /// <summary>
    /// Логика взаимодействия для ProgressBarWindow.xaml
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindow(string message = "Выполняется загрузка")
        {
            InitializeComponent();
            TextBlock_PB.Text = message;
        }
    }
}
