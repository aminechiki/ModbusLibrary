using System;

namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {
            bool state_port;

            modbus communicationArduino = new modbus();
            state_port = communicationArduino.OpenPort("COM10", 9600);

            if (state_port)
            {
                communicationArduino.ReadModbus(2, 10, 5 ,1);

                // WRITE //

                //FC 05 WRITE SINGLE COIL

                ushort coilAdress = 1;
                bool stateOutCoil = true;
                //communicationArduino.WriteSingleCoil(10, coilAdress, stateOutCoil);

                short[] value = {77, 45, 87, 0x55 };


                //communicationArduino.WriteMultipleRegisters(16, 10, 50, value);


                //WRITE REGISTERS

                //communicationArduino.WriteMultipleCoils(10, 15, 1, 1);
                //communicationArduino.WriteSingleRegister(6, 10, 50, 33);

                //communicationArduino.WriteMultipleCoil(15, 10, 1, 6, 255);

            }
        }
    }
}
