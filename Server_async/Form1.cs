using GoodLibrary;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.Json;
using System.Text;

namespace Server_async
{
    public partial class Form1 : Form
    {
        Thread thread;
        delegate void TextDeleg(string str);
        List<Good> goods = new List<Good>();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (thread == null)
            {
                thread = new Thread(ServerFunc);
                thread.IsBackground = true;
                thread.Start();
                this.Text = "Server is working";
            }
        }
        private void ServerFunc(object? obj)
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

                MessageBox.Show(ex.Message);
            }
        }
        async void AcceptCallFunc(IAsyncResult ar)
        {
            Socket? socket = ar.AsyncState as Socket;
            Socket ns = socket!.EndAccept(ar);
            textBox1.BeginInvoke(new TextDeleg(UpdateTextBox), $"Клиент {ns.RemoteEndPoint} был подсодинен");
            byte[] buff = Encoding.UTF8.GetBytes(textBox1.Text + "date:" + DateTime.Now.ToString());
            string data = JsonSerializer.Serialize<List<Good>>(goods);
            byte[] goodBuff = Encoding.UTF8.GetBytes(data);

            ns.BeginSend(goodBuff, 0, goodBuff.Length, SocketFlags.None, SendCallBack, ns);
            socket.BeginAccept(AcceptCallFunc, ns);
        }
        void SendCallBack(IAsyncResult ar)
        {
            Socket? ns = ar.AsyncState as Socket;
            int len = ns!.EndSend(ar);
            textBox1.BeginInvoke(new TextDeleg(UpdateTextBox), $"{len} байты отправляются в {ns.RemoteEndPoint}");
            ns.Shutdown(SocketShutdown.Both);
            ns.Close();
        }
        private void UpdateTextBox(string str)
        {
            StringBuilder sb = new StringBuilder(textBox1.Text);
            sb.Append(str);
            textBox1.Text = sb.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter("goods.json", false, Encoding.UTF8))
            {
                Good good = new Good { Name = "Cake", Price = 120, Producer = "Konti" };
                Good good1 = new Good { Name = "Chocolate", Price = 250, Producer = "Roshen" };
                goods.AddRange(new[] { good, good1 });
                string data = JsonSerializer.Serialize<List<Good>>(goods);
                writer.WriteLine(data);
                MessageBox.Show("Файл json был создан");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Good good = new Good { Name = "Cake", Price = 120, Producer = "Konti" };
            Good good1 = new Good { Name = "Chocolate", Price = 250, Producer = "Roshen" };
            Good good2 = new Good { Name = "Soda", Price = 90, Producer = "CocaCola" };
            Good good3 = new Good { Name = "Water", Price = 50, Producer = "Morshinska" };
            goods.AddRange(new[] { good, good1, good2, good3 });
        }
    }
}