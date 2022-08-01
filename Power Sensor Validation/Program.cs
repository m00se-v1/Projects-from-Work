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
    internal class Program
    {
        #region Variables and New Instances
        MainWindow mW = new MainWindow();
        DispatcherTimer pingTime = new DispatcherTimer();
        OpenFileDialog openFile = new OpenFileDialog();
       
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        public static string powerSensorModel;
        public static int powerSensorSN;
        public static string fileSaveDir;
        public static string biannualCalStatus;
        public string delta;
        RsPwrMeter NRX;
        double[] testResult;
        CultureInfo cultEN_US = new CultureInfo("en-US");
        public int[] powerLevels = {
            -20,
            -10,
            0
        };

        public long[] freqREF = {
            1000000000,
            2000000000,
            3000000000,
            4000000000,
            5000000000,
            6000000000
        };
        public string[] freqSTR =
        {
            "1.0000 GHz",
            "2.0000 GHz",
            "3.0000 GHz",
            "4.0000 GHz",
            "5.0000 GHz",
            "6.0000 GHz"
        };

        #endregion


        public void MainBody()
        {

            pingTime.Stop();
            mW.Dispatcher.Invoke(() =>
            {
                if (mW.sensorModelBox.Text.ToUpper().Contains("NRP") && (Convert.ToInt32(mW.sensorSNBox.Text) > 100000) && mW.fileDir.Text != "" && biannualCalStatus != "")
                {
                    try
                    {

                        powerSensorModel = mW.sensorModelBox.Text.ToUpper();
                        powerSensorSN = Convert.ToInt32(mW.sensorSNBox.Text);
                        fileSaveDir = mW.fileDir.Text;
                        biannualCalStatus = mW.comboBox.Text;
                        Run();
                    }
                    catch (Ivi.Driver.IOException e)
                    {
                        System.Windows.MessageBox.Show("Something went wrong.\n\n" + e.Message, "Oops", MessageBoxButton.OK);
                        if (Convert.ToBoolean(MessageBoxResult.OK))
                        {
                            mW.Close();
                        }
                    }

                }
                else
                {
                    System.Windows.MessageBox.Show("Please make sure that:\n\nThe power sensor model is in the format of NRP-XXX or NRPXXX\nThe power sensor's serial number is correct\nYou've specified a location for the data to be saved\nYou've selected a reason for validation.", "Error");
                    mW.actualStatus.Content = "Not Testing";
                }
                mW.actualStatus.Content = "Done";
                mW.fileSave.IsEnabled = true;
                pingTime.Start();
                EnableAllControls();
            });

        }

        public static string FileName() => $"{powerSensorModel}-{powerSensorSN}-{DateTime.Now.Year}.csv";

        #region Main Process
        public void Run()
        {
            
                DisableAllControls();
                if (mW.dt.Rows.Count > 0)
                {
                    mW.dt.Rows.Clear();
                }


                try
                {
                    NRX = new RsPwrMeter("TCPIP::10.0.0.25::INSTR", true, true, "Simulate=False");
                    try
                    {
                        NRX.Utility.Reset();
                        NRX.UtilityFunctions.OPCTimeout = 6000;
                        NRX.UtilityFunctions.SelfTest();
                        mW.dataGrid.ItemsSource = mW.dt.DefaultView;
                        NRX.Channel["CH1"].Zero();
                        NRX.Measurement.Channel["CH1"].Continuous = false;
                        NRX.Channel["CH1"].Mode = MeasurementMode.ContAv;
                        NRX.Channel["CH1"].Averaging.Enabled = true;
                        NRX.System.WriteString("SOURce:RF:FREQuency:VALue 1.0e9\n");
                        for (int i = 0; i < powerLevels.Length; i++)
                        {
                            NRX.System.WriteString("SOURce:POWer:VALue " + powerLevels[i] + "\n");


                            for (int j = 0; j < freqSTR.Length; j++)
                            {
                                NRX.System.WriteString("OUTPut:SOURce:STATe 1\n");
                                NRX.Channel["CH1"].Frequency = freqREF[j];
                                NRX.Channel["CH1"].Trigger.Source = RohdeSchwarz.RsPwrMeter.TriggerSource.Bus;
                                NRX.Measurement.Channel["CH1"].InitiateWait();
                                //NRX.Measurement.SendSoftwareTrigger();
                                testResult = NRX.Measurement.Fetch();
                                delta = (testResult[0] - powerLevels[i]).ToString("0.000", cultEN_US);
                                mW.dt.Rows.Add(biannualCalStatus, powerLevels[i], freqSTR[j], testResult[0].ToString("0.000", cultEN_US), delta);
                                NRX.System.WriteString("OUTPut:SOURce:STATe 0\n");
                            }
                        }
                        NRX.Utility.Reset();
                        NRX.Dispose();

                    }
                    catch (Ivi.Driver.IOException e)
                    {
                        System.Windows.MessageBox.Show(e.Message, "Error");
                    }
                }
                catch (Ivi.Driver.IOException e)
                {
                    System.Windows.MessageBox.Show(e.Message, "Communication Error");
                }
            

        }
        #endregion

        public void DisableAllControls()
        {
            if (mW.fileSave.IsEnabled)
            {
                mW.Play.IsEnabled = false;
                mW.fileSave.IsEnabled = false;
                mW.sensorSNBox.IsEnabled = false;
                mW.sensorModelBox.IsEnabled = false;
                mW.fileDir.IsEnabled = false;
                mW.checkConnection.IsEnabled = false;
                mW.comboBox.IsEnabled = false;
            }
            else
            {
                mW.Play.IsEnabled = false;
                mW.sensorSNBox.IsEnabled = false;
                mW.sensorModelBox.IsEnabled = false;
                mW.fileDir.IsEnabled = false;
                mW.checkConnection.IsEnabled = false;
                mW.comboBox.IsEnabled = false;
            }

        }

        public void EnableAllControls()
        {
            mW.Play.IsEnabled = true;
            mW.fileSave.IsEnabled = true;
            mW.sensorSNBox.IsEnabled = true;
            mW.sensorModelBox.IsEnabled = true;
            mW.fileDir.IsEnabled = true;
            mW.checkConnection.IsEnabled = true;
            mW.comboBox.IsEnabled = true;
        }
    }

}
