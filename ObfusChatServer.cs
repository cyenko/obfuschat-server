using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
//USES ASP NET 4.5
public class ObfusChatServer
{
    //PROPERTIES
    TcpListener server;
	public ObfusChatServer() 
	{
        server = new TcpListener(IPAddress.Parse("192.168.1.83"),8080);
        try{
            server.Start();
            Console.WriteLine("Server started");

        }
        catch(Exception E){
            Console.WriteLine("ERROR: Server failed to start. Exiting...");
            return;
        }
        TcpClient newclient = server.AcceptTcpClient();
        NetworkStream clientStream = newclient.GetStream();
        while(true){
            while(!clientStream.DataAvailable);
            Byte [] inBytes = new Byte[newclient.Available];
            clientStream.Read(inBytes,0,inBytes.Length);
            String request = Encoding.UTF8.GetString(inBytes);
            Console.WriteLine("REQUEST"+request);
            DecodeIncomingData(inBytes);
            Byte[] decoded = new Byte[inBytes.Length];
            if (new Regex("^GET").IsMatch(request))
            {
                Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                    + "Connection: Upgrade" + Environment.NewLine
                    + "Upgrade: websocket" + Environment.NewLine
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(request).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + Environment.NewLine
                    + Environment.NewLine); //Development standards require two newlines after the header
                Console.WriteLine("Handshaking back.");
                clientStream.Write(response, 0, response.Length);
                Console.WriteLine(response);
                //Once this process is complete, we can start exchanging data
            }
            else //Then it is not the handshake process, read and write stuff.
            {

                //We send them a message back here.
                Console.WriteLine("Sending message to client.");
                Byte[] response = Encoding.UTF8.GetBytes("  " + "yenko"); //There must be two spaces to start the message
                response[0] = 0x81; // denotes this is the final message and it is in text
                response[1] = (byte)(response.Length - 2); // payload size = message - header size
                clientStream.Write(response, 0, response.Length);
                  
            }
        }
    }
    private string DecodeIncomingData(Byte[] inData)
    {
        string returnString = "";
        string bitString = "";
        foreach(Byte b in inData){
            bitString += Convert.ToString(b, 2);
        }
        Console.WriteLine("Bitarray: " + bitString);
        Console.WriteLine("Length of the bit array" + bitString.Length);

        Console.WriteLine("OPCODE: " + bitString[4] + "" + bitString[5] + "" + bitString[6] + "" + bitString[7]);
        Console.WriteLine("Masked? " + bitString[8]);
        string payloadLength = bitString.Substring(9, 7);
        Console.WriteLine("Payload Length: "+ Convert.ToInt32(payloadLength,2));

        string strPayloadKey = bitString.Substring(16, 32);
        Byte[] keyByteArray = new Byte[4]; 
        Array.Copy(inData,2, keyByteArray, 0, 4);
        Console.WriteLine("Payload key: " + strPayloadKey);

        Byte[] payloadArray = new Byte[inData.Length - 6];
        Console.WriteLine("Length: " + payloadArray.Length);
        Array.Copy(inData, 6, payloadArray,0, inData.Length-6);
        Console.WriteLine("PAYLOAD: "+System.Text.Encoding.ASCII.GetString(payloadArray));
        string resultStr = "";
        Byte[] decoded = new Byte[payloadArray.Length];
        for (int i = 0; i < payloadArray.Length; i++)
        {
            decoded[i] = (Byte)(payloadArray[i] ^ keyByteArray[i % 4]);
        }
        resultStr = System.Text.Encoding.UTF8.GetString(decoded);
        Console.WriteLine("Printing str");
        Console.WriteLine("STR:" + resultStr);
        return returnString;

    }
    private static string BitArrayToString(BitArray bits, bool spaced=false)
    {
        string retString = "";
        for (int i = 0; i < bits.Count; i++)
        {
            char c = bits[i] ? '1' : '0';
            retString += c;
            if ((i+1)%4 == 0 && spaced)
            {
                retString += " ";
            }
        }
        return retString;
    }


}
