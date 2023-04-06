using System;

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
                while (true)
                {
                    communicationArduino.ReadHoldingRegisters(address, start, registers);
                }

            }

        }
    }
}
