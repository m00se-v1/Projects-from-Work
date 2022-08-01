//using System;
//using System.IO;
//using System.Globalization;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.Timers;
//using System.Windows.Threading;
//using CsvHelper;
//using CsvHelper.Configuration;

//namespace Test
//{
//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        public DispatcherTimer pingTime = new DispatcherTimer();
//        public int errorMessageCount = 0;
//        public MainWindow()
//        {
//            InitializeComponent();
//        }
//        public void TheWindow_Initialized(object sender, EventArgs e)
//        {
//            pingTime.Interval = new TimeSpan(0, 0, 0, 1, 500);
//            pingTime.Start();
//            pingTime.Tick += new EventHandler(pwrMtrTime);
//        }
//        public void pwrMtrTime(object sender, EventArgs e)
//        {
//            pwrMtrCheck();
//        }
//        public void pwrMtrCheck()
//        {
//            bool isConnected = false;
//            isConnected = Convert.ToBoolean(checkeD.IsChecked);
//            if (isConnected)
//            {
//                errorMessageCount = 0;
//                indicator.Fill = Brushes.Green;
//                if (Convert.ToBoolean(pingTime.IsEnabled))
//                {
//                    pingTime.Start();
//                }
//            }
//            else
//            {
//                indicator.Fill = Brushes.Red;
//                if (errorMessageCount == 0)
//                {
//                    MessageBox.Show("Error", "Error");
//                    errorMessageCount++;
//                }
//                pingTime.Stop();
//            }
//        }

//        private void sync_Click(object sender, RoutedEventArgs e)
//        {
//            pingTime.Start();
//            pingTime.Tick += new EventHandler(pwrMtrTime);
//            pwrMtrCheck();

//        }
//        public static void CsvCreate()
//        {
//            var csvPath = System.IO.Path.Combine(Environment.CurrentDirectory, $"rockets-{DateTime.Now.ToFileTime()}.csv");
//            using (var streamWriter = new StreamWriter(csvPath))
//            {
//                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
//                {
//                    var rockets = PowerMeterData.GetValidationData();
//                    csvWriter.Context.RegisterClassMap<PowerMeterDataClassMap>();
//                    csvWriter.WriteRecords(rockets);
//                }
//            }
//        }
//    }
//    public class PowerMeterData
//    {
//        public string biannualCalStatus { get; set; }
//        public int powerdBm { get; set; }
//        public float frequencyMHz { get; set; }
//        public float actualData { get; set; }
//        public static List<PowerMeterData> GetValidationData()
//        {
//            return new List<PowerMeterData>
//            {

//            };
//        }
//    }
//    public class PowerMeterDataClassMap : ClassMap<PowerMeterData>
//    {
//        public PowerMeterDataClassMap()
//        {

//        }
//    }
//}