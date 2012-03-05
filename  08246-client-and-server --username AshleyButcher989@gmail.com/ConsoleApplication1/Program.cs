//Demonstrate Sockets

using System;
using System.Net.Sockets;
using System.IO;

namespace WhereIs
{
    public class Whois
    {

        static StreamWriter _sw;
        static TcpClient client = new TcpClient();
        static StreamReader _sr;
        static string _ipAddress = "127.0.0.1"; //Default Value

        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Please Enter Some Arguments");
                    break;

                case 1:
                    if (args[0] != "-h")
                        SendData(SendRequest(args[0])); //Get Request Local IP
                    else
                        Console.WriteLine("Please Enter an IP for the Server");
                    break;

                case 2:
                    if (args[0] != "-h")
                        SendData(SendRequest(args[0],args[1])); //Set Request Local IP
                    else
                        Console.WriteLine("Please Enter an arguments for the Server");
                    break;

                case 3:
                    if (args[0] == "-h")
                    {
                        _ipAddress = args[1]; //Set IP Address
                        SendData(SendRequest(args[2])); //Get Request Non Local IP
                    }
                    else
                        Console.WriteLine("Incorrect String");
                    break;

                case 4:
                    if (args[0] == "-h")
                    {
                        _ipAddress = args[1]; //Set IP Address
                        SendData(SendRequest(args[2],args[3])); //Set Request Non Local IP
                    }
                    else
                        Console.WriteLine("Incorrect String");
                    break;
            }
        }

        private static string SendRequest(string name, string location)
        {
            return name + " " + location; //Set Location Method
        }

        private static string SendRequest(string name)
        {
            return name; //Get Location Method
        }

        private static void SendData(string args)
        {
            try
            {
                client.SendTimeout = 1000;
                client.Connect(_ipAddress, 43); //Connect to Server
                _sw = new StreamWriter(client.GetStream()); //New StreamWriter
                _sr = new StreamReader(client.GetStream()); //New StreamReader
                _sw.WriteLine(args); //Write Out Our Command
                _sw.Flush(); //Make sure it writes

                Console.WriteLine(_sr.ReadToEnd()); //Wait for a responce
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: Connection Timed Out");
            }
        }
    }
}
