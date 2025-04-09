using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Forms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using WMPLib;

namespace OnlineRadio
{
    class Station
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }


    public partial class MainWindow : Window
    {
        WindowsMediaPlayer wmp = new WindowsMediaPlayer();
        private bool isPause = false;
        ImageBrush imgBrushPause = new ImageBrush();
        ImageBrush imgBrushPlay = new ImageBrush();
        DateTime time;
        DispatcherTimer timer = new DispatcherTimer();
        RegistryKey? newkey;
        Forms.NotifyIcon? notifyIcon;
        Forms.ContextMenuStrip menuStrip = new Forms.ContextMenuStrip();
        private int lastVolume = 0;
        public MainWindow()
        {
            InitializeComponent();
            //Write();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listBoxStations.SelectedValuePath = "Value";
            listBoxStations.DisplayMemberPath = "Name";
            InitImages();
            InitNotifyIcon();
            LoadStations();
            RegEdit();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (_, _) =>
            {
                time = time.AddSeconds(1);
                textBlockTime.Text = time.ToString("HH:mm:ss");
            };
        }

        private void InitImages()
        {
            imgBrushPause.ImageSource = new BitmapImage(new Uri("resources/pause.png", UriKind.Relative));
            imgBrushPlay.ImageSource = new BitmapImage(new Uri("resources/play.png", UriKind.Relative));
            btnPlay.Background = imgBrushPlay;
            btnPrev.Background = new ImageBrush(new BitmapImage(new Uri("resources/prev.png", UriKind.Relative)));
            btnNext.Background = new ImageBrush(new BitmapImage(new Uri("resources/next.png", UriKind.Relative)));
        }

        private void InitNotifyIcon()
        {
            notifyIcon = new Forms.NotifyIcon();
            notifyIcon.BalloonTipText = "I'm here!";
            notifyIcon.Icon = new System.Drawing.Icon("resources/icon.ico");
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            notifyIcon.ContextMenuStrip = menuStrip;
            menuStrip.Items.Add("Current_station");
            menuStrip.Items.Add("Next Station", null, ContextMenuNext);
            menuStrip.Items.Add("Prev Station", null, ContextMenuPrev);
            menuStrip.Items.Add("Volume +25%", null, ContextMenuVolumeUp);
            menuStrip.Items.Add("Volume -25%", null, ContextMenuVolumeDown);
            menuStrip.Items.Add("Mute", null, ContextMenuMute);
            menuStrip.Items.Add("Close", null, ContextMenuCloseWindow);
            menuStrip.Items[0].BackColor = System.Drawing.Color.LawnGreen;
        }

        private void ContextMenuMute(object? sender, EventArgs e)
        {
            btnMute_Click(null, null);
        }

        private void ContextMenuVolumeDown(object? sender, EventArgs e)
        {
            if (sliderVolume.Value - 25 >= 0)
                sliderVolume.Value -= 25;
            else
                sliderVolume.Value = 0;
        }
        private void ContextMenuVolumeUp(object? sender, EventArgs e)
        {
            if (sliderVolume.Value + 25 <= 100)
                sliderVolume.Value += 25;
            else
                sliderVolume.Value = 100;
        }

        private void ContextMenuPrev(object? sender, EventArgs e)
        {
            btnPrev_Click(null, null);
        }
        private void ContextMenuNext(object? sender, EventArgs e)
        {
            btnNext_Click(null, null);
        }
        private void ContextMenuCloseWindow(object? sender, EventArgs e)
        {
            Close();
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
            notifyIcon.Visible = false;
            WindowState = WindowState.Normal;
            Focus();
        }

        private void RegEdit()
        {
            newkey = Registry.LocalMachine.OpenSubKey("Software\\MyRadio", true);
            if (newkey == null)
            {
                newkey = Registry.LocalMachine.OpenSubKey("Software", true);
                newkey.CreateSubKey("MyRadio");
                newkey = Registry.LocalMachine.OpenSubKey("Software\\MyRadio", true);
                newkey.SetValue("Volume", 100, RegistryValueKind.String);
                newkey.SetValue("CurrentWave", "", RegistryValueKind.String);
                newkey.SetValue("Top", SystemParameters.FullPrimaryScreenHeight - Height + 30, RegistryValueKind.String);
                newkey.SetValue("Left", SystemParameters.FullPrimaryScreenWidth - Width + 5, RegistryValueKind.String);
                newkey.SetValue("Width", Width, RegistryValueKind.String);
                newkey.SetValue("Height", Height, RegistryValueKind.String);
                newkey.SetValue("Topmost", Topmost, RegistryValueKind.String);
            }
            else
            {
                sliderVolume.Value = Convert.ToDouble(newkey.GetValue("Volume"));
                Top = Convert.ToDouble(newkey.GetValue("Top"));
                Left = Convert.ToDouble(newkey.GetValue("Left"));
                Width = Convert.ToDouble(newkey.GetValue("Width"));
                Height = Convert.ToDouble(newkey.GetValue("Height"));
                string top = (string)newkey.GetValue("Topmost");
                btnTopmost.Background = top == "True" ? Brushes.Green : Brushes.Crimson;
                Topmost = top == "True" ? true : false;
                foreach (var item in listBoxStations.Items)
                {
                    Station station = (Station)item;
                    if (station.Value == (string)newkey.GetValue("CurrentWave"))
                    {
                        listBoxStations.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void Write()
        {
            XmlWriter writer;

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;

            settings.NewLineChars = "\r\n";

            settings.Encoding = Encoding.ASCII;

            settings.NewLineOnAttributes = false;

            writer = XmlWriter.Create("stations.xml", settings);

            writer.WriteStartDocument();

            writer.WriteStartElement("body");

            writer.WriteStartElement("station");

            writer.WriteAttributeString("Retro", "https://retro.hostingradio.ru:8014/retro320.mp3");

            writer.WriteEndElement();

            writer.WriteStartElement("station");

            writer.WriteAttributeString("Shanson", "https://chanson.hostingradio.ru:8041/chanson256.mp3");

            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();

        }

        private void LoadStations()
        {
            XmlReader reader;

            reader = XmlReader.Create("resources/stations.xml");

            //по одному тегу
            while(reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element)
                {
                    if(reader.HasAttributes)
                    {
                        reader.MoveToFirstAttribute();
                        Station station = new Station() { Name = reader.Name, Value = reader.Value };
                        listBoxStations.Items.Add(station);
                        while (reader.MoveToNextAttribute())
                        {
                            station = new Station() { Name = reader.Name, Value = reader[reader.Name] };
                            listBoxStations.Items.Add(station);
                        }
                    }
                }
                listBoxStations.SelectedIndex = 0;
            }

            //if (System.IO.File.Exists(fileName))
            //{
            //    XmlDocument xmlDoc = new XmlDocument();
            //    xmlDoc.Load(fileName);
            //    XmlNodeList stationNodes = xmlDoc.SelectNodes("//station");
            //    foreach (XmlNode stationNode in stationNodes)
            //    {
            //        Station station = new Station() { Name = stationNode.Name, Value = stationNode.Value };
            //        listBoxStations.Items.Add(station);
            //    }
            //    xmlDoc.Save(fileName);
            //}
            //else
            //{
            //    MessageBox.Show("Не найдены радиостанции.");
            //}

        }

        private void listBoxStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Play();
            Station station = (Station)listBoxStations.SelectedItem;
            textBlockName.Text = station.Name;
            listBoxStations.ScrollIntoView(listBoxStations.SelectedItem);
            time = new DateTime(2023, 03, 19, 00, 0, 0);
            timer.Start();
            menuStrip.Items[0].Text = "Plays: " + station.Name;
        }

        private void Play()
        {
            if (wmp.playState == WMPPlayState.wmppsPlaying) //если плеер играет
            {
                wmp.controls.stop(); 
            }
            wmp.URL = (string)listBoxStations.SelectedValue;
            wmp.controls.play();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            wmp.close();
            newkey.SetValue("Volume", sliderVolume.Value, RegistryValueKind.String);
            newkey.SetValue("CurrentWave", listBoxStations.SelectedValue, RegistryValueKind.String);
            newkey.SetValue("Top", Top, RegistryValueKind.String);
            newkey.SetValue("Left", Left, RegistryValueKind.String);
            newkey.SetValue("Width", Width, RegistryValueKind.String);
            newkey.SetValue("Height", Height, RegistryValueKind.String);
            newkey.SetValue("Topmost", Topmost, RegistryValueKind.String);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if(!isPause)
            {
                if (wmp.playState == WMPPlayState.wmppsPlaying)
                {
                    wmp.controls.stop();
                }
                btnPlay.Background = imgBrushPause;
                isPause = true;
                timer.Stop();
            }
            else
            {
                Play();
                btnPlay.Background = imgBrushPlay;
                isPause = false;
                timer.Start();
            }

           
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                notifyIcon.Visible = true;
                Visibility = Visibility.Collapsed;
                notifyIcon.ShowBalloonTip(1000);
            }
        }


        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxStations.SelectedIndex == 0)
            {
                listBoxStations.SelectedIndex = listBoxStations.Items.Count - 1;

                var border = (Border)VisualTreeHelper.GetChild(listBoxStations, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
            else
                listBoxStations.SelectedIndex--;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxStations.SelectedIndex == listBoxStations.Items.Count-1)
            {
                listBoxStations.SelectedIndex = 0;
                var border = (Border)VisualTreeHelper.GetChild(listBoxStations, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToTop();
            }
            else
                listBoxStations.SelectedIndex++;
        }

        private void btnTopmost_Click(object sender, RoutedEventArgs e)
        {
            if (Topmost == true)
            {
                btnTopmost.Background = Brushes.Crimson;
                Topmost = false;
            }
            else
            {
                Topmost = true;
                btnTopmost.Background = Brushes.Green;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
            {
                if (sliderVolume.Value <= 100)
                    sliderVolume.Value+=2;
            }
            else
            {
                if (sliderVolume.Value >= 0)
                    sliderVolume.Value-=2;
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Up:
                    if (sliderVolume.Value <= 100)
                        sliderVolume.Value += 2;
                    else
                        sliderVolume.Value = 100;
                    break;
                case Key.Down:
                    if (sliderVolume.Value >= 0)
                        sliderVolume.Value -= 2;
                    else
                        sliderVolume.Value = 0;
                    break;
                case Key.Left:
                    btnPrev_Click(null, null);
                    break;
                case Key.Right:
                    btnNext_Click(null, null);
                    break;
            }
            e.Handled = true;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (Top + Height + 30 >= SystemParameters.FullPrimaryScreenHeight && Left + Width + 30 >= SystemParameters.FullPrimaryScreenWidth)
            {
                Top = SystemParameters.FullPrimaryScreenHeight - Height + 30;
                Left = SystemParameters.FullPrimaryScreenWidth - Width + 10;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            wmp.settings.volume = (int)sliderVolume.Value;
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            if(sliderVolume.Value > 0)
            {
                lastVolume = (int)sliderVolume.Value;
                sliderVolume.Value = 0;
                btnMute.Background = Brushes.Crimson;
            }
            else
            {
                sliderVolume.Value = lastVolume;
                btnMute.Background = Brushes.Green;
            }

        }
    }
}
