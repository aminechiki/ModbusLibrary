using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System;
using System.Diagnostics.Tracing;
using ModbusMaster;


namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {
            //CONTROLLARE COME TI TORNA I REGISTRI SU MIDBNUS RTU

            //punti da modificare 

            // - la pdu deve avere una dimensione di 6 byte 

            int[] valueMultipleRegisters = { 3, 4, 69, 8, 88, 12, 14, 77, 18, 55, 222};

            ModbusTcp modbusClient = new ModbusTcp("127.0.0.1", 502, true);
        

            modbusClient.OnResponseData += new ModbusTcp.ResponseData(MBmaster_OnResponseData);


            //READ SINGLE REGISER
            modbusClient.ReadHoldingRegisters(0, 0, 11);

            //Console.WriteLine(modbusClient._ResponseFromSlave);

            //WRITE SINGLE COIL
            //modbusClient.writeSingleCoil(0,3,true);
            //WRITE MULTIPLE REGISTER

            //WRITE SINGLE REGISTER
            //modbusClient.WriteSingleRegister(0,0,40);
            //WRITE MULTIPLE REGISTERS
            //modbusClient.WriteMultipleRegisters(0,0,valueMultipleRegisters);

            //modbusClient.writeMultipleCoils(0,0,2);

            while (true);
        }


        

        private static void MBmaster_OnResponseData(byte[] values)
        {

            Console.WriteLine("ciao");
            //Dispatcher.Invoke(() => ShowAs(values));

            //foreach (byte b in values) Console.WriteLine(b);

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