using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Xml;
using System.Threading;
namespace WhereIsServer
{
    class Program
    {
        public static List<Person> Personlist = new List<Person>();
        private static TcpListener _listener;
        public static Thread Listenthread;
        static WriteFile writexml = new WriteFile(); //Create Writer Class


        static void Main()
        {
            ReadXmlFile();
            RunServer();
        }

        public static void RunServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, 43); //Starts the Listener
                Listenthread = new Thread(ListenForClients); //Creates a New Thread for Listening for Clients
                Listenthread.Start(); //Starts the Thread
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: Starting Client Listener");
            }
        }
 
        private static void ListenForClients()
        {
            try
            {
                _listener.Start(); //Starts Listening for Clients
                while (true)
                {
                    var tcpclient = _listener.AcceptTcpClient(); //Start a New Client
                    var clientthread = new Thread(SingleClientThread); //Create a new thread
                    _clientIpAddress = "" + IPAddress.Parse(((IPEndPoint)tcpclient.Client.RemoteEndPoint).Address.ToString()); //Gets the Clients IP Address
                    clientthread.Start(tcpclient); //Start New Thread
                }
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: Starting Server");
            }
        }

        static string _clientIpAddress = "0.0.0.0"; //Default Value

        private static void SingleClientThread(object tcpclient)
        {
            TcpClient client = (TcpClient)tcpclient; //Create a new client
            NetworkStream socketStream = null;
            try
            {
                socketStream = client.GetStream(); //Get a Network Stream from the client
                socketStream.WriteTimeout = 1000; //Times Out after a Second
                socketStream.ReadTimeout = 1000; //Times Out after a Second
                DoRequest(socketStream, _clientIpAddress); //Process Information
            }
            catch (Exception)
            {
                Console.WriteLine(_clientIpAddress + ": Client Timed Out"  ); //Write Out the Error
            }
            finally
            {
                if (socketStream != null) socketStream.Close(); //Close Network Stream
                client.Close();//.Client.Close(); //Close Client
                Monitor.Enter(writexml); //Locks file so multiple threads aren't writing to the same file
                writexml.WriteXmlFile(); //Write Out the XML File
                Monitor.Exit(writexml); //Releases the method for other threads to use
            }
        }

        public static string GetPersonLocation(string name)
        {
            foreach (Person p in Personlist) //For Every Person in the list
            {
                if (name.ToUpper() == p.GetName().ToUpper()) //look for the record
                {
                    return p.GetLocation(); //return the location
                }
            }
            return "none"; //could not find location
        }

        public static bool SetPersonLocation(string name, string location)
        {
            foreach (Person p in Personlist)// for every person in the list
            {
                if (name.ToUpper() == p.GetName().ToUpper()) //find the record
                {
                    Person tempPerson = p; //store the record in a temporary person class
                    Personlist.Remove(p); //remove the old one from the list
                    tempPerson.SetLocation(location); //update the location
                    Personlist.Add(tempPerson); //put it back on the list
                     return true; //we updated the location
                }
            }
            return false; //we couldnt update the location
        }
        public  enum Types { Get, Set, None };

        public static void DoRequest(NetworkStream incoming, string ipAddress)
        {
            byte[] data = new byte[1024];
            int length = incoming.Read(data, 0, data.Length);
            if (length > 0)
            {
                string incomingtext = Encoding.ASCII.GetString(data, 0, length);
                                
                string[] input = incomingtext.Split(' ');

                Types type = Types.None;
                string name = " ";
                byte[] outputdata = new byte[1024];

                if (input.Length == 1) //If there is One Argument
                {
                    type = Types.Get; //Performing a GET
                    name = input[0]; 
                }
                else if (input.Length >= 2) //If there are more than 2 arguments
                {
                    type = Types.Set;
                    name = incomingtext;
                }


                if (type == Types.Set)
                {
                    string[] tempstrings = name.Split(new[]{' '},2);
                    string tlocation = tempstrings[1];
                    name = tempstrings[0];
                    int endlocation = tlocation.IndexOf((char)13);
                    tlocation = tlocation.Substring(0, endlocation);

                    Console.WriteLine("Client: request to change " + name + " to " + tlocation);
                        if (SetPersonLocation(name, tlocation))
                        {
                            WriteToLog(ipAddress, type + " " + name + " " + tlocation, 202, outputdata.Length);
                        }
                        else
                        {
                            Person temp = new Person(name, tlocation);
                            Personlist.Add(temp);
                            WriteToLog(ipAddress, type + " " + name + " " + tlocation, 201, outputdata.Length);
                        }
                        outputdata = Encoding.ASCII.GetBytes("OK" + (char)13 + (char)10);
                        Console.WriteLine("Server: Replied with OK");

                }
                else if (type == Types.Get)
                {
                    int endlocation = name.IndexOf((char)13);
                    name = name.Substring(0, endlocation);
                    Console.WriteLine("Client: request for " + name);
                    
                    if (GetPersonLocation(name) != "none")
                    {
                        Console.WriteLine("Server: replied in " + GetPersonLocation(name));
                        WriteToLog(ipAddress, type + " " + name, 200, 1);
                        outputdata = Encoding.ASCII.GetBytes(name + " is in " + GetPersonLocation(name) + (char)13 + (char)10);
                    }
                    else
                    {
                        outputdata = Encoding.ASCII.GetBytes("ERROR: no entries found" + (char)13 + (char)10);
                        Console.WriteLine("Server: Replied with ERROR: no entries found");
                        WriteToLog(ipAddress, type + " " + name, 500, 1);
                    }
                }

                incoming.Write(outputdata, 0, outputdata.Length);
            }
               
        }

        private static void WriteToLog(string ipaddress, string input, int response, int size)
        {
            try
            {
                string text = ipaddress + " - - " + DateTime.Now.ToString("[dd/MMM/yyyy:HH:mm:ss zz00]") + '"' + input + '"' + " " + response + " " + size; 
                StreamWriter logfile = new StreamWriter("Log.txt", true); //Write the file
                logfile.WriteLine(text); //Write a line of the log
                logfile.Close(); //Close the File
            }
            catch (Exception)
            {
                Console.WriteLine("Problems writing the log file");
            }
        }

        private static void ReadXmlFile()
        {
            try
            {
                Personlist.Clear(); //Clear the List
                XmlDocument peopleFile = new XmlDocument(); //Create New Document
                peopleFile.Load("People.xml"); //Load File
                XmlNodeList nodelist = peopleFile.GetElementsByTagName("Person"); //Get all  the data
                foreach (XmlNode node in nodelist) //For Each Person in the file
                {
                    var xmlElement = node["Name"];
                    if (xmlElement != null)
                    {
                        string name = xmlElement.InnerText; //Get the Name
                        var element = node["Location"];
                        if (element != null)
                        {
                            string loc = element.InnerText; //Get the location
                            Person temp = new Person(name, loc); //Create a Class
                            Personlist.Add(temp); //Add to List
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could Not read XML file");
                writexml.WriteXmlFile();
            }
        }
    }
    class WriteFile : Program
    {
        public void WriteXmlFile()
        {
            try
            {
                XmlWriterSettings set = new XmlWriterSettings();
                set.Indent = true; //Indents the Values
                set.NewLineOnAttributes = true; //Makes So Everything isnt on the same line
                using (XmlWriter writer = XmlWriter.Create("People.xml", set)) //Create People File
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("People"); //Parent Node
                    foreach (Person node in Personlist) //For Each Person
                    {
                        writer.WriteStartElement("Person");
                        writer.WriteElementString("Name", node.GetName()); //Writes Name to XML File
                        writer.WriteElementString("Location", node.GetLocation()); //Writes Location to XML File
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could Not Write XML File");
            }
        }

        


    }
}
