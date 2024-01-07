using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace 徐然编辑先生
{
    /// <summary>
    /// listEdit.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        JToken data;
        string type;
        int index;
        public string jsonstr = null;
        public Window1(JToken data, string type, int index)
        {
            InitializeComponent();
            this.data = data;
            this.type = type;
            this.index = index;
            this.Title = type + " - " + index;
            rtb.AppendText(data[index].ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string jsonString = new TextRange(rtb.Document.ContentStart,
                rtb.Document.ContentEnd).Text.ToString();
            try
            {
                var obj = JsonConvert.DeserializeObject(jsonString);
                jsonstr = jsonString;
                Close();
            }
            catch (JsonReaderException)
            {
                System.Windows.MessageBox.Show("格式错误，请检查后重新修改", "警告", MessageBoxButton.OK);
            }
        }
    }
}
