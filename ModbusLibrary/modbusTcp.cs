using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ModbusLibrary
{
    class modbusTcp
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

        //fc 01
        public Dictionary<int, int> readCoilStatus(byte addressSlave, byte addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 1;
            byte[] responseFromSlave = new byte[0];
            byte[] messageSendSlave = new byte[0];
            Dictionary<int, int> valueRead = new Dictionary<int, int>();

            int byteCount;
            int restbyteCount;

            //1 - build and send messageSendSlave and take responseFromSlave  
            responseFromSlave = sendPdu(addressSlave, typeOfFunction, messageSendSlave, responseFromSlave, addressStartRead, numberRegistersRead);
            //2 - sum the two bytes to form a decimal number
            //valueRead = orderAddressValueRead(responseFromSlave[8], 9, addressStartRead, responseFromSlave);

            //2 - Find ByteCount
            byteCount = numberRegistersRead / 8;
            restbyteCount = numberRegistersRead % 8;
            if (restbyteCount != 0) byteCount = byteCount + 1;

            //3 - Take the value read
            for (int i = 0; i < byteCount; i++)
            {
                string binaryByteRead = Convert.ToString(responseFromSlave[9 + i], 2).PadLeft(8, '0');

                //4 - Converts the number of coils read from decimal to binary and enters it into the dictionary by coupling the bit to the register number
                for (int j = 0; j < binaryByteRead.Length; j++)
                {
                    valueRead.Add(addressStartRead, int.Parse(binaryByteRead[(binaryByteRead.Length - 1) - j].ToString()));
                    addressStartRead++;
                }
            }

            return valueRead;
        }
        //fc 02
        public Dictionary<int, int> readDiscreteInputs(byte addressSlave, byte addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 2;
            byte[] responseFromSlave = new byte[0];
            byte[] messageSendSlave = new byte[0];
            Dictionary<int, int> valueRead = new Dictionary<int, int>();

            int byteCount;
            int restbyteCount;

            //1 - build and send messageSendSlave and take responseFromSlave  
            responseFromSlave = sendPdu(addressSlave, typeOfFunction, messageSendSlave, responseFromSlave, addressStartRead, numberRegistersRead);
            //2 - sum the two bytes to form a decimal number
            //valueRead = orderAddressValueRead(responseFromSlave[8], 9, addressStartRead, responseFromSlave);

            //2 - Find ByteCount
            byteCount = numberRegistersRead / 8;
            restbyteCount = numberRegistersRead % 8;
            if (restbyteCount != 0) byteCount = byteCount + 1;

            //3 - Take the value read
            for (int i = 0; i < byteCount; i++)
            {
                string binaryByteRead = Convert.ToString(responseFromSlave[9 + i], 2).PadLeft(8, '0');

                //4 - Converts the number of coils read from decimal to binary and enters it into the dictionary by coupling the bit to the register number
                for (int j = 0; j < binaryByteRead.Length; j++)
                {
                    valueRead.Add(addressStartRead, int.Parse(binaryByteRead[(binaryByteRead.Length - 1) - j].ToString()));
                    addressStartRead++;
                }
            }
            return valueRead;
        }
        //fc 03
        public Dictionary<int, int> readHoldingRegisters(byte addressSlave, byte addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 3;
            byte[] responseFromSlave = new byte[0];
            byte[] messageSendSlave = new byte[0];
            Dictionary<int, int> valueRead = new Dictionary<int, int>();

            //1 - build and send messageSendSlave and take responseFromSlave  
            responseFromSlave = sendPdu(addressSlave, typeOfFunction, messageSendSlave, responseFromSlave, addressStartRead, numberRegistersRead);
            //2 - sum the two bytes to form a decimal number
            valueRead = orderAddressValueRead(responseFromSlave[8], 9, addressStartRead, responseFromSlave);

            return valueRead;
        }
        //fc 04
        public Dictionary<int, int> readInputRegisters(byte addressSlave, byte addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 4;
            byte[] responseFromSlave = new byte[0];
            byte[] messageSendSlave = new byte[0];
            Dictionary<int, int> valueRead = new Dictionary<int, int>();
            int byteCount;
            int restbyteCount;

            //1 - build and send messageSendSlave and take responseFromSlave  
            responseFromSlave = sendPdu(addressSlave, typeOfFunction, messageSendSlave, responseFromSlave, addressStartRead, numberRegistersRead);
            //2 - sum the two bytes to form a decimal number
            valueRead = orderAddressValueRead(responseFromSlave[8], 9, addressStartRead, responseFromSlave);

            return valueRead;
        }
        //fc 05
        public bool writeSingleCoil(byte addressSlave, byte addressStartWrite, bool stateCoil)
        {
            byte typeOfFunction = 6;
            byte[] messageSendSlave = new byte[0];
            byte[] responseFromSlave = new byte[0];
            bool checkResponse = true;
            int numberRegisters = 0;

            //2 - if stateCoil is true numberRegisters is equal 0xFF00
            if (stateCoil) numberRegisters = 0xFF00;
            if (!stateCoil) numberRegisters = 0x0000;

            responseFromSlave = sendPdu(addressSlave, typeOfFunction, messageSendSlave, responseFromSlave, addressStartWrite, numberRegisters);

            /*
            //4 - Chenk reponde
            if (messageSendSlave.Length != responseFromSlave.Length) checkResponse = false;
            for (int i = 0; i < messageSendSlave.Length; i++)
            {
                if (messageSendSlave[i] != responseFromSlave[i]) checkResponse = false;
            }
            */
            return checkResponse;
        }
        //fc 06
        public bool writeSingleRegister(byte addressSlave, byte addressStartWrite, int valuesWriteAddress)
        {
            byte typeOfFunction = 6;
            bool checkResponse = true;
            byte[] messageSendSlave = new byte[0];
            byte[] responseFromSlave = new byte[0];

            //2- Send Pdu
            responseFromSlave = sendPdu(addressSlave, typeOfFunction, messageSendSlave, responseFromSlave, addressStartWrite, valuesWriteAddress);

            return checkResponse;
        }
        public Dictionary<int, int> orderAddressValueRead(byte numberByteResponse ,byte startReadByte, byte addressStartRead, byte[] responseFromSlave)
        {
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
        public byte[] sendPdu(byte addressSlave, byte typeOfFunction, byte[] messageSendSlave, byte[] responseFromSlave, byte addressStartRead, int numberRegisterRead)
        {
            byte[] pdu = buildPdu(addressSlave, typeOfFunction, addressStartRead, numberRegisterRead);
            byte[] mbapSendSlave = makeMBAP((ushort)pdu.Count());
            //Forma l'ADU unendo MBAP alla PDU
            messageSendSlave = mbapSendSlave.Concat(pdu).ToArray();

            socket.Send(messageSendSlave);
            responseFromSlave = GetResponse(responseFromSlave);

            return responseFromSlave;
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
        protected byte[] buildPdu(byte addressSlave, byte function, ushort register, int count)
        {
            byte[] messageSendSlave = new byte[]
            {
                addressSlave,           //slave address
                function,               //function code
                (byte)(register >> 8),  //start register high
                (byte)register,         //start register low
                (byte)(count >> 8),     //# of registers high
                (byte)count
            };

            return messageSendSlave;
        }
    }
}
