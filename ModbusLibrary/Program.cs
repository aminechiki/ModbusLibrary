using System;

namespace ModbusLibrary
{
    class Program
    {
        public static void Main()
        {
            bool state_port;

            modbus communicationArduino = new modbus();
            state_port = communicationArduino.OpenPort("COM10", 9600, 8, System.IO.Ports.Parity.None, System.IO.Ports.StopBits.One);


            if (state_port)
            {
                //settings for read a register in modbus
                //-type of function
                //-address slave
                //-address start read
                //-how many address you want to read
                //communicationArduino.ReadModbus(4, 10, 55 ,6);


                // WRITE //

                //FC 05 WRITE SINGLE COIL

                ushort coilAdress = 1;
                bool stateOutCoil = false;
                //communicationArduino.WriteSingleCoil(10, coilAdress, stateOutCoil);


                //WRITE REGISTERS

                communicationArduino.WriteMultipleCoils(10, 15, 1, 1);
                //communicationArduino.WriteSingleRegisters(10, 6, 55, 33);

            }
        }
    }
}
