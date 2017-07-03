using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPFSensorDisplay
{
    public partial class Form1 : Form
    {

        private const string TITLE = "SPF Sensor Display ";

        private const string CSV_TEST = "000,1,3, 4,45,89,255,6";

        private readonly Color COLOUR_GRAPH_PANEL_COLOUR = Color.Wheat;
        private readonly Color COLOUR_EXTENDED_GRAPH_PANEL_COLOUR = Color.MediumAquamarine;


        private const int GRAPH_PANEL_TOP = 30;
        private const int GRAPH_PANEL_LEFT = 30;
        private const int GRAPH_PANEL_HEIGHT = 150;
        private const int GRAPH_PANEL_WIDTH = 200;
        private const int GRAPH_PANEL_SPACING = 5;
        private const int GRAPH_PANEL_AXIS_SPACING = 17;
        private const int GRAPH_PANEL_MAX_COLUMNS = 5;

        private const int TOTAL_NUMBER_OF_GRAPH_PANELS = 20;
        private const int SHORT_NUMBER_OF_GRAPH_PANELS = 12;

        private const int SENSOR_DATA_LENGTH = 50;

        List<List<byte>> m_SensorData;

        List<Panel> m_PanelList;

        List<long> m_SensorTotals;
        int m_NumberOfSensorReads;

        string m_Folder;

        public Form1()
        {
            SetText("");
            m_Folder = @"c:\";
            InitializeComponent();
            m_PanelList = new List<Panel>();
            m_SensorData = new List<List<byte>>();
            CreateGraphPanels(TOTAL_NUMBER_OF_GRAPH_PANELS, SHORT_NUMBER_OF_GRAPH_PANELS);

            DrawGraphPanelAxes();

            m_SensorTotals = new List<long>();
            m_NumberOfSensorReads = 0;
        }


        private void SetText(string fileName)
        {
            this.Text = TITLE + fileName;


        }

        /// <summary>
        /// Clears the spf data graphs.
        /// </summary>
        private void ClearGraphPannels()
        {
            if (m_PanelList.Count > 0)
            {
                foreach (Panel GraphPanel in m_PanelList)
                {
                    GraphPanel.Dispose();
                }
            }
            m_PanelList.Clear();
        }

        /// <summary>
        /// Creates graph panels to display spf sensor data.
        /// </summary>
        /// <param name="totalNumberOfPanels">Usually 20</param>
        /// <param name="shortNumberOfPanels">Usually 12</param>
        private void CreateGraphPanels(int totalNumberOfPanels, int shortNumberOfPanels)
        {
            //ClearGraphPannels();


            int RowNumber = 0;
            int ColumnNumber = 0;
            for (int i = 0; i < totalNumberOfPanels; i++)
            {
                // Add and position new panel
                m_PanelList.Add(new Panel());
                m_PanelList[i].Top = GRAPH_PANEL_TOP + (RowNumber * (GRAPH_PANEL_HEIGHT + GRAPH_PANEL_SPACING));
                m_PanelList[i].Width = GRAPH_PANEL_WIDTH;
                m_PanelList[i].Height = GRAPH_PANEL_HEIGHT;
                m_PanelList[i].Left = GRAPH_PANEL_LEFT + (ColumnNumber * (GRAPH_PANEL_WIDTH + GRAPH_PANEL_SPACING));

                // Activate panel
                m_PanelList[i].Visible = true;
                m_PanelList[i].Enabled = true;

                //Set background colour.
                if (i >= shortNumberOfPanels)
                {
                    m_PanelList[i].BackColor = COLOUR_EXTENDED_GRAPH_PANEL_COLOUR;
                }
                else
                {
                    m_PanelList[i].BackColor = COLOUR_GRAPH_PANEL_COLOUR;
                }

                // Add panel to controls
                Controls.Add(m_PanelList[i]);
                ColumnNumber++;
                if (ColumnNumber == GRAPH_PANEL_MAX_COLUMNS)
                {
                    RowNumber++;
                    ColumnNumber = 0;
                }
            }

        }

        /// <summary>
        /// Draw x and y axes on panels.
        /// </summary>
        private void DrawGraphPanelAxes()
        {
            // Set up line parameters.
            Pen pen001 = new Pen(Color.Black, 2.0F);
            Font GraphFont = new Font("Arial", 6F, FontStyle.Bold);
            Brush GraphBrush = Brushes.Black;
            foreach (Panel GraphPanel in m_PanelList)
            {
                using (Graphics g = GraphPanel.CreateGraphics())
                {
                    g.DrawLine(pen001, GRAPH_PANEL_SPACING, (GRAPH_PANEL_HEIGHT - GRAPH_PANEL_AXIS_SPACING), (GRAPH_PANEL_WIDTH - GRAPH_PANEL_SPACING), (GRAPH_PANEL_HEIGHT - GRAPH_PANEL_AXIS_SPACING));    // x axis
                    g.DrawLine(pen001, GRAPH_PANEL_SPACING, (GRAPH_PANEL_HEIGHT - GRAPH_PANEL_AXIS_SPACING), GRAPH_PANEL_SPACING, GRAPH_PANEL_SPACING);    // y axis


                }

            }
        }

        /// <summary>
        /// Display the sensor name on each graph panel.
        /// </summary>
        /// <param name="sensorDetailsList">List of CSensorDetails objects</param>
        public void DisplaySensorNames(List<String> sensorDetailsList)
        {
            Font GraphFont = new Font("Arial", 7F, FontStyle.Bold);
            Brush GraphBrush = Brushes.Black;

            int SensorNumber = 0;

            // Loop through the graph panels
            while (SensorNumber < TOTAL_NUMBER_OF_GRAPH_PANELS)
            {
                // If there is a sensor name for this panel, show it.
                if (SensorNumber < sensorDetailsList.Count)
                {
                    using (Graphics g = m_PanelList[SensorNumber].CreateGraphics())
                    {
                        g.DrawString(sensorDetailsList[SensorNumber], GraphFont, GraphBrush, GRAPH_PANEL_SPACING, GRAPH_PANEL_HEIGHT - 14);
                    }
                }
                // Otherwise, hide the panel.
                else
                {
                    m_PanelList[SensorNumber].Visible = false;
                }
                SensorNumber++;
            }
        }

        /// <summary>
        /// Display spf sensor data.
        /// </summary>
        /// <param name="sensorDataList">List of sensor data byte lists.</param>
        public void DisplaySensorData(List<List<byte>> sensorDataList, bool redPen = false)
        {
            Pen PenBlack = Pens.Black;
            Pen PenRed = Pens.Red;

            Pen Pen1;

            if (redPen)
            {
                Pen1 = PenRed;
            }
            else
            {
                Pen1 = PenBlack;
            }

            int SensorNumber = 0;
            // loop through sensors
            while (SensorNumber < sensorDataList.Count)
            {

                List<byte> SensorData = sensorDataList[SensorNumber];
                // Create a point array from the sensor data
                Point[] PointArray = new Point[SensorData.Count];

                for (int i = 0; i < SensorData.Count; i++)
                {
                    PointArray[i].X = GRAPH_PANEL_SPACING + (i * 3);
                    PointArray[i].Y = 128 - (SensorData[i] / 2);

                }

                // If there is a panel to disply it on, display it.
                if (SensorNumber < TOTAL_NUMBER_OF_GRAPH_PANELS)
                {
                    using (Graphics g = m_PanelList[SensorNumber].CreateGraphics())
                    {
                        g.DrawLines(Pen1, PointArray);
                    }
                }
                SensorNumber++;
            }
        }






        private void AddSensorDataToTotal(string csvLine)
        {
            List<byte> SensorData = CSVLine2ByteList(csvLine);

            if (m_SensorTotals.Count == 0)
            {
                for (int i = 0; i < 900; i++)
                {

                    m_SensorTotals.Add(SensorData[i]);


                }

            }
            else
            {
                for (int i = 0; i < 900; i++)
                {

                    m_SensorTotals[i] += SensorData[i];


                }
            }

            m_NumberOfSensorReads++;
        }

        private void CalculateAverageSensorData()
        {
            for (int i = 0; i < m_SensorTotals.Count; i++)
            {
                m_SensorTotals[i] = m_SensorTotals[i] / m_NumberOfSensorReads;


            }

        }


        private List<byte> CSVLine2ByteList(string csvLine)
        {
            List<byte> SensorDataBytes = new List<byte>();
            string[] CsvStrings = csvLine.Split(',');
            byte[] CsvBytes = new byte[CsvStrings.Length];
            for (int i = 0; i < CsvStrings.Length; i++)
            {
                SensorDataBytes.Add(byte.Parse(CsvStrings[i]));
            }
            return SensorDataBytes;
        }

        private List<List<byte>> LongList2ByteLists(List<long> longList, int sensorLength)
        {
            List<List<byte>> SensorAverageList = new List<List<byte>>();
            int Index = 0;

            while (Index < longList.Count)
            {
                List<byte> SensorAverage = new List<byte>();
                for (int i = 0; i < sensorLength; i++)
                {
                    SensorAverage.Add((byte)longList[Index + i]);

                }


                SensorAverageList.Add(SensorAverage);
                Index += sensorLength;
            }



            return SensorAverageList;
        }

        private List<List<byte>> CSVLine2ByteLists(string csvLine, int sensorLength)
        {
            List<List<byte>> SensorDataList = new List<List<byte>>();
            int CsvIndex = 0;

            string[] CsvStrings = csvLine.Split(',');
            byte[] CsvBytes = new byte[CsvStrings.Length];
            for (int i = 0; i < CsvStrings.Length; i++)
            {
                CsvBytes[i] = byte.Parse(CsvStrings[i]);
            }

            while (CsvIndex < CsvBytes.Length)
            {
                List<byte> SensorData = new List<byte>();
                byte[] SensorDataBytes = new byte[sensorLength];
                Array.Copy(CsvBytes, CsvIndex, SensorDataBytes, 0, sensorLength);
                SensorData.AddRange(SensorDataBytes);
                SensorDataList.Add(SensorData);
                CsvIndex += sensorLength;
            }

            return SensorDataList;
        }


        private List<string> ReadCsvLines(string filePath)
        {
            List<string> CsvLines = new List<string>();
            StreamReader SReader = new StreamReader(filePath);
            string FileLine = "";
            while ((FileLine = SReader.ReadLine()) != null)
            {
                CsvLines.Add(FileLine);

            }

            return CsvLines;
        }

        private void ProcessCSVFileContents(string CsvFilePath)
        {
            ClearGraphPannels();
            CreateGraphPanels(TOTAL_NUMBER_OF_GRAPH_PANELS, SHORT_NUMBER_OF_GRAPH_PANELS);
            DrawGraphPanelAxes();
            List<string> CsvLines = ReadCsvLines(CsvFilePath);
            m_SensorTotals.Clear();
            m_NumberOfSensorReads = 0;
            foreach (string CsvLine in CsvLines)
            {
                DisplaySensorData(CSVLine2ByteLists(CsvLine, SENSOR_DATA_LENGTH));
                AddSensorDataToTotal(CsvLine);
            }
            CalculateAverageSensorData();
            DisplaySensorData(LongList2ByteLists(m_SensorTotals, SENSOR_DATA_LENGTH), true);

        }

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog CSVFileDialog = new OpenFileDialog();
            CSVFileDialog.InitialDirectory = m_Folder;
            CSVFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            CSVFileDialog.DefaultExt = ".csv";
            if (CSVFileDialog.ShowDialog() == DialogResult.OK)
            {

                SetText(Path.GetFileName(CSVFileDialog.FileName));
                ProcessCSVFileContents(CSVFileDialog.FileName);
                m_Folder = Path.GetDirectoryName(CSVFileDialog.FileName);

            }


        }


    }
}
