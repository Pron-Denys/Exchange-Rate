namespace Program
{
    using System.Text;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Serialization.Json;
    using Message_Exchange_Rate;
    using Сurrencies;
    class Program
    {
        static void Main()
        {
            FileStream file = new("Exchange Rate.json", FileMode.Open);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, decimal>));
            Dictionary<string, decimal>? currencies = serializer.ReadObject(file) as Dictionary<string, decimal>;
            file.Close();
            Connect(currencies!);
            Console.ReadLine();
        }

        static async void Connect(Dictionary<string, decimal> currencies)
        {
            await Task.Run(async () =>
            {
                try
                {
                    UdpClient client = new UdpClient(49160);
                    UdpReceiveResult receive_result = await client.ReceiveAsync();
                    IPEndPoint end_point = receive_result.RemoteEndPoint;
                    byte[] bytes = receive_result.Buffer;
                    string? str = Encoding.UTF8.GetString(bytes);
                    if (str == "Connect")
                    {
                        Console.WriteLine($"Client: {end_point.Address} : {end_point.Port} connected {DateTime.Now}");
                        while (true)
                        {
                            receive_result = await client.ReceiveAsync();
                            bytes = receive_result.Buffer;
                            MemoryStream stream = new MemoryStream(bytes);
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Exchange_Rate));
                            Exchange_Rate? exchange_rate = serializer.ReadObject(stream) as Exchange_Rate;
                            if (exchange_rate != null)
                            {
                                decimal price_1 = 0;
                                decimal price_2 = 0;
                                decimal result = 0;
                                if (("UAN" == exchange_rate.currency_1.ToString()) && ("UAN" == exchange_rate.currency_2.ToString()))
                                    result = 1;
                                else
                                {
                                    foreach (var currency in currencies)
                                    {
                                        if ("UAN" == exchange_rate.currency_2.ToString())
                                        {
                                            if (currency.Key == exchange_rate.currency_1.ToString())
                                                result = currency.Value;
                                        }
                                        else
                                        {
                                            if (currency.Key == exchange_rate.currency_1.ToString())
                                                price_1 = currency.Value;
                                            if (currency.Key == exchange_rate.currency_2.ToString())
                                                price_2 = currency.Value;
                                            if ("UAN" == exchange_rate.currency_1.ToString())
                                                price_1 = 1;
                                        }
                                    }
                                }
                                if ("UAN" != exchange_rate.currency_2.ToString())
                                    result = price_1 / price_2;
                                string result_str = result.ToString();
                                if (result_str.IndexOf(",") != -1)
                                    if (result_str.Substring(result_str.IndexOf(".") + 1).Length > 5)
                                        result_str = result_str.Substring(0, result_str.IndexOf(".") + 10);
                                bytes = Encoding.Default.GetBytes(result_str);
                                await client.SendAsync(bytes, bytes.Length, end_point);
                            }
                            else
                            {
                                Console.WriteLine($"Client: {end_point.Address} : {end_point.Port} disconnected {DateTime.Now}");
                                client.Dispose();
                                break;
                            }
                        }
                        Connect(currencies);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
        }
    }
}