using System;

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
                // - READ

                //fc 01 read coil status
                //communicationArduino.readModbus(1, 10, 1, 1);
                //fc 02 read input status
                //communicationArduino.readModbus(2, 10, 5 ,1);
                //fc 03 read holding registers
                //communicationArduino.readModbus(3, 10, 50, 4);
                //fc 04 read input registers
                //communicationArduino.readModbus(4, 10, 50, 5);

                // - WRITE

                // COIL
                //fc 05 - write single coil
                //communicationArduino.writeCoil(5, 10, 1, 0xff00);
                //fc 15 - write multiple coil
                //communicationArduino.writeCoil(15, 10, 1, 9, 0);

                //REGISTERS
                byte[] value = { 77, 45, 87, 0x55 };
                //fc 06 write single register
                //communicationArduino.writeRegister(6, 10, 50, 345);
                //fc 16 write multiple registers
                //communicationArduino.writeRegister(16, 10, 50, value);
            }
        }
    }
}
