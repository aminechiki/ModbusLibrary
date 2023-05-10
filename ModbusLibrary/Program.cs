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


            //Dictionary<int, int> coilStatus = new Dictionary<int, int>();
            //Dictionary<int, int> inputStatus = new Dictionary<int, int>();
            //Dictionary<int, int> holdingRegisters = new Dictionary<int, int>();
            //Dictionary<int, int> inputRegisters = new Dictionary<int, int>();

            //modbusRtu deviceRtu = new modbusRtu("COM11", 9600);

            // - READ
            //FC 01 read coil status
            //coilStatus = deviceRtu.readCoilStatus(10, 1, 10);
            //fc 02 read input status
            //inputStatus = deviceRtu.readDiscreteInputs(10, 2, 10);
            //fc 03 read holding registers
            //holdingRegisters = deviceRtu.readHoldingRegisters(1, 8959, 2);
            //fc 04 read input registers
            //inputRegisters = deviceRtu.readInputRegisters(10, 50, 10);

            // - WRITE
            //int[] valueMultipleRegisters = { 0, 1 };
            //fc 05 - write single coil
            //deviceRtu.writeSingleCoil(10, 1, false);
            //fc 15 - write multiple coil
            //deviceRtu.writeMultipleCoils(10, 1, 3); //65535  //tecnicamente si potrebbe togliere il fatto che si voglisa scrivere un toto di registri
            //fc 06 - write single register
            //deviceRtu.writeSingleRegister(1, 8640, 0);
            //fc 16 - write multiple registers
            //deviceRtu.writeMultipleRegisters(1, 8640, valueMultipleRegisters);

            //MODBUS TCP/IP

            //CONTROLLI

            //1 - HOLDING REGISTERS
            //2 - COIL
            //3 - DISCRETE INPUT
            //4 - INPUT REGISTERS

            int[] valueMultipleRegisters = { 1, 2, 3, 4};
            modbus deviceTcp = new modbusTcp("127.0.0.1");
            // - READ
            //FC 01 read coil status
            //deviceTcp.readCoilStatus(1, 120, 10);
            //fc 02 read input status
            deviceTcp.readDiscreteInputs(1, 120, 10);
            //fc 03 read holding registers
            //deviceTcp.readHoldingRegisters(1, 120, 10);
            //fc 04 read input registers
            //deviceTcp.readInputRegisters(1, 120, 4);

            //WRITE
            //fc 05 - write single coil
            //deviceTcp.writeSingleCoil(1, 120, true);
            //fc 15 - write multiple coil
            //deviceTcp.writeMultipleCoils(1, 120, 15);
            //fc 06 - write single register
            //deviceTcp.writeSingleRegister(1, 120, 55);
            //fc 16 - write multiple registers
            //deviceTcp.writeMultipleRegisters(1, 120, valueMultipleRegisters);

        }
    }
}