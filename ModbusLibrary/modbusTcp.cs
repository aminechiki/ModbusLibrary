using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ModbusLibrary
{
    class modbusTcp : modbus
    {
        private TcpClient client;
        private Socket socket;
        public modbusTcp(string ipAddress, int port = 502)
        {
            client = new TcpClient();
            IAsyncResult result = client.BeginConnect(ipAddress, port, null, null);
            if (!result.AsyncWaitHandle.WaitOne(3000, true))
            {
                client.Close();
                throw new ApplicationException("Failed to connect to server");
            }
            else
            {
                socket = client.Client;
                socket.ReceiveTimeout = 1000;
                socket.SendTimeout = 1000;
            }
        }
        public override byte[] SendPdu(byte addressSlave, byte[] messageSendSlave, byte[] responseFromSlave, byte typeOfFunction, int startWriteAddress, int numberRegisters)
        {
            messageSendSlave = buildPdu(messageSendSlave, addressSlave, typeOfFunction, startWriteAddress, numberRegisters);
            byte[] mbapSendSlave = makeMBAP((ushort)messageSendSlave.Count());
            //Forma l'ADU unendo MBAP alla PDU
            messageSendSlave = mbapSendSlave.Concat(messageSendSlave).ToArray();

            //Array.Resize(ref messageSendSlave, messageSendSlave.Length - 2);
            //socket.Send(messageSendSlave);
            responseFromSlave = GetResponse(responseFromSlave);

            return responseFromSlave;
        }
        private byte[] buildPdu(byte[] messageSendSlave, byte addressSlave, byte typeOfFunction, int addressStart, int numberRegisters)
        {
            messageSendSlave[0] = addressSlave;
            messageSendSlave[1] = typeOfFunction;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[2] = (byte)(addressStart >> 8);
            messageSendSlave[3] = (byte)addressStart;
            //is divided into two bytes the value , the first one shifted by 8
            messageSendSlave[4] = (byte)(numberRegisters >> 8);
            messageSendSlave[5] = (byte)numberRegisters;

            return messageSendSlave;
        }
        public byte[] GetResponse(byte[] responseFromSlave)
        {
            ushort sizeAduFromSlave;
            byte[] mbapFromSlave = new byte[7];
            byte[] aduFromSlave;

            

            socket.Receive(mbapFromSlave, 0, mbapFromSlave.Length, SocketFlags.None);
            sizeAduFromSlave = mbapFromSlave[4];
            sizeAduFromSlave <<= 8;
            sizeAduFromSlave += mbapFromSlave[5];
            aduFromSlave = new byte[sizeAduFromSlave - 1];
            socket.Receive(aduFromSlave, 0, aduFromSlave.Count(), SocketFlags.None);

            responseFromSlave = mbapFromSlave.Concat(aduFromSlave).ToArray();

            return responseFromSlave;
        }
        private byte[] makeMBAP(ushort count)
        {
            return new byte[] {
                0,                  //message id high byte
                0,                  //message id low byte
                0,                  //protocol id high byte
                0,                  //protocol id low byte
                (byte)(count >> 8), //length high byte
                (byte)(count)       //length low byte
            };
        }
        public override Dictionary<int, int> orderAddressDigitalFunction(Dictionary<int, int> valueRead, byte[] responseFromSlave, int byteCount, int addressStartRead)
        {
            byte byteStartRead = 3;

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
            byte numberByteResponse = responseFromSlave[8];
            byte startReadByte = 9;

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
