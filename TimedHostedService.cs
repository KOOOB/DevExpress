using DevExpress.Xpo;
using DITECH.Dashboard.Models.SCGDashboard;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KOB.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        public static string? ConnectionString { get; set; }

        private Timer? _timer = null;
        private int executionCount = 0;
        private static UnitOfWork? _uow;

        public TimedHostedService()
        {
            if (_uow == null)
                _uow = new UnitOfWork
                {
                    ConnectionString = ConnectionString
                };
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            Console.WriteLine($"Timed Hosted Service is working. Count: {count} - {DateTime.Now:dd/MM/yy HH:mm:ss}");

            if (DateTime.Now.Minute % 10 == 0) // Each 10 min
            {
                Console.WriteLine($" -- Tick: {DateTime.Now:dd/MM/yy HH:mm:ss}");
                DoSomthing();
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }







        public static void SendLogToLine(string message, string app)
        {
            var ipAddr = "Ip";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ipAddr = ip.ToString();

            var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
            var postData = $"message={app} - {ipAddr}\n\n{message}";
            var data = Encoding.UTF8.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.Headers.Add("Authorization", "Bearer BpyCYdZXBrUVshi3hSOb1coGkMxNgGix5pe2gqVGP5I");

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }


        public void DoSomthing()
        {
           SendLogToLine("{DateTime.Now:dd/MM/yy HH:mm:ss}");

        }

    }
}
