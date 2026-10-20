using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "TCP Client Console";
            Console.WriteLine("Client ise baslayir...");

            var client = new TcpClient();
            var ip = IPAddress.Parse("192.168.31.185");
            var port = 27001;

            var ep = new IPEndPoint(ip, port);

            try
            {
                client.Connect(ep);
                if (client.Connected)
                {
                    var stream = client.GetStream();
                    var bw = new BinaryWriter(stream, Encoding.UTF8);
                    var br = new BinaryReader(stream, Encoding.UTF8);

                    Console.Write("Istifadeci adini daxil edin: ");
                    string? username = Console.ReadLine();

                    Console.Write("Sifreni daxil edin: ");
                    string? password = Console.ReadLine();

                    bw.Write(username ?? "");
                    bw.Write(password ?? "");

                    string response = br.ReadString();
                    if (response.StartsWith("REJECTED"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(response);
                        Console.ResetColor();
                        Thread.Sleep(3000);
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Giris ugurludur! Cixis etmek ucun 'EXIT' yaza bilersen.");
                    Console.ResetColor();

                    var writer = Task.Run(() =>
                    {
                        while (true)
                        {
                            var text = Console.ReadLine();
                            bw.Write(text ?? "bos");
                            if (text?.ToUpper() == "EXIT")
                            {
                                break;
                            }
                        }
                    });

                    var reader = Task.Run(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                var result = br.ReadString();
                                if (!string.IsNullOrEmpty(result))
                                {
                                    Console.WriteLine($"Serverden mesaj: {result}");
                                }
                            }
                            catch
                            {
                                break;
                            }
                        }
                    });

                    Task.WaitAll(writer, reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Xeta bas verdi: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}