using System;
using System.Collections.Generic;

namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {
            bool state_port;

            modbusRtu communicationArduino = new modbusRtu();
            state_port = communicationArduino.OpenPort("COM10", 9600);

            if (state_port)
            {
                // READ
                Dictionary<int, int> coilStatus = new Dictionary<int, int>();
                Dictionary<int, int> inputStatus = new Dictionary<int, int>();
                Dictionary<int, int> holdingRegisters = new Dictionary<int, int>();
                Dictionary<int, int> inputRegisters = new Dictionary<int, int>();

                //FC 01 read coil status
                //coilStatus = communicationArduino.readCoilStatus(10, 1, 37);
                //fc 02 read input status
                //inputStatus = communicationArduino.readDiscreteInputs(10, 2, 10);
                //fc 03 read holding registers
                //holdingRegisters = communicationArduino.readHoldingRegisters(10, 55, 4);
                //fc 04 read input registers
                //inputRegisters = communicationArduino.readInputRegisters(10, 50, 10);

                // - WRITE
                int[] valueMultipleRegisters = { 33, 44, 55, 66 };

                //fc 05 - write single coil
                //communicationArduino.writeSingleCoil(10, 1, true);
                //fc 15 - write multiple coil
                communicationArduino.writeMultipleCoils(10, 1, 53509); //65535  //tecnicamente si potrebbe togliere il fatto che si voglisa scrivere un toto di registri
                //fc 06 - write single register
                //communicationArduino.writeSingleRegister(10, 55, 199);
                //fc 16 - write multiple registers
                //communicationArduino.writeMultipleRegisters(10, 55, valueMultipleRegisters);
            }
        }
    }
}