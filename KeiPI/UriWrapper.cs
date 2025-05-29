using System.Runtime.CompilerServices;

namespace KeiPI
{
    public static class UriWrapper
    {
        public static string Curl(this Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                TaskAwaiter<string> waiter = client.GetStringAsync(uri).GetAwaiter();

                CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

                string content = "";
                Task.Run(() => content = waiter.GetResult()).Wait(cancellationToken);

                return content;
            }
        }
    }
}