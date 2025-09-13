using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Markup;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using TextBox = System.Windows.Forms.TextBox;



namespace GUI
{
    public partial class Form1 : Form
    {
        private List<Chart> charts;
        List<string> receivedDataList = new List<string>();
        string dataIN;
        private DateTime latestTimestamp = DateTime.MinValue;
        string State;
        string State_Sedang;
        string[] dataArray = new string[9];
        private double time = 0.0;
        //private double amplitude = 50.0;
        private double frequency = 2.0;
        // Define threshold constants
        //const double OvershootThreshold = 2000.0;
        //const double UndershootThreshold = 200.0;
        Dictionary<int, double> latestDataByState = new Dictionary<int, double>();
        // Variables to store overshoot, undershoot, max, and min values for each chart
        private int[] overshootCounts = new int[4];
        private int[] undershootCounts = new int[4];
        private double[] maxValues = new double[4];
        private double[] minValues = new double[4];
        string S_Idle = "0";
        string S_Start = "1";
        string S_Pecah1 = "2";
        string S_Turun = "3";
        string S_SZ1 = "4";
        string S_Kelereng = "5";
        string S_Go3 = "6";
        string S_Pecah2_B3 = "7";
        string S_Pecah2_B4 = "8";
        string S_SZ3 = "9";
        string S_Tangga = "10";
        string S_Atas = "11";
        string S_Go5 = "12";
        string S_SZ5 = "13";
        const string HEADER = "#";
        const string TAIL = "\n";
        private StringBuilder buffer = new StringBuilder();
        UInt16 State_GUI, State_Before, State_Sedangapa;
        string[] data_GUI = new string[9];
        string Lidar_Be, Lidar_De, Lidar_Ka, Lidar_Ki, TF_Bka, TF_Bki, TF_Dka, TF_Dki, US_Be, Lidar_Count_V_Be;

        public Form1()
        {
            InitializeComponent();
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";
            chart3.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";
            chart4.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";
            charts = new List<Chart> { chart1, chart2, chart3, chart4 };
            serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort1_DataReceived);

            InitializeChartSeries(chart1);
            InitializeChartSeries(chart2);
            InitializeChartSeries(chart3);
            InitializeChartSeries(chart4);

            // Initialize max and min values
            for (int i = 0; i < 4; i++)
            {
                maxValues[i] = double.MinValue;
                minValues[i] = double.MaxValue;
            }
        }

