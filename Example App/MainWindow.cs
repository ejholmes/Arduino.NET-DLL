using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

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
        private System.IO.Arduino Arduino = new System.IO.Arduino("COM3", 19200, false, 2000);

        public MainWindow()
        {
            InitializeComponent();

            /* Open the connection */
            Arduino.Open();

            /* This event fires when new data is received */
            Arduino.CommandQueueReady += new Arduino.CommandQueueReadyEventHandler(Arduino_CommandQueueReady);
        }

        void Arduino_CommandQueueReady(object sender)
        {
            /* The commands are stored in a Stack of ArduinoCommand's, we simply pop the most recent */
            ArduinoCommand Command = Arduino.CommandQueue.Pop();

            /* Switch the command */
            switch (Command.Command)
            {
                case Commands.Command1:
                    this.toolStripStatusLabel1.Text = "LED On";
                    break;
                case Commands.Command2:
                    this.toolStripStatusLabel1.Text = "LED Off";
                    break;
                case Commands.Command3:
                    this.toolStripStatusLabel1.Text = "Waited " + Command.CommandArgs[0] + "ms";
                    break;
            }
        }

        /* Turn LED on*/
        private void button1_Click(object sender, EventArgs e)
        {
            ArduinoCommand Command = new ArduinoCommand();

            Command.Command = Commands.Command1;

            Arduino.SendCommand(Command);
        }

        /* Turn LED off */
        private void button2_Click(object sender, EventArgs e)
        {
            ArduinoCommand Command = new ArduinoCommand(Commands.Command2);

            Arduino.SendCommand(Command);
        }

        /* Turn LED on in 2000 ms */
        private void button3_Click(object sender, EventArgs e)
        {
            ArrayList arguments = new ArrayList();
            arguments.Add("2000"); // delay 2000 ms

            ArduinoCommand Command = new ArduinoCommand(Commands.Command3, arguments);

            MessageBox.Show(Command.ToString());
            Arduino.SendCommand(Command);
        }
    }
}
