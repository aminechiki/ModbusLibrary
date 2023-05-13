using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System;

namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {
            //CONTROLLARE COME TI TORNA I REGISTRI SU MIDBNUS RTU

            //punti da modificare 

            // - la pdu deve avere una dimensione di 6 byte 

            int[] valueMultipleRegisters = { 1, 2, 3, 4};

            ModbusTcpAsync modbus = new ModbusTcpAsync("127.0.0.1");
            //modbus.readHoldingRegisters(0, 0, 2);


            //modbus.writeSingleRegister(0, 5, 0);


            modbus.writeSingleRegister(0, 0, 44);


            //modbus.writeSingleRegister(0, 5, 24);
        }
    }
}