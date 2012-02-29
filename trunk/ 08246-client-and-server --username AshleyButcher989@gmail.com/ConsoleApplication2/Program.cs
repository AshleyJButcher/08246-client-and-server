using System;
using System.Collections.Generic;
using System.Linq;
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
        public static List<Person> personlist = new List<Person>();
        private static TcpListener listener;
        public static Thread listenthread;
        static WriteFile writexml = new WriteFile(); //Create Writer Class


        static void Main(string[] args)
        {
            ReadXMLFile();
            runServer();
        }

        public static void runServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 43); //Starts the Listener
                listenthread = new Thread(new ThreadStart(ListenForClients)); //Creates a New Thread for Listening for Clients
                listenthread.Start(); //Starts the Thread
            }
            catch (Exception e)
            {
                string warning = e.Message;
                Console.WriteLine("ERROR: Starting Client Listener");
            }
        }
 
        private static void ListenForClients()
        {
            try
            {
                listener.Start(); //Starts Listening for Clients
                while (true)
                {
                    TcpClient tcpclient = listener.AcceptTcpClient(); //Start a New Client
                    Thread clientthread = new Thread(new ParameterizedThreadStart(SingleClientThread)); //Create a new thread
                    clientIPAddress = "" + IPAddress.Parse(((IPEndPoint)tcpclient.Client.RemoteEndPoint).Address.ToString()); //Gets the Clients IP Address
                    clientthread.Start(tcpclient); //Start New Thread
                }
            }
            catch (Exception e)
            {
                string warning = e.Message;
                Console.WriteLine("ERROR: Starting Server");
            }
        }

        static string clientIPAddress = "0.0.0.0"; //Default Value

        private static void SingleClientThread(object tcpclient)
        {
            TcpClient client = (TcpClient)tcpclient; //Create a new client
            NetworkStream socketStream = null;
            try
            {
    
                socketStream = client.GetStream(); //Get a Network Stream from the client
                socketStream.WriteTimeout = 1000; //Times Out after a Second
                socketStream.ReadTimeout = 1000; //Times Out after a Second
                doRequest(socketStream, clientIPAddress); //Process Information
            }
            catch (Exception e)
            {
                string message = e.Message;
                Console.WriteLine(clientIPAddress + ": Client Timed Out"  ); //Write Out the Error
                
            }
            finally
            {
                socketStream.Close(); //Close Network Stream
                client.Close();//.Client.Close(); //Close Client
                Monitor.Enter(writexml); //Locks file so multiple threads aren't writing to the same file
                writexml.WriteXMLFile(); //Write Out the XML File
                Monitor.Exit(writexml); //Releases the method for other threads to use
            }
        }

        public static string GetPersonLocation(string Name)
        {
            foreach (Person p in personlist) //For Every Person in the list
            {
                if (Name.ToUpper() == p.GetName().ToUpper()) //look for the record
                {
                    return p.GetLocation(); //return the location
                }
            }
            return "none"; //could not find location
        }

        public static bool SetPersonLocation(string name, string location)
        {
            foreach (Person p in personlist)// for every person in the list
            {
                if (name.ToUpper() == p.GetName().ToUpper()) //find the record
                {
                    Person tempPerson = p; //store the record in a temporary person class
                    personlist.Remove(p); //remove the old one from the list
                    tempPerson.SetLocation(location); //update the location
                    personlist.Add(tempPerson); //put it back on the list
                     return true; //we updated the location
                }
            }
            return false; //we couldnt update the location
        }
        public  enum types { GET, SET, ADD, NONE };

        public static void doRequest(NetworkStream incoming, string IpAddress)
        {
            byte[] data = new byte[1024];
            int length = incoming.Read(data, 0, data.Length);
            if (length > 0)
            {
                string incomingtext = Encoding.ASCII.GetString(data, 0, length);
                                
                string[] Input = incomingtext.Split(' ');

                types type = types.NONE;
                string name = " ";
                byte[] outputdata = new byte[1024];

                if (Input.Length == 1) //If there is One Argument
                {
                    type = types.GET; //Performing a GET
                    name = Input[0]; 
                }
                else if (Input.Length >= 2) //If there are more than 2 arguments
                {
                    type = types.SET;
                    name = incomingtext;
                }


                if (type == types.SET)
                {
                    string[] tempstrings = name.Split(new Char[]{' '},2);
                    string Tlocation = tempstrings[1];
                    name = tempstrings[0];
                    int endlocation = Tlocation.IndexOf((char)13);
                    Tlocation = Tlocation.Substring(0, endlocation);


                        if (SetPersonLocation(name, Tlocation) == true)
                        {
                            Console.WriteLine("Client: request to change " + name + " to " + Tlocation);
                            outputdata = Encoding.ASCII.GetBytes("OK" + (char)13 + (char)10);
                            Console.WriteLine("Server: Replied with OK");
                            WriteToLog(IpAddress + " - - " + DateTime.Today + " '" + type + " " + name + " " + Tlocation + "' OK");
                        }
                        else
                        {
                            Person temp = new Person(name, Tlocation);
                            personlist.Add(temp);
                            outputdata = Encoding.ASCII.GetBytes("OK" + (char)13 + (char)10);
                            Console.WriteLine("Server: Replied with OK");
                            WriteToLog(IpAddress + " - - " + DateTime.Today + " '" + type + " " + name + " " + Tlocation + "' PERSON ADDED");
                        }
                    

                }
                else if (type == types.GET)
                {
                    int endlocation = name.IndexOf((char)13);
                    name = name.Substring(0, endlocation);
                    Console.WriteLine("Client: request for " + name);
                    Console.WriteLine("Server: replied in " + GetPersonLocation(name));
                    if (GetPersonLocation(name) != "none")
                    {
                        WriteToLog(IpAddress + " - - " + DateTime.Today + " '" + type + " " + name + "' OK");
                        outputdata = Encoding.ASCII.GetBytes(name + " is in " + GetPersonLocation(name) + (char)13 + (char)10);
                    }
                    else
                    {
                        outputdata = Encoding.ASCII.GetBytes("ERROR: no entries found" + (char)13 + (char)10);
                        Console.WriteLine("Server: Replied with ERROR: no entries found");
                        WriteToLog(IpAddress + " - - " + DateTime.Today + " '" + type + " " + name + "' ERROR : no entries found");
                    }
                }

                incoming.Write(outputdata, 0, outputdata.Length);
            }
               
        }

        private static void WriteToLog(string input)
        {
            try
            {
                StreamWriter logfile = new StreamWriter("Log.txt", true); //Write the file
                logfile.WriteLine(input); //Write a line of the log
                logfile.Close(); //Close the File
            }
            catch (Exception e)
            {
                string warning = e.Message;
                Console.WriteLine("Problems writing the log file");
            }
        }

        private static void ReadXMLFile()
        {
            try
            {
                personlist.Clear(); //Clear the List
                XmlDocument PeopleFile = new XmlDocument(); //Create New Document
                PeopleFile.Load("People.xml"); //Load File
                XmlNodeList nodelist = PeopleFile.GetElementsByTagName("Person"); //Get all  the data
                foreach (XmlNode node in nodelist) //For Each Person in the file
                {
                    string name = node["Name"].InnerText; //Get the Name
                    string loc = node["Location"].InnerText; //Get the location
                    Person temp = new Person(name, loc); //Create a Class
                    personlist.Add(temp); //Add to List

                }
            }
            catch (Exception e)
            {
                string warning = e.Message;
                Console.WriteLine("Could Not read XML file");
                writexml.WriteXMLFile();
            }
        }
    }
    class WriteFile : Program
    {
        public void WriteXMLFile()
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
                    foreach (Person node in personlist) //For Each Person
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
            catch (Exception e)
            {
                string warning = e.Message;
                Console.WriteLine("Could Not Write XML File");
            }
        }

        


    }
}
