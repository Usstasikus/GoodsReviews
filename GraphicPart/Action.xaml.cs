using GoodsReivewsLibrary;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace GraphicPart
{
    /// <summary>
    /// Логика взаимодействия для Action.xaml
    /// </summary>
    public partial class Action : Window
    {
        Fields _fields;
        Actions act;
        CancellationTokenSource cancelTokenSource;
        CancellationToken token;
        private List<string[]> GetNotNullableFields()
        {
            List<string[]> nnf = new List<string[]>();
            for(int i = 0; i<_fields.unknown_fields.Count; i++)
            {
                UnknownField uf = _fields.unknown_fields[i];
                nnf.Add(new string[] { uf.FieldName, uf.Type });
            }
            return nnf;
        }

        public Action()
        {
            InitializeComponent();
        }
        public Action(Fields fields)
        {
            InitializeComponent();
            _fields = fields;
            act = new Actions(_fields);
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadReviews(act);
            //act.LoadNewReviews(TextBox_Output);
        }
        private async void LoadReviews(Actions act)
        {
            var progress = new Progress<string>(s => TextBlock_Output.Text = s);
            string result = await Task.Factory.StartNew<string>(() => act.LoadNewReviews(progress, token), token);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите завершить работу приложения?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                cancelTokenSource.Cancel();
            }
            else e.Cancel = true;
        }
    }
}
