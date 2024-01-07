using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static System.Resources.ResXFileRef;
using System.Reflection;
using Microsoft.VisualBasic;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using static System.Windows.Forms.AxHost;
using System.Xml.Linq;
using System.Windows.Shapes;

namespace 徐然编辑先生
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string configPath = Environment.CurrentDirectory + "\\config.xml";
        XDocument xd = null;
        JObject o = null;
        JArray data = null;
        JObject map = new JObject();
        int select = -1;
        int checkSelect = -1;
        int comSelect = -1;
        string jfile = "";
        public ObservableCollection<ListItem> Items { get; set; }
        public class ListItem
        {
            public string Text { get; set; }
            public SolidColorBrush TextColor { get; set; }
            public string tag { get; set; }
            public FontWeight FW { get; set; }
        }
        private void opentask(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();  //显示选择文件对话框
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Title = "打开任务文件";
            openFileDialog1.Filter = "json file (*.json)|*.json"; //所有的文件格式
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                jfile = openFileDialog1.FileName;
                reload();
            }
        }
        private void savetask(object sender, RoutedEventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();  //显示选择文件对话框
            saveFileDialog1.InitialDirectory = "c:\\";
            saveFileDialog1.Title = "保存任务文件";
            saveFileDialog1.DefaultExt = ".json";
            saveFileDialog1.Filter = "json file (*.json)|*.json"; //所有的文件格式
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, o.ToString());
            }
        }
        private void createEmptyData(object sender, RoutedEventArgs e)
        {
            o = JObject.Parse("{\"current\": \"\",\"current_b\": \"\",\"config\": {\"is_state\": false},\"_data\": [],\"_branch_data\": [],\"_connect\": {},\"_connect_branch\": {},\"extend_struct\": {}}");
            reload();
        }
        public void reload(bool is_real = true)
        {
            if (jfile.Length > 0)
            {
                //写配置
                XElement root = xd.Element("root");
                XElement path = root.Element("path");
                path.SetValue(jfile.ToString());
                xd.Save(configPath);
                if (is_real)
                {
                    using StreamReader reader = File.OpenText(jfile.ToString());
                    o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    // 初始化数据
                    map = new JObject();
                    Items = new ObservableCollection<ListItem>();
                }
                else
                {
                    Items.Clear();
                }
                foreach (JProperty item in o.Properties())
                    {
                        if (item.Name == "_data")
                        {
                            var ja = JArray.Parse(item.Value.ToString());
                            data = ja;
                            int index = 0;
                            foreach (var sitem in ja)
                            {
                                if (sitem.Type == JTokenType.Object)
                                {
                                    JObject ss = (JObject)sitem;
                                    map[(string)ss.GetValue("title")] = index;
                                    Items.Add(new ListItem { Text = (string)ss.GetValue("title"), tag = (string)ss.GetValue("tag"), TextColor = System.Windows.Media.Brushes.Black, FW = FontWeights.Normal });
                                }
                                index++;
                            }
                        }
                    }
                // 绑定数据清空
                listbox1.ItemsSource = null;
                if (select != -1)
                {
                    string tag = Items[select].tag;
                    JObject _c = (JObject)o.Value<JToken>("_connect");
                    string to = _c.Value<string>(tag);
                    //刷新链接颜色
                    for (int i = 0; i < Items.Count; i++)
                    {
                        string tagn = Items[i].tag;
                        // 当前对象
                        ListItem sub = Items[i];
                        if (to != null && tagn == to)
                        {
                            sub.FW = FontWeights.Bold;
                            sub.TextColor = System.Windows.Media.Brushes.Green;
                        }
                        else if (tag == tagn)
                        {
                            sub.FW = FontWeights.Bold;
                            sub.TextColor = System.Windows.Media.Brushes.Black;
                        }
                        else
                        {
                            sub.FW = FontWeights.Normal;
                            sub.TextColor = System.Windows.Media.Brushes.Black;
                        }
                    }
                }
                // 绑定数据到 ListBox
                listbox1.ItemsSource = Items;
                if(select == -1)
                {
                    XRcontainer.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                listbox1.Items.Clear();
                XRcontainer.Visibility = Visibility.Collapsed;
            }
        }
        ContextMenuStrip contextMenuStrip = null;
        public MainWindow()
        {
            InitializeComponent();
            // 配置文件
            //读
            try
            {
                // 存在
                xd = XDocument.Load(configPath);
                XElement root = xd.Element("root");
                XElement path = root?.Element("path");
                if (path != null && path.Value.Length > 0)
                {
                    jfile = path.Value;
                }
            }
            catch (Exception ex)
            {
                // 不存在
                xd = new XDocument(new XElement("root", new XElement("path", "")));
            }
            // 加载数据
            reload();
            // 加载右键菜单
            contextMenuStrip = new ContextMenuStrip();
        }
        public void UpdateTask()
        {
            if (select != -1 && o != null)
            {
                if (XRcontainer.Visibility != Visibility.Visible)
                {
                    XRcontainer.Visibility = Visibility.Visible;
                }
                JToken dataToken = o["_data"][select];
                JToken conToken = o["_connect"][dataToken.Value<string>("tag")];
                tag.Content = "任务标识：" + dataToken.Value<string>("tag");
                title.Content = "任务标题：" + dataToken.Value<string>("title");
                state.Content = "任务状态：" + dataToken.Value<string>("state");
                connect.Content = "连接任务：" + (conToken != null ? conToken : "未连接");
                JArray cList = dataToken.Value<JArray>("item");
                JArray cmList = dataToken.Value<JArray>("complete_item");
                // 初始化
                checkList.Items.Clear();
                comList.Items.Clear();
                checkList.SelectedIndex = -1;
                comList.SelectedIndex = -1;
                // 检测列表刷新
                foreach (JObject ss in cList)
                {
                    checkList.Items.Add("ID:" + ss.GetValue("id") + "\n类型：" + ss.GetValue("type")
                        + "\n数量：" + (ss.GetValue("num") == null ? "未定义" : ss.GetValue("num")));
                }

                // 完成列表刷新
                foreach (JObject ss in cmList)
                {
                    comList.Items.Add("ID:" + ss.GetValue("id") + "\n类型：" + ss.GetValue("type") + "\n数量：" + ss.GetValue("num"));
                }
            }
        }
        private void listbox1_DC(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (listbox1.SelectedIndex != select)
                {
                    select = listbox1.SelectedIndex;
                    UpdateTask();
                    // 通过代码修改某项的文字颜色
                    if (listbox1.SelectedItem is ListItem selectedItem)
                    {
                        selectedItem.FW = FontWeights.Bold;
                        selectedItem.TextColor = System.Windows.Media.Brushes.Red;  // 修改颜色为红色
                        reload(false);
                    }
                }
            }
            else if (listbox1.SelectedIndex != -1 && e.RightButton == MouseButtonState.Pressed)
            {
                JToken dataToken = o["_data"][listbox1.SelectedIndex];
                string s = "" + dataToken.Value<string>("tag");
                contextMenuStrip.Items.Clear();
                contextMenuStrip.Items.Add(new ToolStripMenuItem("当前选择：" + s));
                ToolStripMenuItem new_obj = new ToolStripMenuItem("新建空白任务");
                new_obj.Click += createEmptyTask;
                contextMenuStrip.Items.Add(new_obj);
                contextMenuStrip.Items.Add(new ToolStripMenuItem("复制"));
                contextMenuStrip.Items.Add(new ToolStripMenuItem("删除"));
                // 加载右键菜单
                contextMenuStrip.Show(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
            }
        }
        private void listbox1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (listbox1.SelectedIndex != -1)
            {
                JToken dataToken = o["_data"][listbox1.SelectedIndex];
                string s = "" + dataToken.Value<string>("tag");
                contextMenuStrip.Items.Clear();
                contextMenuStrip.Items.Add(new ToolStripMenuItem("当前选择：" + s));
                ToolStripMenuItem new_obj = new ToolStripMenuItem("新建空白任务");
                new_obj.Click += createEmptyTask;
                contextMenuStrip.Items.Add(new_obj);
                contextMenuStrip.Items.Add(new ToolStripMenuItem("复制"));
                contextMenuStrip.Items.Add(new ToolStripMenuItem("删除"));
                // 加载右键菜单
                contextMenuStrip.Show(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
            }
        }

        private void createEmptyTask(object? sender, EventArgs e)
        {
            string Id = Guid.NewGuid().ToString();
            Id = Id.Substring(0, 8);
            JArray _data = (JArray)o["_data"];
            _data.Add(JObject.Parse("{\"title\": \"默认任务" + Id + "\",\"desc\": \"\",\"tag\": \"tag" + Id
                + "\",\"state\": false,\"item\": [],\"complete_item\": []}"));
            reload(false);
        }
        private void checkList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (checkList.SelectedIndex != -1)
            {
                JToken dataToken = o["_data"][select];
                checkSelect = checkList.SelectedIndex;
                // 检查事件
                Window1 w1 = new(data: dataToken.Value<JToken>("item"), type: "item", index: checkList.SelectedIndex);
                w1.WindowStartupLocation = (WindowStartupLocation)FormStartPosition.CenterScreen;
                w1.ShowDialog();
                if (w1.jsonstr != null)
                {
                    o["_data"][select]["item"][checkList.SelectedIndex] = JToken.Parse(w1.jsonstr);
                    UpdateTask();
                }
            }
        }

        private void comList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (comList.SelectedIndex != -1)
            {
                JToken dataToken = o["_data"][select];
                checkSelect = comList.SelectedIndex;
                // 检查事件
                Window1 w1 = new(data: dataToken.Value<JToken>("complete_item"), type: "complete_item", index: comList.SelectedIndex);
                w1.WindowStartupLocation = (WindowStartupLocation)FormStartPosition.CenterScreen;
                w1.ShowDialog();
                if (w1.jsonstr != null)
                {
                    o["_data"][select]["complete_item"][comList.SelectedIndex] = JToken.Parse(w1.jsonstr);
                    UpdateTask();
                }
            }
        }

        private void about(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("作者：徐然\nQQ：1783558957\nGitHub：github.com/xiaoxustudio","关于我们");
        }
    }
}
