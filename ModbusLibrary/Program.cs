using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System;
using System.Diagnostics.Tracing;

namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {
            //CONTROLLARE COME TI TORNA I REGISTRI SU MIDBNUS RTU

            //punti da modificare 

            // - la pdu deve avere una dimensione di 6 byte 

            int[] valueMultipleRegisters = { 9, 18, 36, 42};

            ModbusTcp modbusClient = new ModbusTcp("127.0.0.1", 502, true);
            
            modbusClient.OnResponseData += new ModbusTcp.ResponseData(MBmaster_OnResponseData);

            //WRITE SINGLE COIL
            //modbusClient.writeSingleCoil(0,3,true);
            //WRITE MULTIPLE REGISTER

            //WRITE SINGLE REGISTER
            //modbusClient.writeSingleRegister(0,0,33);
            //WRITE MULTIPLE REGISTERS
            //modbusClient.writeMultipleRegisters(0,0,valueMultipleRegisters);

            modbusClient.writeMultipleCoils(0,0,2);

            while (true);
        }


        

        private static void MBmaster_OnResponseData(byte[] values)
        {

            //Dispatcher.Invoke(() => ShowAs(values));

            foreach (byte b in values) Console.WriteLine(b);

            /*
            new Thread(() =>
            {
                string x = "Yes.";
                // Invoke the dispatcher.
                Dispatcher.Invoke((Action)delegate ()
                {
                    // Get the string off a UI element which contains the text, "No."
                    string g = "";
                    g = MBmaster.dataRecived[1].ToString();
                    testo.Text = g;
                });
                // Is x either ("Yes" or "No") here, or always "No"?
            }).Start();
            */
        }
    }
}