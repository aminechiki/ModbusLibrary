using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace ModbusLibrary
{
    class modbusRtu : modbus
    {
        private SerialPort serialPort = new SerialPort();
        public modbusRtu(string portName, int baudRate)
        {
            //Ensure port isn't already opened:
            //Assign settings to the serial port:
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            //sets the standard lenght of data bits per byte
            serialPort.DataBits = 8;
            //imposte parity and stop bit
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            //Imposte Timeout:
            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;
            //Try to open the port
            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }        
        public override byte[] SendPdu(byte addressSlave, byte[] messageSendSlave, byte[] responseFromSlave, byte typeOfFunction, int startWriteAddress, int numberRegisters)
        {
            messageSendSlave = buildPdu(messageSendSlave, addressSlave, typeOfFunction, startWriteAddress, numberRegisters);

            //messageSendSlave = buildPdu(addressSlave, typeOfFunction, startWriteAddress, numberRegisters);
            //Console.WriteLine("MESSAGGIO INVIATO");
            //foreach (byte m in messageSendSlave) Console.WriteLine(m);
            try
            {
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                GetResponse(responseFromSlave);
                //Console.WriteLine("MESSAGGIO RICEVUTO");
                //foreach (byte m in responseFromSlave) Console.WriteLine(m);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

            return responseFromSlave;
        }
        //method use inside a function for send to send pdu to slave
        private byte[] buildPdu(byte[] messageSendSlave, byte addressSlave, byte typeOfFunction, int addressStart, int numberRegisters)
        {
            byte[] CRC = new byte[2];
            messageSendSlave[0] = addressSlave;
            messageSendSlave[1] = typeOfFunction;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[2] = (byte)(addressStart >> 8);
            messageSendSlave[3] = (byte)addressStart;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[4] = (byte)(numberRegisters >> 8);
            messageSendSlave[5] = (byte)numberRegisters;

            GetCRC(messageSendSlave, CRC);
            //CRC - get the CRC with the methd and pot resul in two last position    
            messageSendSlave[messageSendSlave.Length - 2] = CRC[0];
            messageSendSlave[messageSendSlave.Length - 1] = CRC[1];

            return messageSendSlave;
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
        public override Dictionary<int, int> orderAddressDigitalFunction(byte[] responseFromSlave, int byteCount, int addressStartRead)
        {
            byte byteStartRead = 3;
            Dictionary<int, int> valueRead = new Dictionary<int, int>();

            for (int i = 0; i < byteCount; i++)
            {
                string binaryByteRead = Convert.ToString(responseFromSlave[byteStartRead + i], 2).PadLeft(8, '0');

                //6 - Converts the number of coils read from decimal to binary and enters it into the dictionary by coupling the bit to the register number
                for (int j = 0; j < binaryByteRead.Length; j++)
                {
                    valueRead.Add(addressStartRead, int.Parse(binaryByteRead[(binaryByteRead.Length - 1) - j].ToString()));
                    addressStartRead++;
                }
            }
            return valueRead;

        }
        public override Dictionary<int, int> orderAddressAnalogFunction(int addressStartRead, byte[] responseFromSlave)
        {
            byte numberByteResponse = responseFromSlave[2];
            byte startReadByte = 3;

            Dictionary<int, int> valueRead = new Dictionary<int, int>();
            int j = 0;
            //4 - sum the two bytes to form a decimal number
            for (int i = 0; i < numberByteResponse; i++)
            {
                if ((9 + i) % 2 == 0)
                {
                    valueRead.Add(addressStartRead, responseFromSlave[startReadByte + i] | responseFromSlave[(startReadByte + i) - 1] << 8);
                    addressStartRead++;
                    j++;
                }
            }
            return valueRead;
        }
    }
}