using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLibrary
{
    class ModbusTcpAsync
    {
        private TcpClient client;
        private Socket socket;

        private static ushort _timeout = 500;

        private byte[] tcpAsyClBuffer = new byte[2048];

        public bool datoRicevuto = false;

        public ModbusTcpAsync(string ipAddress, int port = 502)
        {
            IPAddress _ipAddress;
            if (IPAddress.TryParse(ipAddress, out _ipAddress) == false)
            {
                IPHostEntry hst = Dns.GetHostEntry(ipAddress);
                ipAddress = hst.AddressList[0].ToString();
            }

            socket = new Socket(IPAddress.Parse(ipAddress).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);
        }
        public Dictionary<int, int> readCoilStatus(byte addressSlave, int addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 1;
            byte[] messageSendSlave = new byte[8];
            int byteCount;
            int restbyteCount;
            //2 - Find ByteCount
            byteCount = numberRegistersRead / 8;
            restbyteCount = numberRegistersRead % 8;
            if (restbyteCount != 0) byteCount = byteCount + 1;
            //3 - Based onthe type ti function will be set the correct size of the response array read inputs are bits that are written in bytes,
            //so for every 8 bits you want to read the slave will respond to you with a response byte, if the bits you want to read are less than
            //8 then you will put 1 by default.
            byte[] responseFromSlave = new byte[5 + byteCount];
            //4 - Send Pdu
            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartRead, numberRegistersRead);
            //5 - Take the value read
            return orderAddressDigitalFunction(responseFromSlave, byteCount, addressStartRead);
        }
        ///<summary>
        ///<para>fc 02 - Read Discrete Inputs</para>
        ///</summary>
        ///<returns>return ...</returns>
        public Dictionary<int, int> readDiscreteInputs(byte addressSlave, int addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 2;
            byte[] messageSendSlave = new byte[8];
            int byteCount;
            int restbyteCount;
            //2 - Find ByteCount
            byteCount = numberRegistersRead / 8;
            restbyteCount = numberRegistersRead % 8;
            if (restbyteCount != 0) byteCount = byteCount + 1;
            //3 - Based onthe type ti function will be set the correct size of the response array read inputs are bits that are written in bytes, so for every 8 bits you want
            //to read the slave will respond to you with a response byte, if the bits you want to read are less than 8 then you will put 1 by default
            byte[] responseFromSlave = new byte[5 + byteCount];
            //4 - Send Pdu
            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartRead, numberRegistersRead);
            //5 - Take the value read 
            return orderAddressDigitalFunction(responseFromSlave, byteCount, addressStartRead);
        }
        ///<summary>
        ///<para>fc 03 - Read Holding Register</para>
        ///</summary>
        ///<returns>return ...</returns>
        public Dictionary<int, int> readHoldingRegisters(byte addressSlave, int addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 3;
            byte[] messageSendSlave = new byte[6];   //HO CAMBIATO QUESTOOOOOO  //PRIMA LA DIMENSIONE ERA DI 8
            byte[] responseFromSlave = new byte[5 + 2 * numberRegistersRead];
            //1 - Send messagge and wait the take the response from salve           
            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartRead, numberRegistersRead);
            //2 - sort the value by register
            //return orderAddressAnalogFunction(addressStartRead, responseFromSlave);


            //socket.(byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, AsyncCallback ? callback, object ? state);

            Dictionary<int, int> p = new Dictionary<int, int>();

            return p;
        }
        ///<summary>
        ///<para>fc 04 - Read Input Register</para>
        ///</summary>
        ///<returns>return ...</returns>
        public Dictionary<int, int> readInputRegisters(byte addressSlave, int addressStartRead, byte numberRegistersRead)
        {
            byte typeOfFunction = 4;
            byte[] messageSendSlave = new byte[8];
            byte[] responseFromSlave = new byte[5 + 2 * numberRegistersRead];
            //1 - Send messagge and wait the take the response from salve          
            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartRead, numberRegistersRead);
            //2 - sort the value by register
            return orderAddressAnalogFunction(addressStartRead, responseFromSlave);
        }

        //WRITE
        ///<summary>
        ///<para>fc 05 - Write Single Coil</para>
        ///</summary>
        ///<returns>return ...</returns
        public void writeSingleCoil(byte addressSlave, int addressStartWrite, bool stateCoil)
        {
            byte typeOfFunction = 5;
            byte[] messageSendSlave = new byte[8];
            byte[] responseFromSlave = new byte[8];
            bool checkResponse = true;
            int numberRegisters = 0;
            //2 - if stateCoil is true numberRegisters is equal 0xFF00
            if (stateCoil) numberRegisters = 0xFF00;
            if (!stateCoil) numberRegisters = 0x0000;
            //3 - Send Pdu
            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartWrite, numberRegisters);
        }
        ///<summary>
        ///<para>fc 06 -Write Single Register</para>
        ///</summary>
        ///<returns>return ...</returns>
        public void writeSingleRegister(byte addressSlave, int addressStartWrite, int valuesWriteAddress)
        {
            byte typeOfFunction = 6;
            bool checkResponse = true;
            byte[] messageSendSlave = new byte[6];
            byte[] responseFromSlave = new byte[8];
            //2- Send Pdu


            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, (int)addressStartWrite, valuesWriteAddress);
        }
        ///<summary>
        ///<para>fc 15 - Write Multiple Coils</para>
        ///</summary>
        ///<returns>return ...</returns>
        ///
        public void writeMultipleCoils(byte addressSlave, byte addressStartWrite, long valuesWriteAddress)
        {
            byte typeOfFunction = 15;
            bool checkResponse = true;
            byte[] messageSendSlave = new byte[0];
            byte[] responseFromSlave = new byte[8];
            int numberRegisters = 0;
            //2 - Find numberRegisters
            //while (valuesWriteAddress > Math.Pow(2, numberRegisters)) numberRegisters++;
            string valuesWriteAddressToBit = Convert.ToString(valuesWriteAddress, 2);
            numberRegisters = valuesWriteAddressToBit.Length;
            //3 - Based onthe type ti function will be set the correct size of the response array
            if (numberRegisters > 8)
            {
                messageSendSlave = new byte[11];
                messageSendSlave[7] = (byte)(valuesWriteAddress >> 8);
                messageSendSlave[8] = (byte)valuesWriteAddress;
            }
            if (numberRegisters <= 8)
            {
                messageSendSlave = new byte[10];
                messageSendSlave[7] = (byte)valuesWriteAddress;
            }
            //Byte count
            int byteCount = numberRegisters / 8;
            int restbyteCount = numberRegisters % 8;
            if (restbyteCount != 0) byteCount = byteCount + 1;
            messageSendSlave[6] = (byte)byteCount;
            //4 - Send Pdu
            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartWrite, numberRegisters);
        }
        ///<summary>
        ///<para>fc 16 - Write Multiple Registers</para>
        ///</summary>
        ///<returns>return ...</returns>
        ///
        public void writeMultipleRegisters(byte addressSlave, int addressStartWrite, int[] valuesWriteAddress)
        {
            byte typeOfFunction = 16;
            bool checkResponse = true;
            //Take the numbers of register do you want write
            byte numberRegistersWrite = (byte)valuesWriteAddress.Length;
            byte[] messageSendSlave = new byte[9 + 2 * numberRegistersWrite];
            byte[] responseFromSlave = new byte[8];
            //Add bytecount to message:
            messageSendSlave[6] = (byte)(numberRegistersWrite * 2);
            //2 - Put write values into message prior to sending:
            for (int i = 0; i < numberRegistersWrite; i++)
            {
                messageSendSlave[7 + 2 * i] = (byte)(valuesWriteAddress[i] >> 8);
                messageSendSlave[8 + 2 * i] = (byte)(valuesWriteAddress[i]);
            }
            //3 - Send Pdu
            responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartWrite, numberRegistersWrite);
        }
        public byte[] SendPdu(byte addressSlave, byte[] messageSendSlave, byte[] responseFromSlave, byte typeOfFunction, int startWriteAddress, int numberRegisters)
        {
            messageSendSlave = buildPdu(messageSendSlave, addressSlave, typeOfFunction, startWriteAddress, numberRegisters);
            byte[] mbapSendSlave = makeMBAP((ushort)messageSendSlave.Count());
            //Forma l'ADU unendo MBAP alla PDU
            messageSendSlave = mbapSendSlave.Concat(messageSendSlave).ToArray();

            //Array.Resize(ref messageSendSlave, messageSendSlave.Length - 2);
            socket.Send(messageSendSlave);
            responseFromSlave = GetResponse(responseFromSlave, messageSendSlave);

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
        public byte[] GetResponse(byte[] responseFromSlave, byte[] messageSendSlave)
        {

            //socket.BeginReceive(byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, AsyncCallback ? callback, object ? state);

            //messageSendSlave = CreateReadWriteHeader(0, 0, 0, 2, 0, 0);

            socket.BeginSend(messageSendSlave, 0, messageSendSlave.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

            //socket.Receive(buffer);

            //buffer = messageSendSlave;
            return responseFromSlave;
            /*
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
   */
        }

        private void OnSend(System.IAsyncResult result)
        {
            datoRicevuto = false;
            while (!datoRicevuto)
            {
                socket.BeginReceive(tcpAsyClBuffer, 0, tcpAsyClBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
            }
        }
        private void OnReceive(System.IAsyncResult result)
        {
            datoRicevuto = true;
            
        }

        private byte[] makeMBAP(ushort count)
        {
            //capire come camabiare il codice del id del messaggio

            return new byte[] {
                0,                  //message id high byte
                0,                  //message id low byte
                0,                  //protocol id high byte
                0,                  //protocol id low byte
                (byte)(count >> 8), //length high byte
                (byte)(6)       //length low byte
            };
        }
        public Dictionary<int, int> orderAddressDigitalFunction(byte[] responseFromSlave, int byteCount, int addressStartRead)
        {
            byte byteStartRead = 9;
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
        public Dictionary<int, int> orderAddressAnalogFunction(int addressStartRead, byte[] responseFromSlave)
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
