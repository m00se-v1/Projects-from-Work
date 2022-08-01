using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Forms;
using System.Windows.Threading;
using System.Net.NetworkInformation;
using System.IO;
using System.Globalization;
using RohdeSchwarz.RsPwrMeter;
using System.Threading;
using CsvHelper;


namespace Power_Sensor_Validation
{
    public partial class MainWindow : Window
    {
        #region Variables and New Instances
        DispatcherTimer pingTime = new DispatcherTimer();
        Ping ping = new Ping();
        public static string pwrMtrAddress = "10.0.0.25";
        public PingReply reply;
        public int errorMessageCount = 0;
        OpenFileDialog openFile = new OpenFileDialog();
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        public static string fileSaveDir;
        public static string biannualCalStatus;
        public System.Data.DataTable dt = new System.Data.DataTable();
        Thread hardestWorker;
        #endregion

        #region UI Controls
        public MainWindow()
        {
            InitializeComponent();
            
        }

        public void TheWindow_Initialized(object sender, EventArgs e)
        {
            pingTime.Interval = new TimeSpan(0, 0, 0, 1, 500);
            pingTime.Start();
            pingTime.Tick += new EventHandler(pwrMeterTime);

        }

        private void dataGrid_Initialized(object sender, EventArgs e)
        {
            
            dt.Columns.Add("Bi-Annual Calibration Status");
            dt.Columns.Add("Power (dBm)");
            dt.Columns.Add("Frequency");
            dt.Columns.Add("Value (dB)");
            dt.Columns.Add("Delta");
        }

        private void fileSave_Click(object sender, RoutedEventArgs e)
        {

            if (biannualCalStatus == "Before Bi-Annual Calibration")
            {
                CsvCreate();
                actualStatus.Content = "File has been saved!";
            }
            else if (biannualCalStatus == "After Bi-Annual Calibration")
            {

                if (File.Exists(fileDir.Text))
                {
                    CsvAppend();
                    actualStatus.Content = "File has been appended!";
                }
                else
                {
                    System.Windows.MessageBox.Show("File not found.", "Error");
                }


            }
        }

        private void checkConnection_Click(object sender, RoutedEventArgs e)
        {
            TheWindow_Initialized(sender, e);
        }

        private void comboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBox.Text == "After Bi-Annual Calibration")
            {
                saveDir.Content = "File Name:";
                fileBrowse.Visibility = Visibility.Hidden;
                fileSelect.Visibility = Visibility.Visible;
            }
            else
            {
                if (fileBrowse.Visibility != Visibility.Visible)
                {
                    saveDir.Content = "File Save Directory:";
                    fileBrowse.Visibility = Visibility.Visible;
                    fileSelect.Visibility = Visibility.Hidden;
                }
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fileSave.IsEnabled)
            {
                fileSave.IsEnabled = false;
            }
        }

        private void sensorModelBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (fileSave.IsEnabled)
            {
                fileSave.IsEnabled = false;
            }
        }

        private void sensorSNBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (fileSave.IsEnabled)
            {
                fileSave.IsEnabled = false;
            }
        }

        private void fileSelect_Click(object sender, RoutedEventArgs e)
        {
            
            openFile.ShowDialog();
            fileDir.Text = openFile.FileName;
            StreamReader reader = new StreamReader(openFile.FileName);
            while(!reader.EndOfStream)
            {
                string allText = reader.ReadToEnd().ToString();
                string[] rows = allText.Split('\n');
                for(int i=0;i<rows.Count()-1;i++)
                {
                    string[] rowValues = rows[i].Split(',');
                    {
                        if(i==0)
                        {
                            for(int j=0;j<rowValues.Count();j++)
                            {
                                dt.Columns.Add(rowValues[j]);
                            }
                        }
                        else
                        {
                            System.Data.DataRow dR = dt.NewRow();
                            for(int k=0;k<rowValues.Count();k++)
                            {
                                dR[k] = rowValues[k].ToString();
                            }
                            dt.Rows.Add(dR);
                        }
                    }
                }
            }

        }

        

        public void fileBrowse_Click(object sender, RoutedEventArgs e)
        {
            folderBrowserDialog.ShowDialog();
            fileDir.Text = folderBrowserDialog.SelectedPath;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Program prog = new Program();
            actualStatus.Content = "Starting...";
            hardestWorker = new Thread(prog.MainBody);
            hardestWorker.Start();
        }

        #endregion
        public void pwrMeterConnection()
        {
            pingTime.Stop();
            bool isPresent = false;
            reply = ping.Send("10.0.0.25");
            isPresent = reply.Status == IPStatus.Success;
            if (isPresent)
            {
                errorMessageCount = 0;
                indicator.Fill = Brushes.Green;
                pingTime.Start();
            }
            else
            {
                pingTime.Stop();
                indicator.Fill = Brushes.Red;
                if (errorMessageCount == 0)
                {
                    errorMessageCount = 1;
                    System.Windows.MessageBox.Show("Please make sure the NRX power meter is connected to the rack's network switch via ethernet cable.\n\nOnce you are sure the power meter is connected, press the Sync button.", "Error");
                }
            }
            
        }
        public void pwrMeterTime(object sender, EventArgs e)
        {
            pwrMeterConnection();
        }
        public void CsvCreate()
        {
            
            var dataWriter = File.AppendText(fileSaveDir + @"\" + Program.FileName());
            CsvWriter csv = new CsvWriter(dataWriter, CultureInfo.InvariantCulture);
            foreach (System.Data.DataColumn column in dt.Columns)
            {
                csv.WriteField(column.ColumnName);
            }
            csv.NextRecord();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    csv.WriteField(row[i]);
                }
                csv.NextRecord();
            }
            dataWriter.Close();

        }
        public void CsvAppend()
        {
            
            var dataWriter = File.AppendText(fileSaveDir + @"\" + Program.FileName()) ;
            CsvWriter csv = new CsvWriter(dataWriter, CultureInfo.InvariantCulture);

            foreach (System.Data.DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    csv.WriteField(row[i]);
                }
                csv.NextRecord();
            }
            dataWriter.Close();
        }

    }
}


