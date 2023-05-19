﻿using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace ModbusLibrary
{
    class ModbusRtu : Modbus
    {
        private SerialPort serialPort = new SerialPort();
        public ModbusRtu(string portName, int baudRate)
        {

            /*
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
            */
        }

        public ushort GetCrc(byte[] pdu)
        {
            ushort crcFull = 0xFFFF;
            char crcLsb;
            // 1 - Implementation algoritm CRC
            for (int i = 0; i < (pdu.Length) - 2; i++)
            {
                //1 - XOR: only when one of the two bits 1 and the other 0 the result will be 1
                crcFull ^= (ushort)pdu[i];
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

            return crcFull;
        }

        public override void BuildPacket(byte slaveId, byte functionCode, int startAddress, int numAddress)
        {
            throw new NotImplementedException();
        }

        public override void SendPacket()
        {
            throw new NotImplementedException();
        }

        public override void GetResponse()
        {
            throw new NotImplementedException();
        }

        public override void BuildPacket(byte slaveId, byte functionCode, int startAddress, int[] numAddress)
        {
            throw new NotImplementedException();
        }

        public override bool OpenConnection(string ipAddress, int port, bool connectionType)
        {
            throw new NotImplementedException();
        }

        //AGGIUNGERE METODI PER L'ORDINAMENTO DELLA RISPOSTA



    }
}