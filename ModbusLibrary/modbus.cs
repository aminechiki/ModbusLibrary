using System;
using System.IO.Ports;

namespace ModbusLibrary
{
    class modbusRtu
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
        // FC 01 - READ COIL STATUS / FC 02 - READ INPUT STATUS / FC 03 READ HOLDING REGISTERS / FC 04 READ INPUT REGISTERS
        public int[] readModbus(byte typeOfFunction, byte addressSlave, byte addressStartRead, byte numberRegistersRead)
        {
            byte[] messageSendSlave = new byte[8];
            byte[] responseFromSlave = new byte[0];
            int[] valueRead = new int[0];

            int byteCount;
            int restbyteCount;
            //1 - Clear buffer in In and Out of serial Port
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();      
            //2 - Find ByteCount
            byteCount = numberRegistersRead / 8;
            restbyteCount = numberRegistersRead % 8;
            if (restbyteCount != 0) byteCount = byteCount + 1;
            //3 - Based onthe type ti function will be set the correct size of the response array
            switch (typeOfFunction)
            {
                case 1: case 2:               
                    //read inputs are bits that are written in bytes, so for every 8 bits you want
                    //to read the slave will respond to you with a response byte, if the bits you
                    //want to read are less than 8 then you will put 1 by default
                    responseFromSlave = new byte[5 + byteCount];
                    SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartRead, numberRegistersRead);
                    valueRead = new int[byteCount];
                    for (int i = 0; i < byteCount; i++) valueRead[i] = responseFromSlave[3 + i];
                    break;
                case 3: case 4:             
                    responseFromSlave = new byte[5 + 2 * numberRegistersRead];
                    valueRead = new int[numberRegistersRead];
                    SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartRead, numberRegistersRead);
                    byte incremento = 0;
                    for (int i = 0; i < responseFromSlave[2]; i++)
                    {
                        if((3 + i) % 2 == 0)
                        {

                            valueRead[incremento] = responseFromSlave[3 + i] | responseFromSlave[3 + i + 1] << 8; 
                            incremento++;
                        }

                        //valueRead[i] = responseFromSlave[3 + i];

                    }

                    valueRead = new int[numberRegistersRead];                  
                    break;                                
            }
            return valueRead;
        }

        // - FUNCTION FOR WRITE 
        //FC 05 - WRITE SINGLE COIL / FC 15 - WRITE MULTIPLE COILS
        public void writeCoil(byte typeOfFunction, byte addressSlave, byte addressStartWrite, int numberRegisters, int bitWriteMultipleCoils = 0)
        {
            byte[] messageSendSlave = new byte[0];
            byte[] responseFromSlave = new byte[8];
            //1 - Clear buffer in In and Out of serial Port
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();
            //2 - Based onthe type ti function will be set the correct size of the response array
            switch (typeOfFunction)
            {
                case 5:                  
                    messageSendSlave = new byte[8];
                    break;
                case 15:
                    if (numberRegisters > 8)
                    {
                        messageSendSlave = new byte[11];
                        messageSendSlave[7] = (byte)(bitWriteMultipleCoils >> 8);
                        messageSendSlave[8] = (byte)bitWriteMultipleCoils;
                    }
                    if (numberRegisters <= 8)
                    {
                        messageSendSlave = new byte[10];
                        messageSendSlave[7] = (byte)bitWriteMultipleCoils;
                    }
                    //Byte count
                    int byteCount = numberRegisters / 8;
                    int restbyteCount = numberRegisters % 8;
                    if (restbyteCount != 0) byteCount = byteCount + 1;
                    messageSendSlave[6] = (byte)byteCount;
                    break;
            }
            SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartWrite, numberRegisters);
        }

        //FC 06 - WRITE SINGLE REGISTERS
        //Ovveride method
        public void writeRegister(byte typeOfFunction, byte addressSlave, byte addressStartWrite, int valuesWriteAddress)
        {
                byte[] messageSendSlave = new byte[8];
                byte[] responseFromSlave = new byte[8];
                //1 - Clear buffer in In and Out of serial Port
                serialPort.DiscardOutBuffer();
                serialPort.DiscardInBuffer();
                SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartWrite, valuesWriteAddress);
        }

        //FC 16 - WRITE MULTIPLE REGISTERS
        public void writeRegister(byte typeOfFunction, byte addressSlave, byte addressStartWrite, int[] valuesWriteAddress)
        {
                //Take the numbers of register do you want write
                byte numberRegistersWrite = (byte)valuesWriteAddress.Length;
                byte[] messageSendSlave = new byte[9 + 2 * numberRegistersWrite];
                byte[] responseFromSlave = new byte[8];
                //Add bytecount to message:
                messageSendSlave[6] = (byte)(numberRegistersWrite * 2);
                //1 - Clear buffer in In and Out of serial Port
                serialPort.DiscardOutBuffer();
                serialPort.DiscardInBuffer();

                //2 - Put write values into message prior to sending:
                for (int i = 0; i < numberRegistersWrite; i++)
                {
                    messageSendSlave[7 + 2 * i] = (byte)(valuesWriteAddress[i] >> 8);
                    messageSendSlave[8 + 2 * i] = (byte)(valuesWriteAddress[i]);
                }
                //3 - Send Pdu
                SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartWrite, numberRegistersWrite);
        }

        // - METHOD USE IN ALL APPLICATION

        //method use inside a function for send to send pdu to slave 
        private void SendPdu(byte addressSlave, byte[] messageSendSlave, byte[] responseFromSlave, byte typeOfFunction, byte startWriteAddress, int numberRegisters)
        {
            buildPdu(addressSlave, messageSendSlave, typeOfFunction, startWriteAddress, numberRegisters);
            
            Console.WriteLine("MESSAGGIO INVIATO");
            foreach (byte m in messageSendSlave) Console.WriteLine(m);
            try
            {
                serialPort.Write(messageSendSlave, 0, messageSendSlave.Length);
                GetResponse(responseFromSlave);
                Console.WriteLine("MESSAGGIO RICEVUTO");
                foreach (byte m in responseFromSlave) Console.WriteLine(m);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        //method for Build a messagge to send to slave fro read
        private void buildPdu(byte addressSlave, byte[] messageSendSlave, byte typeOfFunction, ushort addressStart, int numberRegisters)
        {
            //Array to receive CRC bytes: 
            byte[] CRC = new byte[2];
            //- Builds the message com eindicated in the modbus protocol for function
            //address Slave
            messageSendSlave[0] = addressSlave;
            //type of function
            messageSendSlave[1] = typeOfFunction;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[2] = (byte)(addressStart >> 8);
            messageSendSlave[3] = (byte)addressStart;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[4] = (byte)(numberRegisters >> 8);
            messageSendSlave[5] = (byte)numberRegisters;
            //CRC - get the CRC with the methd and pot resul in two last position
            GetCRC(messageSendSlave, CRC);
            messageSendSlave[messageSendSlave.Length - 2] = CRC[0];
            messageSendSlave[messageSendSlave.Length - 1] = CRC[1];
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