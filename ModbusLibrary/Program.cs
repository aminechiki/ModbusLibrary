﻿using System;

namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {
            bool state_port;
            byte address = 10;
            ushort start = 50;
            ushort registers = 1;

            modbus communicationArduino = new modbus();
            state_port = communicationArduino.OpenPort("COM10", 9600, 8, System.IO.Ports.Parity.None, System.IO.Ports.StopBits.One);


            if (state_port)
            {
                //FC 01 - READ COIL STATUS
                ushort coilStartRead = 1;
                ushort numberCoilsRead = 1;
                communicationArduino.ReadCoilStatus(address, coilStartRead, numberCoilsRead);
                

                //FC 03 - READ HOLDING REGISTERS

                /*
                while (true)
                {
                    communicationArduino.ReadHoldingRegisters(address, start, registers);
                }
                */

                //FC 05 WRITE SINGLE COIL

                ushort coilAdress = 1;
                bool stateOutCoil = true;
                //communicationArduino.WriteSingleCoil(address, coilAdress, stateOutCoil);

            }

        }
    }
}
