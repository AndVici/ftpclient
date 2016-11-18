using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace ftpclient
{
    class Program
    {
        static bool pasv = true;
        static bool debug = false;
        static Socket ftpC = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Port()
        {
            IPAddress local = IPAddress.Parse("127.0.0.1");
            TcpListener temp = new TcpListener(local, 3762);
            temp.Start();
            string com = local.ToString();
            com = com.Replace('.', ',');
            com = com + ",14,178";
            Console.WriteLine(com);
            byte[] send = Encoding.ASCII.GetBytes("PORT " +com+"\n");
            byte[] data = new byte[1024];
            int num = ftpC.Send(send);
            num = ftpC.Receive(data);
            com = Encoding.ASCII.GetString(data, 0, num);
            sock = temp.AcceptSocket();
            num = ftpC.Receive(data);
            com = Encoding.ASCII.GetString(data, 0, num);
        }

        static void Pasv()
        {
            byte[] command;
            int rin, port;
            char[] dilem = { ',', ' ', '(', ')' };
            byte[] data = new byte[1024];
            command = Encoding.ASCII.GetBytes("PASV\n");
            rin = ftpC.Send(command);
            rin = ftpC.Receive(data);
            string usrString = Encoding.ASCII.GetString(data, 0, rin);
            string[] array = usrString.Split(dilem);
            port = Convert.ToInt32(array[9]) * 256 + Convert.ToInt32(array[10]);
            IPAddress ad = ((IPEndPoint)ftpC.RemoteEndPoint).Address;
            sock.Connect(new IPEndPoint(ad, port));
            Console.WriteLine(usrString);
        }
        static void getF(string file)
        {
            byte[] data = new byte[1024];
            int num, size;
            byte [] command = Encoding.ASCII.GetBytes("SIZE " + file + "\n");
            num = ftpC.Send(command);
            num = ftpC.Receive(data);
            string usrString = Encoding.ASCII.GetString(data, 0, num);
            char[] ch = { ' ', '\r' };
            string[] array = usrString.Split(ch);
            usrString = array[1];
            size = Convert.ToInt32(usrString);
            if (size > 1024)
                data = new byte[size];
            if (pasv == true)
                Pasv();
            else
                Port();
            command = Encoding.ASCII.GetBytes("RETR " + file + "\n");
            num = ftpC.Send(command);
            num = ftpC.Receive(data);
            usrString = Encoding.ASCII.GetString(data, 0, num);
            Console.WriteLine(usrString);
            num = sock.Receive(data);
            usrString = Encoding.ASCII.GetString(data, 0, num);
            Console.WriteLine(usrString);
            sock.Close();
        }
        static void list()
        {
            byte[] data = new byte[1024];
            int num, size;
            byte[] command;//= Encoding.ASCII.GetBytes("MLST\n");
            //num = ftpC.Send(command);
            //num = ftpC.Receive(data);
            string usrString;// = Encoding.ASCII.GetString(data, 0, num);
            if (pasv == true)
                Pasv();
            else
                Port();
            command = Encoding.ASCII.GetBytes("LIST\n");
            num = ftpC.Send(command);
            num = ftpC.Receive(data);
            usrString = Encoding.ASCII.GetString(data, 0, num);
            num = sock.Receive(data);
            usrString = Encoding.ASCII.GetString(data, 0, num);
            Console.WriteLine(usrString);
            sock.Close();
        }

        static void login(string cmd)
        {
            byte[] data = new byte[1024];
            string xStr;
            int inS;
            byte[] outstream;
            Console.Write(cmd + ": ");
            xStr = Console.ReadLine();
            outstream = Encoding.ASCII.GetBytes(cmd + xStr  + "\n");
            int outS = ftpC.Send(outstream);
            inS = ftpC.Receive(data);
            xStr = Encoding.ASCII.GetString(data, 0, inS);
            Console.WriteLine(xStr);
            xStr = xStr.Substring(0, 3);
            if (xStr == "331")
            {
                cmd = "PASS ";
                login(cmd);
            }

            else if (xStr == "230" || xStr == "530")
             {
                 
             }
            else
            {
                Console.WriteLine("Invalid " + cmd);
                login(cmd);
            }
        }

        static void Main(string[] args)
        {
            string usrString, var;
            int rin, toServ;
            string[] commands = { "ascii", "binary", "cd", "cdup", "debug", "dir", "get", "help", "passive", "pwd", "quit", "user" };
            var = Console.ReadLine();
            IPHostEntry host = Dns.GetHostEntry(var);
            IPEndPoint ips = new IPEndPoint(host.AddressList[0], 21);
            ftpC.Connect(ips);
            byte[] data = new byte[1024];
            rin = ftpC.Receive(data);
            var = Encoding.ASCII.GetString(data, 0, rin);
            char[] sp = { '\r' };
            string[] array = var.Split(sp);
            foreach(string s in array)
                Console.Write(s);
            login("USER ");
            byte[] command;


            while ((true))
            {
                string arg = null;
                usrString = Console.ReadLine();
                array = usrString.Split(' ');
                usrString = array[0];
                if (array.Length > 1)
                    arg = array[1];
                if (usrString == "ascii")
                {
                    command = Encoding.ASCII.GetBytes("TYPE A\n");
                    toServ = ftpC.Send(command);
                    rin = ftpC.Receive(data);
                }
                else if (usrString == "binary")
                {
                    command = Encoding.ASCII.GetBytes("TYPE I\n");
                    toServ = ftpC.Send(command);
                    rin = ftpC.Receive(data);
                }
                else if (usrString == "cd")
                {
                    command = Encoding.ASCII.GetBytes("CD "+ arg +"\n");
                    toServ = ftpC.Send(command);
                    rin = ftpC.Receive(data);
                    usrString = Encoding.ASCII.GetString(data, 0, rin);
                    Console.WriteLine(usrString);
                }
                else if(usrString == "cdup")
                {
                    command = Encoding.ASCII.GetBytes("CDUP\n");
                    toServ = ftpC.Send(command);
                    rin = ftpC.Receive(data);
                }
                else if(usrString == "debug")
                {
                    if(debug == false)
                    {
                        debug = true;
                        Console.WriteLine("Debugging is on");
                    }
                    else
                    {
                        debug = false;
                        Console.WriteLine("Debugging is off");
                    }
                }
                else if (usrString == "dir")
                    list();

                else if (usrString == "get")
                {
                    
                    if (array.Length > 1)
                    {
                        getF(arg);
                    }

                }
                else if (usrString == "help")
                {
                    foreach (string s in commands)
                        Console.WriteLine(s);
                }
                else if (usrString == "passive")
                {
                    if(pasv == true)
                        pasv = false;
                    else
                        pasv = true;
                    

                }
                else if (usrString == "pwd")
                {
                    command = Encoding.ASCII.GetBytes("PWD\n");
                    toServ = ftpC.Send(command);

                    rin = ftpC.Receive(data);
                    usrString = Encoding.ASCII.GetString(data, 0, rin);
                    Console.WriteLine(usrString);
                }
                else if (usrString == "quit")
                {
                    command = Encoding.ASCII.GetBytes("QUIT\n");
                    toServ = ftpC.Send(command);
                    Console.WriteLine("bye bye");
                    break;
                }
                else if(usrString == "user")
                {
                    command = Encoding.ASCII.GetBytes("USER " +arg+"\n");
                    toServ = ftpC.Send(command);
                    rin = ftpC.Receive(data);
                    usrString = Encoding.ASCII.GetString(data, 0, rin);
                    Console.WriteLine(usrString);
                    if (usrString.Substring(0, 3) == "331")
                        login("PASS ");
                }
                else
                {
                    Console.WriteLine("Invalid output");
                }

            }
            
        }
    }
}
