﻿/*
 *   This file is part of Arduino.NET
 *
 *   Arduino.NET is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   Arduino.NET is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Arduino.NET.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;

namespace System.IO
{
    [ComVisible(true)]
    public class Arduino
    {
        public delegate void CommandQueueReadyEventHandler(object sender);
        public event CommandQueueReadyEventHandler CommandQueueReady;

        private SerialPort _serialPort;
        private Stack<ArduinoCommand> _commandStack;

        private int delay = 0;

        public Arduino(string comport, int baudrate, bool autostart, int delay)
        {
            this._serialPort = new SerialPort();
            this._serialPort.PortName = comport;
            this._serialPort.BaudRate = baudrate;
            this._serialPort.DataBits = 8;
            this._serialPort.Parity = Parity.None;
            this._serialPort.StopBits = StopBits.One;

            this._commandStack = new Stack<ArduinoCommand>();

            if (autostart)
            {
                this.delay = delay;
                this.Open();
            }
        }

        public Arduino(string comport, int baudrate, bool autostart) : this(comport, baudrate, autostart, 3000) { }

        public Arduino(string comport, int baudrate) : this(comport, baudrate, false) { }

        public Arduino(string comport) : this(comport, 9600) { }

        public void Open()
        {
            this._serialPort.Open();
            this._serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            Thread.Sleep(this.delay);
        }

        public void Close()
        {
            this._serialPort.Close();
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ArduinoCommand command = new ArduinoCommand(this._serialPort.ReadLine().Trim("\r".ToCharArray()));
            this._commandStack.Push(command);
            if (this.CommandQueueReady != null)
                this.CommandQueueReady(this);
        }

        public void SendCommand(ArduinoCommand command)
        {
            if (!_serialPort.IsOpen)
                throw new Exception("Port is not open");
            _serialPort.Write(command.ToString());
        }

        public Stack<ArduinoCommand> CommandQueue
        {
            get { return this._commandStack; }
        }
    }

    [ComVisible(true)]
    public class ArduinoCommand
    {
        private const byte ARG_SEPARATOR = 0x20;
        private const byte END_MESSAGE = 0x23;
        private const byte ESCAPE_CHARACTER = 0x5C;

        private byte _Command;
        private ArrayList _CommandArgs = new ArrayList();
        private ASCIIEncoding encoder = new ASCIIEncoding();

        public ArduinoCommand(string CommandString)
        {
            string separator = encoder.GetString(new byte[] { ARG_SEPARATOR });
            string[] split = CommandString.Split(separator.ToCharArray());

            this._Command = encoder.GetBytes(split[0])[0];

            for (int i = 1; i < split.Length - 1; i++)
            {
                this._CommandArgs.Add(split[i]);
            }
        }

        public ArduinoCommand(byte command, ArrayList argv)
        {
            this._Command = command;
            this._CommandArgs = argv;
        }

        public ArduinoCommand(byte command) : this(command, new ArrayList()) { }

        public ArduinoCommand(byte command, params object[] list)
        {
            ArrayList l = new ArrayList();
            foreach (object o in list)
            {
                l.Add(o);
            }
            this._Command = command;
            this._CommandArgs = l;
        }

        public ArduinoCommand() { }

        public byte Command
        {
            get { return this._Command; }
            set { this._Command = value; }
        }

        public ArrayList CommandArgs
        {
            get { return this._CommandArgs; }
            set { this._CommandArgs = value; }
        }

        public override string ToString()
        {
            string ret = "";
            ret += encoder.GetString(new byte[] { this._Command });

            foreach (object o in this._CommandArgs)
            {
                ret += encoder.GetString(new byte[] { ARG_SEPARATOR });
                if (o.GetType() == typeof(bool))
                {
                    ret += ((bool)o) ? "1" : "0";
                }
                else
                {
                    ret += o.ToString();
                }
            }

            ret+= encoder.GetString(new byte[] { ARG_SEPARATOR }) +
                encoder.GetString(new byte[] { END_MESSAGE });

            return ret;
        }
    }
}
