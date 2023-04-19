﻿using System.Collections.Generic;

namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {    
            Dictionary<int, int> coilStatus = new Dictionary<int, int>();
            Dictionary<int, int> inputStatus = new Dictionary<int, int>();
            Dictionary<int, int> holdingRegisters = new Dictionary<int, int>();
            Dictionary<int, int> inputRegisters = new Dictionary<int, int>();

            modbusRtu deviceRtu = new modbusRtu("COM10", 9600);
            // - READ
            //FC 01 read coil status
            //coilStatus = deviceRtu.readCoilStatus(10, 1, 37);
            //fc 02 read input status
            //inputStatus = deviceRtu.readDiscreteInputs(10, 2, 10);
            //fc 03 read holding registers
            //holdingRegisters = deviceRtu.readHoldingRegisters(10, 55, 4);
            //fc 04 read input registers
            //inputRegisters = deviceRtu.readInputRegisters(10, 50, 10);

            // - WRITE
            int[] valueMultipleRegisters = { 33, 44, 55, 66 };
            //fc 05 - write single coil
            //deviceRtu.writeSingleCoil(10, 1, true);
            //fc 15 - write multiple coil
            //deviceRtu.writeMultipleCoils(10, 1, 53509); //65535  //tecnicamente si potrebbe togliere il fatto che si voglisa scrivere un toto di registri
            //fc 06 - write single register
            //deviceRtu.writeSingleRegister(10, 55, 199);
            //fc 16 - write multiple registers
            //deviceRtu.writeMultipleRegisters(10, 55, valueMultipleRegisters);

            //MODBUS TCP/IP

            modbusTcp deviceTcp = new modbusTcp("127.0.0.1");
            // - READ
            //FC 01 read coil status
            //deviceTcp.readCoilStatus(1, 00120, 9);
            //fc 02 read input status
            //deviceTcp.readDiscreteInputs(1, 00120, 20);
            //fc 03 read holding registers
            //deviceTcp.readHoldingRegisters(1, 00120, 7);
            //fc 04 read input registers
            //deviceTcp.readInputRegisters(1, 00120, 7);

            //WRITE
            //fc 05 - write single coil
            //deviceTcp.writeSingleCoil(1, 00120, true);
            //fc 06 - write single register
            deviceTcp.writeSingleRegister(1, 00120, 244);


        }
    }
}