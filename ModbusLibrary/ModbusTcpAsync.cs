using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ModbusLibrary.Master;

namespace ModbusLibrary
{
    class ModbusTcpAsync : modbus
    {
        private TcpClient client;
        private Socket socket;

        private static ushort _timeout = 500;

        private byte[] tcpAsyClBuffer = new byte[2048];

        public bool datoRicevuto = false;
        public ushort timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public byte[] buffer;

        public event ResponseData OnResponseData;

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
        public override byte[] SendPdu(byte addressSlave, byte[] messageSendSlave, byte[] responseFromSlave, byte typeOfFunction, int startWriteAddress, int numberRegisters)
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
            Int32 size = socket.EndSend(result);
            /*
            if (result.IsCompleted == false) CallException(0xFFFF, 0xFF, 0xFF, excSendFailt);
            else socket.BeginReceive(tcpAsyClBuffer, 0, tcpAsyClBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), tcpAsyCl);
            */

            while (!datoRicevuto)
            {
                socket.BeginReceive(tcpAsyClBuffer, 0, tcpAsyClBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);

                //datoRicevuto = true;
            }
                

            



        }

        private byte[] CreateReadWriteHeader(ushort id, byte unit, ushort startReadAddress, ushort numRead, ushort startWriteAddress, ushort numWrite)
        {
            byte[] data = new byte[numWrite * 2 + 17];

            byte[] _id = BitConverter.GetBytes((short)id);
            data[0] = _id[1];						// Slave id high byte
            data[1] = _id[0];						// Slave id low byte
            byte[] _size = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)(11 + numWrite * 2)));
            data[4] = _size[0];						// Complete message size in bytes
            data[5] = _size[1];						// Complete message size in bytes
            data[6] = unit;							// Slave address
            data[7] = 3;	// Function code
            byte[] _adr_read = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)startReadAddress));
            data[8] = _adr_read[0];					// Start read address
            data[9] = _adr_read[1];					// Start read address
            byte[] _cnt_read = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)numRead));
            data[10] = _cnt_read[0];				// Number of bytes to read
            data[11] = _cnt_read[1];				// Number of bytes to read
            byte[] _adr_write = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)startWriteAddress));
            data[12] = _adr_write[0];				// Start write address
            data[13] = _adr_write[1];				// Start write address
            byte[] _cnt_write = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)numWrite));
            data[14] = _cnt_write[0];				// Number of bytes to write
            data[15] = _cnt_write[1];				// Number of bytes to write
            data[16] = (byte)(numWrite * 2);

            return data;
        }

        private void OnReceive(System.IAsyncResult result)
        {
            
            //Console.WriteLine(22);
            Console.WriteLine("DATO RICEVUTO");


            foreach(byte c in tcpAsyClBuffer)
            {
                Console.WriteLine(c);
            }


            datoRicevuto = true;
        }

        private byte[] makeMBAP(ushort count)
        {
            return new byte[] {
                0,                  //message id high byte
                3,                  //message id low byte
                0,                  //protocol id high byte
                0,                  //protocol id low byte
                (byte)(count >> 8), //length high byte
                (byte)(6)       //length low byte
            };
        }
        public override Dictionary<int, int> orderAddressDigitalFunction(byte[] responseFromSlave, int byteCount, int addressStartRead)
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
