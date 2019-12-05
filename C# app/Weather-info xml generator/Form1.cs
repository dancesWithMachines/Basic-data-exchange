using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace Weather_info_xml_generator
{
    public partial class Form1 : Form
    {
        bool isConnected = false;
        bool exists = false;
        SerialPort choosenPort;
        String[] portsAvailable;

        public Form1()
        {
            InitializeComponent();
            setStatus("Disabled");
            setControlls(isConnected);
            setPorts();
        }

        private void setPorts()
        {
            portsAvailable = SerialPort.GetPortNames();
            foreach (string port in portsAvailable)
            {
                selectPortBox.Items.Add(port);
            }
        }

        private void connect(object sender, EventArgs e)
        {
            string selectedPort = selectPortBox.GetItemText(selectPortBox.SelectedItem);
            if (!string.IsNullOrEmpty(selectPortBox.Text))
            {
                setStatus("Port selected");
                choosenPort = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);
                if (!isConnected)
                {
                    try
                    {
                        choosenPort.Open();
                        choosenPort.WriteLine("disable");
                        setStatus("Enabling");
                    }
                    catch
                    {
                        setStatus("Port busy");
                    }
                }
                
                if (choosenPort.IsOpen)
                {
                    choosenPort.ReadTimeout = 10000;
                    try
                    {
                        choosenPort.ReadLine();
                        choosenPort.WriteLine("enable");
                        if (choosenPort.ReadLine().StartsWith("!"))
                            setStatus("Device enabled");                        
                        choosenPort.ReadLine();
                        isConnected = true;
                        setControlls(isConnected);
                    }
                    catch (TimeoutException) {
                        setStatus("No connection");
                        choosenPort.Close();
                    }                 
                }
            }            
        }

        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (isConnected)
            {
                choosenPort.WriteLine("disable");
                choosenPort.ReadLine();
                isConnected = false;
                choosenPort.Close();
            }
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void setControlls(bool isOn)
        {
            descriptionTextBox.Enabled = isOn;
            setPressureTextBox.Enabled = isOn;
            setPressureButton.Enabled = isOn;
            generateButtton.Enabled = isOn;
            selectPortBox.Enabled = !isOn;
            connectButton.Enabled = !isOn;
        }

        private void setStatus(string newStatus)
        {
            statusTextBox.Text = newStatus;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void setPressureButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(setPressureTextBox.Text))
            {
                if (float.TryParse(setPressureTextBox.Text, out float output))
                {
                    choosenPort.WriteLine("setpressure");
                    choosenPort.ReadLine();
                    choosenPort.WriteLine(setPressureTextBox.Text);
                    if (choosenPort.ReadLine().StartsWith("Pressure"))
                        setStatus("Set to " + setPressureTextBox.Text + "HPa");
                    else
                        setStatus("Unable to set");
                }
                else
                    setStatus("Cant't set text");
            }
            else
                setStatus("Can't set null");
        }

        private void generateButtton_Click(object sender, EventArgs e)
        {
            //check if file exists
            if (File.Exists("WeatherData.xml"))
                exists = true;
            //get data from arduino
            setStatus("Reading data");
            choosenPort.WriteLine("get");
            IDictionary<string, string> processedData = new Dictionary<string, string>();
            string[] wanted = { "tmp", "pre", "app", "lig" };
            foreach (string variable in wanted )
                processedData[variable] = choosenPort.ReadLine().Split(':', ';')[1];
            //get data from computer
            processedData["dat"] = DateTime.Today.ToString().Split(' ')[0];
            processedData["tim"] = DateTime.Now.ToString().Split(' ')[1];
            processedData["day"] = DateTime.Now.DayOfWeek.ToString();
            processedData["mon"] = DateTime.Today.ToString("MMMM");
            processedData["yea"] = DateTime.Today.Year.ToString();
            processedData["mac"] = Environment.MachineName;
            processedData["des"] = "";
            //get description if present
            if (!string.IsNullOrEmpty(descriptionTextBox.Text))
                processedData["des"] = descriptionTextBox.Text;
            else
                processedData["des"] = "No description added";
            //reading xml file
            int no = 0;
            if (exists)
            {
                setStatus("Reading xml file");
                XDocument PreviousWeatherData = XDocument.Load("WeatherData.xml");
                no = Int32.Parse(PreviousWeatherData.Element("WeatherInfo").Attribute("No").Value) + 1;
                processedData["datl"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("dateInfo").Element("fullDate").Value;
                processedData["timl"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("dateInfo").Element("time").Value;
                processedData["tmpl"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("data").Element("temperature").Value;
                processedData["prel"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("data").Element("pressure").Value;
                processedData["appl"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("data").Element("appHeight").Value;
                processedData["ligl"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("data").Element("light").Value;
                processedData["macl"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("data").Element("machineName").Value;
                processedData["desl"] = PreviousWeatherData.Element("WeatherInfo").Element("now").Element("description").Value;
            }
            else
            {
                setStatus("Setting defaults");
                string[] set = { "datl", "timl", "tmpl", "prel", "appl", "ligl", "macl" };
                foreach (string variable in set)
                    processedData[variable] = "";
                processedData["desl"] = "<NoFilesAvilable>";

            }
            //creating xml
            setStatus("Generating xml file");
            XDocument WeatherData = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("WeatherInfo",
                    new XAttribute("No", no),
                    new XElement("now",
                        new XAttribute("date", processedData["dat"]),
                        new XAttribute("time", processedData["tim"]),
                        new XElement("description", processedData["des"]),
                        new XElement("dateInfo",
                            new XElement("day", processedData["day"]),
                            new XElement("month", processedData["mon"]),
                            new XElement("year", processedData["yea"]),
                            new XElement("fullDate", processedData["dat"]),
                            new XElement("time", processedData["tim"])),
                        new XElement("data",
                            new XElement("temperature", processedData["tmp"]),
                            new XElement("pressure", processedData["pre"]),
                            new XElement("appHeight", processedData["app"]),
                            new XElement("light", processedData["lig"]),
                            new XElement("machineName", processedData["mac"]))),
                    new XElement("then",
                        new XAttribute("date", processedData["datl"]),
                        new XAttribute("time", processedData["timl"]),
                        new XElement("description",processedData["desl"]),
                        new XElement("dateInfo",
                            new XElement("fullDate", processedData["datl"]),
                            new XElement("time", processedData["timl"])),
                        new XElement("data",
                            new XElement("temperature", processedData["tmpl"]),
                            new XElement("pressure", processedData["prel"]),
                            new XElement("appHeight", processedData["appl"]),
                            new XElement("light", processedData["ligl"]),
                            new XElement("machineName", processedData["macl"])))));
            WeatherData.Save("WeatherData.xml");
            setStatus("Generating succesfull");
        }
    }
}
