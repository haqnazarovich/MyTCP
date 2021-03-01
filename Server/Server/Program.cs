using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static int port;
        static IPAddress serverIp;
        static Thread listenThread;
        static TcpListener tcpListener;
        static NetworkStream Stream;
        static Random random = new Random();
        static List<TcpClient> tcpClients = new List<TcpClient>();
		static int clientNumber;

        static void Main(string[] args)
        {
            Console.WriteLine("Введите ip-адрес сервера");
            serverIp = IPAddress.Parse(Console.ReadLine());
            Console.WriteLine("Введите номер порта для сервера");
            port = int.Parse(Console.ReadLine());
			Console.Clear();
			listenThread = new Thread(new ThreadStart(Listen));
			listenThread.Start();
		}

		static void Listen()
		{
			tcpListener = new TcpListener(serverIp, port);
			tcpListener.Start();
			Console.WriteLine("Сервер создан, ожидается подключение клиентов");
			while (true)
			{
				TcpClient tcpClient = tcpListener.AcceptTcpClient();
				if (tcpClient != null)
				{
					tcpClients.Add(tcpClient);
					clientNumber = clientNumber + 1;
					Thread clientThread = new Thread(new ThreadStart(Process));
					clientThread.Start();
				}
			}

		}

		static void Process()
		{
			Stream = tcpClients[clientNumber - 1].GetStream();
			Console.WriteLine("Клиент с номером потока {0} - подключился", clientNumber);
			bool firstTime = false;
			while (true)
			{
				string oldMessage = null;
				try
				{
					if (clientNumber <= 2)
					{
						string newMessage = GetMessage();

						if (oldMessage != newMessage && firstTime)
						{
							string attention = "Запрос обрабатывается!";
							SendMessage(attention);
						}
						else
						{
							firstTime = true;
							if (oldMessage == newMessage)
							{
								string attention = "Сервер не закончил обработку запроса!";
								SendMessage(attention);

							}
							else
							{
								string processingMessage = newMessage;
								oldMessage = newMessage;
								ProcessingMessageAsync(oldMessage);
							}
						}
					}
					else
					{
						GetMessage();
						SendMessage("Превышен лимит потоков, попробуйте позже");
					}
				}
				catch
				{
					clientNumber = clientNumber - 1;
					Console.WriteLine("Клиент с номером потока {0} - отключился", clientNumber);
					break;
				}
			}
		}

		static string GetMessage()
		{
			string message = null;
			do
			{
				byte[] data = new byte[256];
				int bytes = Stream.Read(data, 0, data.Length);
				message = Encoding.Unicode.GetString(data, 0, bytes);
			}
			while (Stream.DataAvailable);

			return message;
		}
		static void SendMessage(string message)
		{
			byte[] data = Encoding.Unicode.GetBytes(message);
			Stream.Write(data, 0, data.Length);
		}

		static void ProcessingMessageAsync(string message)
		{
			Thread.Sleep(5000);
			Console.WriteLine("Номер клиента {0}, " + DateTime.Now.ToShortTimeString() + ": " + message, clientNumber);
			string messageWithValue = message.ToString() + ": " + random.Next(80, 85)/10;
			SendMessage(messageWithValue);		
		}
	}
}