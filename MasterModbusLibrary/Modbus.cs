﻿using System;
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
    abstract class Modbus
    {
        protected bool _ConnectionType { get; set; }               //true = async 
        protected bool _Connected;
        public byte[] _ResponseFromSlave = new byte[2048];
        protected byte[] DataRecived;
        protected byte[] MessageSendSlave;
        protected static ushort _timeout = 1000;

        //vengono richiamti quando arriva un uovo dato  
        public delegate void ResponseData(byte[] data);
        public Modbus() 
        {

        }

        //READ
        ///<para>fc 01 - Read Coil Status</para>
        ///</summary>
        ///<returns>return ...</returns>
        public void readCoilStatus(byte slaveId, int startAddress, int numAddress)
        {

        }
        ///<summary>
        ///<para>fc 02 - Read Discrete Inputs</para>
        ///</summary>
        ///<returns>return ...</returns>
        public void readDiscreteInputs(byte slaveId, int startAddress, int numAddress)
        {

        }
        ///<summary>
        ///<para>fc 03 - Read Holding Register</para>
        ///</summary>
        ///<returns>return ...</returns>
        public void readHoldingRegisters(byte slaveId, int startAddress, int numAddress)
        {
            Dictionary<int, int> p = new Dictionary<int, int>();
            byte functionCode = 3;
            //Build a Paket 
            BuildPacket(slaveId, functionCode, startAddress, numAddress);
            //Send Packet 
            //else responseFromSlave = GetSyncResponse(messageSendSlave);
            SendPacket();
        }
        ///<summary>
        ///<para>fc 04 - Read Input Register</para>
        ///</summary>
        ///<returns>return ...</returns>
        public void readInputRegisters(byte addressSlave, int addressStartRead, byte numberRegistersRead)
        {

        }

        //WRITE
        ///<summary>
        ///<para>fc 05 - Write Single Coil</para>
        ///</summary>
        ///<returns>return ...</returns
        public void writeSingleCoil(byte slaveId, int startAddress, bool stateCoil)
        {
            byte functionCode = 5;
            int numAddress = 0;
            //2 - if stateCoil is true numberRegisters is equal 0xFF00
            if (stateCoil) numAddress = 0xFF00;
            if (!stateCoil) numAddress = 0x0000;
            //Build a Paket 
            BuildPacket(slaveId, functionCode, startAddress, numAddress);
            //Send Packet 
            //else responseFromSlave = GetSyncResponse(messageSendSlave);
            SendPacket();

            //3 - Send Pdu
            //responseFromSlave = SendPdu(addressSlave, messageSendSlave, responseFromSlave, typeOfFunction, addressStartWrite, numberRegisters);
        }
        ///<summary>
        ///<para>fc 15 - Write Multiple Coils</para>
        ///</summary>
        ///<returns>return ...</returns>
        ///
        public void writeMultipleCoils(byte slaveId, int startAddress, int numAddress)
        {
            byte typeOfFunction = 15;
            //Build a Paket 
            BuildPacket(slaveId, typeOfFunction, startAddress, numAddress);
            //Send Packet 
            //else responseFromSlave = GetSyncResponse(messageSendSlave);
            SendPacket();
        }
        ///<summary>
        ///<para>fc 06 -Write Single Register</para>
        ///</summary>
        ///<returns>return ...</returns>
        public void writeSingleRegister(byte slaveId, int startAddress, int numAddress)
        {
            byte functionCode = 6;

            //Build a Paket 
            BuildPacket(slaveId, functionCode, startAddress, numAddress);
            //Send Packet 
            SendPacket();
            //Get response from slave, if connection if Sync wait the response from slave
            //GetResponse();
            //else responseFromSlave = GetSyncResponse(messageSendSlave);
        }
        ///<summary>
        ///<para>fc 16 - Write Multiple Registers</para>
        ///</summary>
        ///<returns>return ...</returns>
        public void writeMultipleRegisters(byte slaveId, int startAddress, int[] numAddress)
        {
            byte functionCode = 16;

            //Build a Paket 
            BuildPacket(slaveId, functionCode, startAddress, numAddress);
            //Send Packet 
            SendPacket();
            //Get response from slave, if connection if Sync wait the response from slave
            //GetResponse();
            //else responseFromSlave = GetSyncResponse(messageSendSlave);
        }

        //METHODS
        public List<byte> buildPdu(byte functionCode, int startAddress, int numAddress)
        {
            //FUNCTION 1,2,3,4,6 
            //PDU = FUNCTION CODE + DATA

            List<byte> pdu = new List<byte>();                               //compose the request
                                                                             //Add function code in header
            pdu.Add((byte)(functionCode));
            //Add start address and number of address 
            pdu.Add((byte)(startAddress >> 8));
            pdu.Add((byte)(startAddress));

            switch (functionCode)
            {
                case 1 or 2 or 3 or 4 or 5 or 6:
                    pdu.Add((byte)(numAddress >> 8));
                    pdu.Add((byte)(numAddress));
                    break;
                case 15:
                    int numberRegisters = 0;
                    //2 - Find numberRegisters
                    string valuesWriteAddressToBit = Convert.ToString(numAddress, 2);
                    valuesWriteAddressToBit = valuesWriteAddressToBit.PadLeft(8, '0');
                    numberRegisters = valuesWriteAddressToBit.Length;
                    //3 - Based onthe type ti function will be set the correct size of the response array
                    pdu.Add((byte)(0));
                    pdu.Add((byte)(numberRegisters));
                    //Byte count
                    int byteCount = numberRegisters / 8;
                    int restbyteCount = numberRegisters % 8;
                    if (restbyteCount != 0) byteCount = byteCount + 1;

                    pdu.Add((byte)((byte)byteCount));

                    if (numberRegisters <= 8) pdu.Add((byte)((byte)numAddress));
                    if (numberRegisters >= 8)
                    {
                        pdu.Add((byte)((byte)numAddress >> 8));
                        pdu.Add((byte)((byte)numAddress));
                    }

                    break;
            }
            return pdu;
        }
        public List<byte> buildPdu(byte functionCode, int startAddress, int[] valuesnumAddress)
        {
            List<byte> pdu = new List<byte>();                               //compose the request
            //Add function code in header
            pdu.Add((byte)(functionCode));
            //Add start address and number of address 
            pdu.Add((byte)(startAddress >> 8));
            pdu.Add((byte)(startAddress));

            switch (functionCode)
            {
                case 16:
                    byte numberRegistersWrite = (byte)valuesnumAddress.Length;

                    pdu.Add((byte)(numberRegistersWrite >> 8));
                    pdu.Add((byte)(numberRegistersWrite));

                    //Add bytecount to message:
                    pdu.Add((byte)(numberRegistersWrite * 2));
                    //2 - Put write values into message prior to sending:
                    for (int i = 0; i < numberRegistersWrite; i++)
                    {
                        pdu.Add((byte)(valuesnumAddress[i] >> 8));
                        pdu.Add((byte)(valuesnumAddress[i]));
                    }
                    break;
            }
            return pdu;
        }

        //ABSTRACT METHODS

        public abstract void BuildPacket(byte slaveId, byte functionCode, int startAddress, int[] numAddress);
        public abstract void BuildPacket(byte slaveId, byte functionCode, int startAddress, int numAddress);
        public abstract void SendPacket();
        public abstract void GetResponse();
    }
}