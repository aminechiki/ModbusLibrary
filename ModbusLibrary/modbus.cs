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
                //Assign desired settings to the serial port:
                serialPort.PortName = portName;
                serialPort.BaudRate = baudRate;
                serialPort.DataBits = databits;
                serialPort.Parity = parity;
                serialPort.StopBits = stopBits;
                //These timeouts are default and cannot be editted through the class at this point:
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

        public void ReadHoldingRegisters(byte addressSlave, ushort addressMemoryStartRead, ushort numberRegistersRead)
        {
            //1 - Clear buffer in In and Out of serial Port
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();

            //2 - Create a message send to slave
            byte[] messageSendSlave = new byte[8];
            byte typeOfFunction = 3;
            BuildMessage(addressSlave, typeOfFunction, addressMemoryStartRead, numberRegistersRead, messageSendSlave);

            //3 - Take the response from Slave
            byte[] response = new byte[5 + 2 * numberRegistersRead];

            //4 - Send a message to Slave 
            try
            {
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                GetResponse(response);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        private void BuildMessage(byte address, byte type, ushort start, ushort registers, byte[] message)
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];

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
        private void GetResponse(byte[] response)
        {
            Console.WriteLine("MESSAGGIO RICEVUTO");
            for (int i = 0; i < response.Length; i++)
            {
                response[i] = (byte)(serialPort.ReadByte());

                //recomposes the two bytes into a single 16-bit value
                if (i == 4)
                {
                    int risultato = response[i] | response[i - 1] << 8;
                    Console.WriteLine(risultato);
                }
            }
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