        private void InitializeChartSeries(Chart chart)
        {
            chart.Series.Clear();
            var series = new Series
            {
                Name = "DataSeries",
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Double,
                YValueType = ChartValueType.Double
            };
            chart.Series.Add(series);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cBoxCom.Items.AddRange(ports);
            cBoxBaud.SelectedIndex = 0;
            cBoxData.SelectedIndex = 3;
            cBoxStopBits.SelectedIndex = 0;
            cBoxParityBits.SelectedIndex = 0;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "CONNECT")
            {
                try
                {
                    serialPort1.PortName = cBoxCom.Text;
                    serialPort1.BaudRate = Convert.ToInt32(cBoxBaud.Text);
                    serialPort1.DataBits = Convert.ToInt32(cBoxData.Text);
                    serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                    serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);

                    serialPort1.Open();
                    progressBar1.Value = 100;
                    timer1.Enabled = !(timer1.Enabled);
                    if (timer1.Enabled == false) { timer1.Enabled = true; }
                    btnConnect.Text = "DISCONNECT";
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error Brother", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (btnConnect.Text == "DISCONNECT")
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    progressBar1.Value = 0;
                    if (timer1.Enabled == true) { timer1.Enabled = false; }
                    btnConnect.Text = "CONNECT";
                }
            }

        }


        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string dataIN = serialPort1.ReadExisting();

            Debug.WriteLine($"Data received: {dataIN}");  // Debugging statement

            // Tambahkan data ke buffer
            buffer.Append(dataIN);
            this.Invoke(new Action(() =>
            {
                Data_Mentah.AppendText($"{buffer.ToString()}");
            }));
            // Periksa apakah buffer mengandung header dan tail
            while (buffer.ToString().Contains(HEADER) && buffer.ToString().Contains(TAIL))
            {
                string bufferString = buffer.ToString();
                int startIndex = bufferString.IndexOf(HEADER);
                int endIndex = bufferString.IndexOf(TAIL, startIndex);

                if (endIndex > startIndex)
                {
                    // Ekstrak data aktual
                    string actualData = bufferString.Substring(startIndex + HEADER.Length, endIndex - (startIndex + HEADER.Length));
                    Debug.WriteLine($"Actual data extracted: {actualData}");  // Debugging statement

                    // Hapus paket data yang telah diproses dari buffer
                    buffer.Remove(startIndex, endIndex - startIndex + TAIL.Length);

                    // Bagi data aktual menjadi bagian berdasarkan delimiter ":"
                    data_GUI = actualData.Split(new string[] { ":" }, StringSplitOptions.None);

                    // Panggil metode untuk menampilkan dan memproses data
                    this.Invoke(new EventHandler(ProcessData));
                }
                else
                {
                    // Jika endIndex tidak valid, hentikan loop
                    break;
                }
            }
        }

        private void ProcessData(object sender, EventArgs e)
        {
            try
            {

                //data_GUI = dataIN.Split(new string[] { ":" }, StringSplitOptions.None);
                Debug.WriteLine($"Data received: {dataIN}");  // Debugging statement

                // Memeriksa apakah data_GUI memiliki setidaknya 2 elemen (untuk State dan Data1)
                if (data_GUI.Length >= 9)
                {
                    State = data_GUI[2];
                    if (State == "0") State_GUI = 0;
                    else if (State == "1") State_GUI = 1;
                    else if (State == "2") State_GUI = 2;
                    else if (State == "3") State_GUI = 3;
                    else if (State == "4") State_GUI = 4;
                    else if (State == "5") State_GUI = 5;
                    else if (State == "6") State_GUI = 6;
                    else if (State == "7") State_GUI = 7;
                    else if (State == "8") State_GUI = 8;
                    else if (State == "9") State_GUI = 9;
                    else if (State == "10") State_GUI = 10;
                    else if (State == "11") State_GUI = 11;
                    else if (State == "12") State_GUI = 12;
                    else if (State == "13") State_GUI = 13;

                    State_Sedang = data_GUI[1];
                    if (State_Sedang == "1") State_Sedangapa = 1;
                    else if (State_Sedang == "2") State_Sedangapa = 2;
                    else if (State_Sedang == "3") State_Sedangapa = 3;
                    else if (State_Sedang == "4") State_Sedangapa = 4;
                    else if (State_Sedang == "5") State_Sedangapa = 5;
                }
                DateTime currentTimestamp = DateTime.Now;
                if (currentTimestamp > latestTimestamp)
                {
                    // Update timestamp terbaru
                    latestTimestamp = currentTimestamp;

                    // Memeriksa apakah state ini telah disimpan sebelumnya
                    if (latestDataByState.ContainsKey(State_GUI))
                    {
                        // Jika sudah ada, update nilai terbaru dari state tersebut
                        latestDataByState[State_GUI] = double.Parse(data_GUI[3]); // Misalnya, data_GUI[3] adalah data yang ingin disimpan
                    }
                    else
                    {
                        // Jika belum ada, tambahkan state baru bersama dengan nilai terbarunya
                        latestDataByState.Add(State_GUI, double.Parse(data_GUI[3])); // Misalnya, data_GUI[3] adalah data yang ingin disimpan
                    }
                    // Update UI
                    UpdateStateUI();
                    time += 0.1;
                    UpdateTextBoxes(data_GUI);

                    for (int i = 3; i <= 6; i++)
                    {
                        double dataValue = double.Parse(data_GUI[i]);
                        int chartIndex = i - 3;

                        maxValues[chartIndex] = Math.Max(maxValues[chartIndex], dataValue);
                        minValues[chartIndex] = Math.Min(minValues[chartIndex], dataValue);

                        if (dataValue > 2000)
                        {
                            overshootCounts[chartIndex]++;
                        }
                        if (dataValue < 160)
                        {
                            undershootCounts[chartIndex]++;
                        }

                        // Tambahkan data ke chart
                        if (chart1.Series.Count > 0) chart1.Series[0].Points.AddXY(time, data_GUI[3]);
                        if (chart2.Series.Count > 0) chart2.Series[0].Points.AddXY(time, data_GUI[4]);
                        if (chart3.Series.Count > 0) chart3.Series[0].Points.AddXY(time, data_GUI[5]);
                        if (chart4.Series.Count > 0) chart4.Series[0].Points.AddXY(time, data_GUI[6]);

                        if (State_GUI != State_Before)
                        {
                            ResetChart(chart1);
                            ResetChart(chart2);
                            ResetChart(chart3);
                            ResetChart(chart4);
                            time = 0;
                            charts[chartIndex].Series[0].Points.AddXY(time, dataValue);
                            UpdateDataLog();  // Log the data after processing
                        }



                    }
                    State_Before = State_GUI;


                }
            }




            catch (Exception error)
            {

                //MessageBox.Show(error.Message);
                //Debug.WriteLine(error.Message);
            }
        }

        private void UpdateStateUI()
        {
            switch (State_GUI)
            {
                case 0:
                    Graph_State_GUI.Text = "Idle";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "TF Belakang Kanan";
                    Label_D1.Text = "TF Belakang Kanan :";
                    Graph_D2.Text = "TF Belakang Kiri";
                    Label_D2.Text = "TF Belakang Kiri :";
                    Graph_D3.Text = "None";
                    Label_D3.Text = "None :";
                    Graph_D4.Text = "None";
                    Label_D4.Text = "None :";
                    break;

                case 1:
                    Graph_State_GUI.Text = "Start";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Belakang";
                    Label_D1.Text = "Lidar Belakang :";
                    Graph_D2.Text = "Lidar Kanan";
                    Label_D2.Text = "Lidar Kanan :";
                    Graph_D3.Text = "None";
                    Label_D3.Text = "None :";
                    Graph_D4.Text = "None";
                    Label_D4.Text = "None :";
                    break;
                case 2:
                    Graph_State_GUI.Text = "Jalan Pecah";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Belakang";
                    Label_D1.Text = "Lidar Belakang :";
                    Graph_D2.Text = "Lidar Kanan";
                    Label_D2.Text = "Lidar Kanan :";
                    Graph_D3.Text = "Lidar Kiri";
                    Label_D3.Text = "Lidar Kiri :";
                    Graph_D4.Text = "None";
                    Label_D4.Text = "None :";
                    break;
                case 3:
                    Graph_State_GUI.Text = "Jalan Turun";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Belakang";
                    Label_D1.Text = "Lidar Belakang :";
                    Graph_D2.Text = "Lidar Kanan";
                    Label_D2.Text = "Lidar Kanan :";
                    Graph_D3.Text = "Lidar Kiri";
                    Label_D3.Text = "Lidar Kiri :";
                    Graph_D4.Text = "None";
                    Label_D4.Text = "None :";
                    break;
                case 4:
                    Graph_State_GUI.Text = "Safety Zone 1";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Depan";
                    Label_D1.Text = "Lidar Depan :";
                    Graph_D2.Text = "Lidar Kanan";
                    Label_D2.Text = "Lidar kanan :";
                    Graph_D3.Text = "None";
                    Label_D3.Text = "None :  ";
                    Graph_D4.Text = "None";
                    Label_D4.Text = "None :";
                    break;
                case 5:
                    Graph_State_GUI.Text = "Ruang Kelereng";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Depan";
                    Label_D1.Text = "Lidar Depan :";
                    Graph_D2.Text = "Lidar Kanan";
                    Label_D2.Text = "Lidar Kanan :";
                    Graph_D3.Text = "Lidar Kiri";
                    Label_D3.Text = "Lidar Kiri :";
                    Graph_D4.Text = "None";
                    Label_D4.Text = "None :";
                    break;
                case 6:
                    Graph_State_GUI.Text = "Ruang Go 3";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Depan";
                    Label_D1.Text = "Lidar Depan :";
                    Graph_D2.Text = "Lidar Kanan";
                    Label_D2.Text = "Lidar Kanan :";
                    Graph_D3.Text = "Lidar Kiri";
                    Label_D3.Text = "Lidar Kiri :";
                    Graph_D4.Text = "TF Depan Kanan";
                    Label_D4.Text = "TF Depan Kanan :";
                    break;
                case 7:
                    Graph_State_GUI.Text = "Jalan Pecah Boneka 3";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Kanan";
                    Label_D1.Text = "Lidar Kanan :";
                    Graph_D2.Text = "Lidar Depan";
                    Label_D2.Text = "Lidar Depan :";
                    Graph_D3.Text = "TF Depan Kanan";
                    Label_D3.Text = "TF Depan Kanan :";
                    Graph_D4.Text = "TF Belakang Kanan";
                    Label_D4.Text = "TF Belakang Kanan :";
                    break;
                case 8:
                    Graph_State_GUI.Text = "Jalan Pecah Boneka 4";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Kanan";
                    Label_D1.Text = "Lidar Kanan :";
                    Graph_D2.Text = "Lidar Belakang";
                    Label_D2.Text = "Lidar Belakang";
                    Graph_D3.Text = "US Belakang";
                    Label_D3.Text = "US Belakang :";
                    Graph_D4.Text = "TF Belakang Kanan";
                    Label_D4.Text = "TF Belakang Kanan :";
                    break;
                case 9:
                    Graph_State_GUI.Text = "Safety Zone 3";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Kanan";
                    Label_D1.Text = "Lidar Kanan :";
                    Graph_D2.Text = "Lidar Belakang";
                    Label_D2.Text = "Lidar belakang :";
                    Graph_D3.Text = "US Belakang";
                    Label_D3.Text = "US Belakang :";
                    Graph_D4.Text = "TF Belakang Kanan";
                    Label_D4.Text = "TF Belakang kanan :";
                    break;
                case 10:
                    Graph_State_GUI.Text = "Tangga";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Count Var Belakang";
                    Label_D1.Text = "Lidar Count Var Belakang :";
                    Graph_D2.Text = "Lidar Belakang";
                    Label_D2.Text = "Lidar Belakang :";
                    Graph_D3.Text = "TF Belakang Kanan";
                    Label_D3.Text = "TF Belakang Kanan :";
                    Graph_D4.Text = "Lidar Kiri";
                    Label_D4.Text = "Lidar Kiri :";
                    break;
                case 11:
                    Graph_State_GUI.Text = "Ruang Atas";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Belakang";
                    Label_D1.Text = "Lidar Belakang :";
                    Graph_D2.Text = "Lidar Kiri";
                    Label_D2.Text = "Lidar Kiri :";
                    Graph_D3.Text = "US Belakang";
                    Label_D3.Text = "US Belakang :";
                    Graph_D4.Text = "TF Belakang Kiri";
                    Label_D4.Text = "TF Belakang Kiri :";
                    break;
                case 12:
                    Graph_State_GUI.Text = "Ruang Go 5";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Belakang";
                    Label_D1.Text = "Lidar Belakang :";
                    Graph_D2.Text = "Lidar Kiri";
                    Label_D2.Text = "Lidar kiri :";
                    Graph_D3.Text = "US Belakang";
                    Label_D3.Text = "US Belakang :";
                    Graph_D4.Text = "TF Belakang Kiri";
                    Label_D4.Text = "TF Belakang Kiri :";
                    break;
                case 13:
                    Graph_State_GUI.Text = "Safety Zone 5";
                    tBoxState.Text = Graph_State_GUI.Text;
                    Graph_D1.Text = "Lidar Depan";
                    Label_D1.Text = "Lidar Depan";
                    Graph_D2.Text = "Lidar Kanan";
                    Label_D2.Text = "Lidar Kanan";
                    Graph_D3.Text = "TF Depan Kiri";
                    Label_D3.Text = "TF Depan Kiri";
                    Graph_D4.Text = "None";
                    Label_D4.Text = "None";
                    break;

            }
            switch (State_Sedangapa)
            {
                case 1:
                    tBoxSedang.Text = "Mundur";
                    break;

                case 2:
                    tBoxSedang.Text = "Maju";
                    break;
                case 3:
                    tBoxSedang.Text = "Kiri";
                    break;
                case 4:
                    tBoxSedang.Text = "Kanan";
                    break;
                case 5:
                    tBoxSedang.Text = "Idle";
                    break;
            }

            UpdateDataLog();
        }

        private void UpdateDataLog()
        {
            // Buat variabel untuk menentukan apakah data baru telah ditambahkan
            bool newDataAdded = false;

            // Simpan data sebelum pergantian state terjadi
            string previousState = Graph_State_GUI.Text;

            // Ubah nilai state
            Graph_State_GUI.Text = tBoxState.Text; // Ganti dengan state yang baru

            // Loop melalui setiap state terbaru dalam latestDataByState
            foreach (var state in latestDataByState)
            {
                // Buat string untuk menyimpan log data
                StringBuilder logBuilder = new StringBuilder();

                string[] dataNames;
                switch (Graph_State_GUI.Text)
                {
                    case "Idle":
                        dataNames = new string[] { "TF Belakang Kanan", "TF Belakang Kiri", "None", "None" };
                        break;
                    case "Start":
                        dataNames = new string[] { "Lidar Belakang", "Lidar Kanan", "None", "None" };
                        break;
                    case "Jalan Pecah":
                        dataNames = new string[] { "Lidar Belakang", "Lidar Kanan", "Lidar Kiri", "None" };
                        break;
                    case "Jalan Turun":
                        dataNames = new string[] { "Lidar Belakang", "Lidar kanan", "Lidar Kiri", "None" };
                        break;
                    case "Safety Zone 1":
                        dataNames = new string[] { "Lidar Depan", "Lidar Kanan", "None", "None" };
                        break;
                    case "Ruang Kelereng":
                        dataNames = new string[] { "Lidar Depan", "Lidar Kanan", "Lidar Kiri", "None" };
                        break;
                    case "Ruang Go 3":
                        dataNames = new string[] { "Lidar depan", "Lidar kanan", "Lidar kiri", "None" };
                        break;
                    case "Jalan Pecah Boneka 3":
                        dataNames = new string[] { "Lidar kanan", "Lidar depan", "TF Depan Kanan", "TF belakang kanan" };
                        break;
                    case "Jalan Pecah Boneka 4":
                        dataNames = new string[] { "lidar kanan", "Lidar Belakang", "US Belakang", "TF belakang Kanan" };
                        break;
                    case "Safety Zone 3":
                        dataNames = new string[] { "lidar kanan", "Lidar Belakang", "US Belakang Kanan", "TF Belakang Kanan" };
                        break;
                    case "Tangga":
                        dataNames = new string[] { "Lidar Count Var belakang ", "Lidar belakang ", "Lidar kanan ", "Lidar kiri " };
                        break;
                    case "Ruang Atas":
                        dataNames = new string[] { "Lidar belakang ", "Lidar kiri ", "US belakang ", "TF belakang kiri" };
                        break;
                    case "Ruang Go 5":
                        dataNames = new string[] { "Lidar Belakang", "Lidar kiri", "US Belakang", "TF belakang Kiri" };
                        break;
                    case "Safety Zone 5":
                        dataNames = new string[] { "Lidar Depan", "Lidar kanan", "TF Depan Kiri", "None" };
                        break;

                    // Tambahkan case untuk setiap state lainnya
                    default:
                        dataNames = new string[] { "None", "None", "None", "None" };
                        break;
                }


                // Timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                logBuilder.AppendLine($"Timestamp: {timestamp}");

                // State
                logBuilder.AppendLine($"State: {Graph_State_GUI.Text}");

                // Data
                for (int i = 0; i < 4; i++)
                {
                    logBuilder.AppendLine($"Data {i + 1}: {dataNames[i]}");
                    logBuilder.AppendLine($"  - Max Value: {maxValues[i]}");
                    logBuilder.AppendLine($"  - Min Value: {minValues[i]}");
                    logBuilder.AppendLine($"  - Overshoot Count: {overshootCounts[i]}");
                    logBuilder.AppendLine($"  - Undershoot Count: {undershootCounts[i]}");
                }

                logBuilder.AppendLine("---------------------------------------------");

                // Loop melalui receivedDataList untuk mencari data dengan state yang sama
                for (int index = 0; index < receivedDataList.Count; index++)
                {
                    // Periksa jika ada entri dengan state yang sama
                    if (receivedDataList[index].Contains($"State: {previousState}"))
                    {
                        // Jika ditemukan, ganti entri tersebut dengan data yang baru
                        receivedDataList[index] = logBuilder.ToString();
                        newDataAdded = true; // Set newDataAdded menjadi true
                        break; // Keluar dari loop karena data baru telah ditambahkan
                    }
                }

                // Jika data baru belum ditambahkan, tambahkan data baru ke receivedDataList
                if (!newDataAdded)
                {
                    receivedDataList.Add(logBuilder.ToString());
                }
            }
        }

        private void UpdateTextBoxes(string[] data_GUI)
        {
            if (data_GUI.Length > 1) textBoxD1.Text = data_GUI[3];
            if (data_GUI.Length > 2) textBoxD2.Text = data_GUI[4];
            if (data_GUI.Length > 3) textBoxD3.Text = data_GUI[5];
            if (data_GUI.Length > 4) textBoxD4.Text = data_GUI[6];
            if (data_GUI.Length > 4) textBoxD5.Text = data_GUI[7];


        }

        private void ResetChart(Chart chart)
        {
            if (chart.Series.Count > 0)
            {
                // Clear data points without removing the series
                foreach (var series in chart.Series)
                {
                    series.Points.Clear();
                }
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void btnSimpan_Click_1(object sender, EventArgs e)
        {
            // Meminta pengguna memberikan nama file untuk menyimpan data
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text File|*.txt";
            saveFileDialog1.Title = "Simpan Data ke File";
            saveFileDialog1.ShowDialog();

            // Jika pengguna memilih nama file dan menekan OK
            if (saveFileDialog1.FileName != "")
            {
                // Gabungkan semua log data dari receivedDataList
                string allLogs = string.Join(Environment.NewLine, receivedDataList);

                // Tulis semua log data ke file
                File.WriteAllText(saveFileDialog1.FileName, allLogs);

                // Inform the user
                MessageBox.Show("Anjay, wes ke-Save cuy");
            }
        }



        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void tBoxPotensio_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }


        private void Graph_D1_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void Graph_D2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Graph_D4_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click_1(object sender, EventArgs e)
        {

        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void Graph_State_GUI_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart3_SelectionRangeChanged(object sender, CursorEventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void Graph_D3_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            // Update chart axis for each chart
            UpdateChartAxis(chart1);
            UpdateChartAxis(chart2);
            UpdateChartAxis(chart3);
            UpdateChartAxis(chart4);
        }

        private void UpdateChartAxis(Chart chart)
        {
            if (chart.Series.Count > 0 && chart.Series[0].Points.Count > 30)
            {
                var series = chart.Series[0];
                double minX = time - (0.1 * 30);
                double maxX = time;

                // Remove old data points
                while (series.Points.Count > 0 && series.Points[0].XValue < minX)
                {
                    series.Points.RemoveAt(0);
                }

                // Update X-axis range
                chart.ChartAreas[0].AxisX.Minimum = minX;
                chart.ChartAreas[0].AxisX.Maximum = maxX;
            }
        }


        private void tBoxD1_TextChanged(object sender, EventArgs e)
        {

        }

        private void cBoxBaud_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cBoxData_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

    }
}
