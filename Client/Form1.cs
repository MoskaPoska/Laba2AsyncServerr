using GoodLibrary;
using System.Text.Json;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Process.Start("Server_async.exe");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                IPAddress adress = Dns.GetHostAddresses(Dns.GetHostName())[2];
                int port = 1024;
                IPEndPoint point = new IPEndPoint(adress, port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                try
                {
                    socket.Connect(point);
                    if (socket.Connected)
                    {
                        string query = "GET\r\n\r\n";
                        string res;
                        byte[] mas_buff = Encoding.UTF8.GetBytes(query);
                        await socket.SendAsync(new ArraySegment<byte>(mas_buff), SocketFlags.None);
                        byte[] buff = new byte[1024];
                        StringBuilder stringBuilder = new StringBuilder();
                        int len;
                        do
                        {
                            len = await socket.ReceiveAsync(new ArraySegment<byte>(buff), SocketFlags.None);
                            res = Encoding.UTF8.GetString(buff, 0, len);
                            stringBuilder.Append(res);
                            textBox1.BeginInvoke(new Action<string>(UpdateTextBox), res);
                        } while (socket.Available > 0);
                        List<Good>? goods = JsonSerializer.Deserialize<List<Good>>(stringBuilder.ToString());
                        dataGridView1.BeginInvoke(new Action<List<Good>>(UpdateList), goods);
                    }
                }
                catch (SocketException ex)
                {

                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (StreamReader reader = new StreamReader("goods.json", Encoding.Default))
            {
                string? data = reader.ReadLine();
                List<Good>? goods = JsonSerializer.Deserialize<List<Good>>(data);
                dataGridView1.BeginInvoke(new Action<List<Good>>(UpdateList), goods);
            }
        }
        private void UpdateList(List<Good> list)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = list;
        }
        private void UpdateTextBox(string str)
        {
            StringBuilder sb = new StringBuilder(textBox1.Text);
            sb.Append(str);
            textBox1.Text = sb.ToString();
        }
    }
}