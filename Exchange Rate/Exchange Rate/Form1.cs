namespace Exchange_Rate
{
    using Сurrencies;
    using Message_Exchange_Rate;
    using System.Text;
    using System.Runtime.Serialization.Json;
    using System.Net;
    using System.Net.Sockets;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement;

    public partial class Form1 : Form
    {
        UdpClient client;
        string currency_1;
        string currency_2;
        SynchronizationContext uiContext;
        public Form1()
        {
            InitializeComponent();
            client = null!;
            currency_1 = null!;
            currency_2 = null!;
            uiContext = SynchronizationContext.Current!;
            Сurrencies[] currencies_1 = { Сurrencies.USD, Сurrencies.EUR,
                Сurrencies.UAN, Сurrencies.GBP, Сurrencies.JPY };
            Сurrencies[] currencies_2 = { Сurrencies.USD, Сurrencies.EUR,
                Сurrencies.UAN, Сurrencies.GBP, Сurrencies.JPY };
            comboBox1.DataSource = currencies_1;
            comboBox2.DataSource = currencies_2;
        }

        private async void ConnectToServer()
        {
            await Task.Run(async () =>
            {
                try
                {
                    client = new UdpClient(textBox1.Text, 49160);
                    await client.SendAsync(Encoding.Default.GetBytes("Connect"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void Disconnect()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Exchange_Rate? exchange_rate = null;
                    MemoryStream stream = new MemoryStream();
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Exchange_Rate));
                    serializer.WriteObject(stream, exchange_rate);
                    byte[] bytes = stream.ToArray();
                    await client.SendAsync(bytes);
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void Click_ConnectToServer(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                ConnectToServer();
            }
            else
                MessageBox.Show("Заповніть поле", "", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
        }

        private async void Convert()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Exchange_Rate exchange_rate = new Exchange_Rate();
                    Сurrencies[] currencies = { Сurrencies.USD, Сurrencies.EUR,
                            Сurrencies.UAN, Сurrencies.GBP, Сurrencies.JPY };
                    foreach (Сurrencies currency in currencies)
                    {
                        if (currency.ToString() == currency_1)
                            exchange_rate.currency_1 = currency;
                        if (currency.ToString() == currency_2)
                            exchange_rate.currency_2 = currency;
                    }
                    MemoryStream stream = new MemoryStream();
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Exchange_Rate));
                    serializer.WriteObject(stream, exchange_rate);
                    byte[] bytes = stream.ToArray();
                    await client.SendAsync(bytes);
                    UdpReceiveResult receive_result = await client.ReceiveAsync();
                    string result = Encoding.Default.GetString(receive_result.Buffer);
                    uiContext.Send((parametr) => label2.Text = result, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void Click_Disconnect(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Click_Convert(object sender, EventArgs e)
        {
            currency_1 = comboBox1.Text;
            currency_2 = comboBox2.Text;
            Convert();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
        }
    }
}
