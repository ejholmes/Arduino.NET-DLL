using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Arduino.NET;

namespace Example_App
{
    public struct Commands
    {
        public const byte Command1 = 0x01;
        public const byte Command2 = 0x02;
        public const byte Command3 = 0x03;
    }
    public partial class MainWindow : Form
    {
        private Arduino.NET.Arduino Arduino = new Arduino.NET.Arduino("COM3", 9600, false, 2000);

        public MainWindow()
        {
            InitializeComponent();
            Arduino.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ArduinoCommand Command = new ArduinoCommand();

            Command.Command = Commands.Command1;

            Arduino.SendCommand(Command);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ArduinoCommand Command = new ArduinoCommand(Commands.Command2);

            Arduino.SendCommand(Command);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> arguments = new List<string>();
            arguments.Add("100");
            arguments.Add("200");

            ArduinoCommand Command = new ArduinoCommand(Commands.Command3, arguments);

            Arduino.SendCommand(Command);
        }
    }
}
