using System;
using System.Net;
using System.Windows.Forms;
using EventStore.ClientAPI;
using System.Text;

namespace ChatRoom
{
    public partial class Form1 : Form
    {

        private string _curChatRoom;
        private IEventStoreConnection _connection;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            textBox1.Text = ""; // Reset Again

            _connection =
                EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            // Don't forget to tell the connection to connect!
            _connection.ConnectAsync().Wait();
            _curChatRoom = "chat_stream";

            //var sub = _connection.SubscribeToStreamAsync(_curChatRoom, true,
            var sub = _connection.SubscribeToStreamFrom(_curChatRoom, StreamPosition.Start, true,
                (_, x) =>
                {
                    var data = Encoding.ASCII.GetString(x.Event.Data);
                    var mdata = Encoding.ASCII.GetString(x.Event.Metadata);
                    SetText(data, mdata);
                });

        }

        delegate void SetTextCallback(string text, string mtext);

        private void SetText(string text, string mtext)
        {
            if (textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Invoke(d, new object[] { text, mtext });
            }
            else
            {
                textBox1.Text += Environment.NewLine + mtext + " : " + text;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (uxUserName.Text == "")
            {
                MessageBox.Show("Please Provide User Name!");
                return;
            }

            if (_connection == null)
            {
                MessageBox.Show("Connection is Closed!");
                return;
            }

            PostMessage(textBox2.Text, uxUserName.Text);
            textBox2.Text = string.Empty;
        }

        private void PostMessage(string text, string fromUser)
        {
            var myEvent = new EventData(Guid.NewGuid(), "testEvent", false,
                                        Encoding.UTF8.GetBytes(text),
                                        Encoding.UTF8.GetBytes(fromUser));  // Not sure if this is the right use of it

            _connection.AppendToStreamAsync(_curChatRoom,
                                           ExpectedVersion.Any, myEvent).Wait();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart += textBox1.Text.Length;
        }


    }

}