using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
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
        server = new TcpListener(IPAddress.Parse("127.0.0.1"),8080);
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
                        + Environment.NewLine);
                    Console.WriteLine("Got response.");
                    clientStream.Write(response, 0, response.Length);
                    Console.WriteLine(response);
                }
                else
                {
                    //We send them a message back here.
                    //Console.WriteLine(request);
                    Byte[] response = Encoding.UTF8.GetBytes("  " + "yenko");
                    response[0] = 0x81; // denotes this is the final message and it is in text
                    response[1] = (byte)(response.Length - 2); // payload size = message - header size
                    clientStream.Write(response, 0, response.Length);
                  
                }
            }
    }


}
