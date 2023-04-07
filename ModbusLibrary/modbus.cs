using System;
using System.IO.Ports;

namespace ModbusLibrary
{
    class modbus
    {
        private SerialPort serialPort = new SerialPort();
        public bool OpenPort(string portName, int baudRate, int databits, Parity parity, StopBits stopBits)
        {
            bool statePort = false;

            //Ensure port isn't already opened:
            if (!serialPort.IsOpen)
            {
                //Assign settings to the serial port:
                serialPort.PortName = portName;
                serialPort.BaudRate = baudRate;
                serialPort.DataBits = databits;
                serialPort.Parity = parity;
                serialPort.StopBits = stopBits;
                //Imposte Timeout:
                serialPort.ReadTimeout = 1000;
                serialPort.WriteTimeout = 1000;

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

        //FC 01 - READ COIL STATUS
        public void ReadCoilStatus(byte addressSlave, ushort coilStartRead, ushort numberCoilsRead)
        {
            byte typeOfFunction = 1;
            byte[] messageSendSlave = new byte[8];
            byte[] response = new byte[5 + numberCoilsRead];

            //2 - Create a message send to slave
            BuildMessageReadCoilStatus(addressSlave, messageSendSlave, typeOfFunction, coilStartRead, numberCoilsRead);
            Console.WriteLine("FC 01 - MESSAGGIO INVIATO");
            foreach(byte m in messageSendSlave)
            {
                Console.WriteLine(m);
            }

            //3 - Send a message to Slave 
            try
            {
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                GetResponse(response);
                Console.WriteLine("FC01 - MESSAGGIO RICEVUTO");
                for (int i = 0; i < response.Length; i++)
                {
                    Console.WriteLine(response[i]);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        public void BuildMessageReadCoilStatus(byte addressSlave, byte[] messageSendSlave, byte typeOfFunction, ushort coilStartRead, ushort numberCoilsRead) 
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];
            //- Builds the message com eindicated in the modbus protocol for function FC01
            //address Slave
            messageSendSlave[0] = addressSlave;
            //type of function
            messageSendSlave[1] = typeOfFunction;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[2] = (byte)(coilStartRead >> 8);
            messageSendSlave[3] = (byte)coilStartRead;
            //If stateOut is true send messagge 0xFF then 0x00; this result is divide in two byte
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[4] = (byte)(numberCoilsRead >> 8);
            messageSendSlave[5] = (byte)numberCoilsRead;
            //CRC - get the CRC with the methd and pot resul in two last position
            GetCRC(messageSendSlave, CRC);
            messageSendSlave[messageSendSlave.Length - 2] = CRC[0];
            messageSendSlave[messageSendSlave.Length - 1] = CRC[1];
        }


        //FC 05 - READ HOLDING REGISTERS
        public void ReadHoldingRegisters(byte addressSlave, ushort addressMemoryStartRead, ushort numberRegistersRead)
        {
            byte[] messageSendSlave = new byte[8];
            byte typeOfFunction = 3;

            //1 - Clear buffer in In and Out of serial Port
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();
            //2 - Create a message send to slave
            BuildMessageReadHoldingRegisters(addressSlave, typeOfFunction, addressMemoryStartRead, numberRegistersRead, messageSendSlave);
            
            Console.WriteLine("FC05 - MESSAGGIO INVIATO");
            foreach (byte m in messageSendSlave)
            {
                Console.WriteLine(m);
            }         

            //3 - Take the response from Slave
            byte[] response = new byte[5 + 2 * numberRegistersRead];

            //4 - Send a message to Slave 
            try
            {
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                GetResponse(response);
                Console.WriteLine("FC05 - MESSAGGIO RICEVUTO");
                for (int i = 0; i < response.Length; i++)
                {
                    if (i == 4) Console.WriteLine(response[i] | response[i - 1] << 8);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        //FC 05 WRITE SINGLE COIL
        public void WriteSingleCoil(byte addressSlave, ushort coilAddress, bool stateOut)
        {
            byte typeOfFunction = 5;
            byte[] messageSendSlave = new byte[8];
            byte[] responseFromSlave = new byte[8];

            //1 - Build a messagge send to slave
            BuildMessageWriteSingleCoil(addressSlave, messageSendSlave, typeOfFunction, coilAddress, stateOut);
            Console.WriteLine("FC05 - MESSAGGIO INVIATO");
            foreach(byte m in messageSendSlave)
            {
                Console.WriteLine(m);
            }

            try
            {
                //2 - Send a messagge to slave
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                //3 - get respone from slave
                GetResponse(responseFromSlave);
                Console.WriteLine("FC05 - MESSAGGIO RICEVUTO");
                foreach (byte m in responseFromSlave)
                {
                    Console.WriteLine(m);
                }
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

        private void BuildMessageReadHoldingRegisters(byte address, byte type, ushort start, ushort registers, byte[] message)
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];
            //Builds the message com eindicated in the modbus protocol for function FC03
            //address Slave
            message[0] = address;
            //type of function
            message[1] = type;
            //is divided into two bytes the value , the first one shifted by 8
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            //is divided into two bytes the value , the first one shifted by 8
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;

            //CRC - sono gli ultimo 2 bit
            GetCRC(message, CRC);
            message[message.Length - 2] = CRC[0];
            message[message.Length - 1] = CRC[1];
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