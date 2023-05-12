using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary
{
    abstract class modbus
    {
        //READ
        ///<summary>
        ///<para>fc 01 - Read Coil Status</para>
        ///</summary>
        ///<returns>return ...</returns>
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

        //METHOD USE INSIDE FUNCTIONS
        public abstract byte[] SendPdu(byte addressSlave, byte[] messageSendSlave, byte[] responseFromSlave, byte typeOfFunction, int startWriteAddress, int numberRegisters);
        public abstract Dictionary<int, int> orderAddressDigitalFunction(byte[] responseFromSlave, int byteCount, int addressStartRead);
        public abstract Dictionary<int, int> orderAddressAnalogFunction(int addressStartRead, byte[] responseFromSlave);
    }
}