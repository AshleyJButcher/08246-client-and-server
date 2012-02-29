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
        if (args.Length > 1) //Must Be At Least 2 Arguments
        {
            IPAddress = args[0]; //IP Address
            string inputname = args[1]; //Name
            if (args.Length == 4) //If there 4 arguments
            {
                if (args[3] == "ADD") //if you are adding a person
                {
                    string inputlocation = args[2]; //Location
                    SendData(SendRequest2(inputname, inputlocation)); //using a slightly different version of sendrequest(arg,arg)
                }

            }
            else if (args.Length == 3) //If You Are Updating Location
            {
                string inputlocation = args[2]; //Location
                SendData(SendRequest(inputname, inputlocation)); //Call Set Location Method
            }
            else
            {
                SendData(SendRequest(inputname)); //Call Get Location Method
            }
        }
        else
        {
            if (args.Length == 1) //If there is only one method
            {
                if (args[0].ToUpper() == "HELP") //Help Menu
                {
                    Console.WriteLine("#################################EXAMPLES#################################");
                    Console.WriteLine("127.0.0.1 Name - this will return names location");
                    Console.WriteLine("127.0.0.1 Name Location - this will set names location");
                    Console.WriteLine("127.0.0.1 Name Location ADD - this will add Name & Location to the server");
                    Console.WriteLine("NOTE: ADD must be capitalised");
                }
                else
                {
                    Console.WriteLine("Please Enter Some Arguments for the Server");
                }
            }
            else
            {
                Console.WriteLine("Please Enter Some Arguments");
            }
        }
        Console.ReadLine(); //Wont Terminate Instantly
    }

    private static string SendRequest(string Name, string Location)
    {
        return Name + " " + Location + ((char)13) + ((char)10); //Set Location Method
    }

    private static string SendRequest2(string Name, string Location)
    {
        return "ADD " + Name + " " + Location + ((char)13) + ((char)10); //Add Person Method
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

            Console.WriteLine(sr.ReadLine()); //Wait for a responce
        }
        catch (Exception e)
        {
            string message = e.Message;
            Console.WriteLine("Could not connect to server, Server Timed Out");
        }
    }
}
