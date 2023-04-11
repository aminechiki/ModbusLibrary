using System;
using System.IO.Ports;

namespace ModbusLibrary
{
    class modbus
    {
        private SerialPort serialPort = new SerialPort();
        public bool OpenPort(string portName, int baudRate)
        {
            bool statePort = false;

            //Ensure port isn't already opened:
            if (!serialPort.IsOpen)
            {
                //Assign settings to the serial port:
                serialPort.PortName = portName;
                serialPort.BaudRate = baudRate;
                serialPort.DataBits = 8;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                //Imposte Timeout:
                serialPort.ReadTimeout = 1000;
                serialPort.WriteTimeout = 1000;
                //Try to open port
                try
                {
                    serialPort.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
            return statePort;
        }

        // - FUNCTION FOR READ
        public void ReadModbus(byte typeOfFunction, byte addressSlave, ushort addressStartRead, ushort numberRegistersRead)
        {
            byte[] messageSendSlave = new byte[8];
            byte[] responseFromSlave = new byte[0];
            //1 - based onthe type ti function will be set the correct size of the response array
            switch (typeOfFunction)
            {
                case 1:
                    responseFromSlave = new byte[5 + numberRegistersRead];
                    break;
                case 2:
                    //read inputs are bits that are written in bytes, so for every 8 bits you want
                    //to read the slave will respond to you with a response byte, if the bits you
                    //want to read are less than 8 then you will put 1 by default
                    int inputStatus = numberRegistersRead / 8;
                    if (inputStatus == 0) inputStatus = 1;
                    responseFromSlave = new byte[5 + inputStatus];
                    break;
                case 3:
                    responseFromSlave = new byte[5 + 2 * numberRegistersRead];
                    break;
                case 4:
                    responseFromSlave = new byte[5 + 2 * numberRegistersRead];
                    break;
            }         
            //2 - Clear buffer in In and Out of serial Port
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();
            //3 - Create a message send to slave
            seampleBuildMessage(addressSlave, messageSendSlave, typeOfFunction, addressStartRead, numberRegistersRead);
            Console.WriteLine("FC 01 - MESSAGGIO INVIATO");
            foreach (byte m in messageSendSlave)
            {
                Console.WriteLine(m);
            }
            //4 - Send a message to Slave 
            try
            {
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                //5 - Get response from slave
                GetResponse(responseFromSlave);
                Console.WriteLine("FC01 - MESSAGGIO RICEVUTO");
                for (int i = 0; i < responseFromSlave.Length; i++)
                {
                    Console.WriteLine(responseFromSlave[i]);
                    //if (i == 4) Console.WriteLine(responseFromSlave[i] | responseFromSlave[i - 1] << 8);
                    //if (i == 6) Console.WriteLine(responseFromSlave[i] | responseFromSlave[i - 1] << 8);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        //method use in function for Build a messagge to send to slave fro read
        public void seampleBuildMessage(byte addressSlave, byte[] message, byte typeOfFunction, ushort addressMemoryStartRead, ushort numberRegistersRead)
        {
            //Array to receive CRC bytes: 
            byte[] CRC = new byte[2];
            //- Builds the message com eindicated in the modbus protocol for function
            //address Slave
            message[0] = addressSlave;
            //type of function
            message[1] = typeOfFunction;
            //is divided into two bytes the value , the first one shifted by 8
            message[2] = (byte)(addressMemoryStartRead >> 8);
            message[3] = (byte)addressMemoryStartRead;
            //is divided into two bytes the value , the first one shifted by 8
            message[4] = (byte)(numberRegistersRead >> 8);
            message[5] = (byte)numberRegistersRead;
            //CRC - get the CRC with the methd and pot resul in two last position
            GetCRC(message, CRC);
            message[message.Length - 2] = CRC[0];
            message[message.Length - 1] = CRC[1];
        }

        // - FUNCTION FOR WRITE ///////////////////////////////////////////////////////////////////////////////////////////

        //FC 05 WRITE SINGLE COIL
        public void WriteSingleCoil(byte addressSlave, ushort coilAddress, bool stateOut)
        {
            byte typeOfFunction = 5;
            byte[] messageSendSlave = new byte[8];
            byte[] responseFromSlave = new byte[8];
            //1 - Build a messagge send to slave
            BuildMessageWriteSingleCoil(addressSlave, messageSendSlave, typeOfFunction, coilAddress, stateOut);
            Console.WriteLine("FC05 - MESSAGGIO INVIATO");
            foreach(byte m in messageSendSlave) Console.WriteLine(m);
            //2 - Try to send a messagge to slave
            try
            {
                
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                //3 - get respone from slave
                GetResponse(responseFromSlave);
                Console.WriteLine("FC05 - MESSAGGIO RICEVUTO");
                foreach (byte m in responseFromSlave) Console.WriteLine(m);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }
        public void BuildMessageWriteSingleCoil(byte addressSlave, byte[] messageSendSlave, byte typeOfFunction, ushort coilAddress, bool stateOut)
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];
            //- Builds the message com eindicated in the modbus protocol for function FC05
            //address Slave
            messageSendSlave[0] = addressSlave;
            //type of function
            messageSendSlave[1] = typeOfFunction;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[2] = (byte)(coilAddress >> 8);
            messageSendSlave[3] = (byte)coilAddress;
            //If stateOut is true send messagge 0xFF then 0x00; this result is divide in two byte
            if (stateOut)
            {
                messageSendSlave[4] = 0xFF;
                messageSendSlave[5] = 0x00;
            }
            else
            {
                messageSendSlave[4] = 0x00;
                messageSendSlave[5] = 0x00;
            }

            //CRC - get the CRC with the methd and pot resul in two last position
            GetCRC(messageSendSlave, CRC);
            messageSendSlave[messageSendSlave.Length - 2] = CRC[0];
            messageSendSlave[messageSendSlave.Length - 1] = CRC[1];
        }

        //FC 16 - WRITE MULTIPLE REGISTERS
        public void WriteMultipleRegisters(byte typeOfFunction, byte addressSlave, ushort addressStartWrite, short[] valuesWriteAddress)
        {          
            //Take the numbers of register do you want write
            int numberRegistersWrite = valuesWriteAddress.Length;
            byte[] messageSendSlave = new byte[9 + 2 * numberRegistersWrite];
            byte[] responseFromSlave = new byte[8];

            //Add bytecount to message:
            messageSendSlave[6] = (byte)(numberRegistersWrite * 2);
            //Put write values into message prior to sending:
            for (int i = 0; i < numberRegistersWrite; i++)
            {
                messageSendSlave[7 + 2 * i] = (byte)(valuesWriteAddress[i] >> 8);
                messageSendSlave[8 + 2 * i] = (byte)(valuesWriteAddress[i]);
            }

            seampleBuildMessage(addressSlave, messageSendSlave, typeOfFunction, addressStartWrite, (ushort)numberRegistersWrite);
            Console.WriteLine("FC05 - MESSAGGIO INVIATO");
            foreach (byte m in messageSendSlave) Console.WriteLine(m);
            //2 - Try to send a messagge to slave
            try
            {
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                //3 - get respone from slave
                GetResponse(responseFromSlave);
                Console.WriteLine("FC05 - MESSAGGIO RICEVUTO");
                foreach (byte m in responseFromSlave) Console.WriteLine(m);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        public void WriteSingleRegister(byte typeOfFunction, byte addressSlave, ushort addressStartWrite, short[] valuesWriteAddress)
        {

        }

        //These are the methods used by all functions
        private void GetResponse(byte[] response)
        {
            for (int i = 0; i < response.Length; i++) response[i] = (byte)(serialPort.ReadByte());
        }

        //Alogirm CRC for find error when trasmition Data with serial communication
        private void GetCRC(byte[] message, byte[] CRC)
        {
            ushort crcFull = 0xFFFF;
            char crcLsb;

            // 1 - Implementation algoritm CRC
            for (int i = 0; i < (message.Length) - 2; i++)
            {
                //1 - XOR: only when one of the two bits 1 and the other 0 the result will be 1
                crcFull ^= (ushort)message[i];
                //2 - It cycles 8 times because each message[i] is a byte (8 bits)
                for (int j = 0; j < 8; j++)
                {
                    //3 - If crcLsb is equal to 1 it means that the last number of crcFull converted to bytes was equal to 1
                    crcLsb = (char)(crcFull & 0x0001);
                    //4 - If crcLsb different from than 0 then Shift right and crcFull XOR 0xA001 otherwise only crcFull Shift right
                    if (crcLsb != 0)
                    {
                        crcFull >>= 1;
                        crcFull ^= 0xA001;
                    }
                    else
                    {
                        crcFull >>= 1;
                    }
                }
            }
            //2 - CRC is split into two bytes, the first one shifted by 8
            CRC[1] = (byte)(crcFull >> 8);
            CRC[0] = (byte)crcFull;
        }
    }
}