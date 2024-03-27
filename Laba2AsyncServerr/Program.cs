﻿using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Laba2AsyncServerr
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPAddress adress = Dns.GetHostAddresses(Dns.GetHostName())[2];           
            int port = 1024;
            IPEndPoint point = new IPEndPoint(adress, port);

            Socket pass_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            pass_socket.Bind(point);
            pass_socket.Listen(10);
            Console.WriteLine($"Сервер начал просмотр на {port} порту");
            try
            {
                pass_socket.BeginAccept(AcceptCallFunc, pass_socket);
            }
            catch (SocketException ex)
            {

                Console.WriteLine(ex.Message);
            }

            void AcceptCallFunc(IAsyncResult ar)
            {
                Socket? socket = ar.AsyncState as Socket;
                Socket ns = socket!.EndAccept(ar);
                Console.WriteLine($"Клиент {ns.RemoteEndPoint} был подсодинен");
                byte[] buff = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                ns.BeginSend(buff, 0, buff.Length, SocketFlags.None, SendCallBack, ns);
                socket.BeginAccept(AcceptCallFunc, ns);
            }
            void SendCallBack(IAsyncResult ar)
            {
                Socket? ns = ar.AsyncState as Socket;
                int len = ns!.EndSend(ar);
                Console.WriteLine($"{len} байты отправляются в {ns.RemoteEndPoint}");
                ns.Shutdown(SocketShutdown.Both);
                ns.Close();
            }
            Console.ReadLine();
        }
    }   
}