//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
public class Whois
{

    static StreamWriter sw;
    static TcpClient client = new TcpClient();
    static StreamReader sr;
    static string IPAddress = "127.0.0.1"; //Default Value

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
                    IPAddress = args[1]; //Set IP Address
                    SendData(SendRequest(args[2])); //Get Request Non Local IP
                }
                else
                    Console.WriteLine("Incorrect String");
            break;

            case 4:
                if (args[0] == "-h")
                {
                    IPAddress = args[1]; //Set IP Address
                    SendData(SendRequest(args[2],args[3])); //Set Request Non Local IP
                }
                else
                    Console.WriteLine("Incorrect String");
            break;
        }
    }

    private static string SendRequest(string Name, string Location)
    {
        return Name + " " + Location + ((char)13) + ((char)10); //Set Location Method
    }

    private static string SendRequest(string Name)
    {
        return Name + ((char)13) + ((char)10); //Get Location Method
    }

    private static void SendData(string args)
    {
        try
        {
            client.SendTimeout = 1000;
            client.Connect(IPAddress, 43); //Connect to Server
            sw = new StreamWriter(client.GetStream()); //New StreamWriter
            sr = new StreamReader(client.GetStream()); //New StreamReader
            sw.WriteLine(args); //Write Out Our Command
            sw.Flush(); //Make sure it writes

            Console.WriteLine(sr.ReadToEnd()); //Wait for a responce
        }
        catch (Exception e)
        {
            string message = e.Message;
            Console.WriteLine("ERROR: Connection Timed Out");
        }
    }
}
