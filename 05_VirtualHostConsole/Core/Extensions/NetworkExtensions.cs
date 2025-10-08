using System.Text;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Extensions
{
    public static class NetworkExtensions
    {
        public static async Task<string> ReadLineAsync(this NetworkStream stream)
        {
            var buffer = new List<byte>();
            var tempBuffer = new byte[1];

            while (true)
            {
                var bytesRead = await stream.ReadAsync(tempBuffer, 0, 1);
                if (bytesRead == 0)
                    break;

                var b = tempBuffer[0];
                if (b == '\n')
                    break;

                if (b != '\r')
                    buffer.Add(b);
            }

            return Encoding.ASCII.GetString(buffer.ToArray());
        }

        public static async Task WriteLineAsync(this NetworkStream stream, string line)
        {
            var data = Encoding.ASCII.GetBytes(line + "\r\n");
            await stream.WriteAsync(data, 0, data.Length);
        }

        public static async Task WriteAsync(this NetworkStream stream, string text)
        {
            var data = Encoding.ASCII.GetBytes(text);
            await stream.WriteAsync(data, 0, data.Length);
        }

        public static EndPoint RemoteEndPoint(this NetworkStream stream)
        {
            // This would need to be implemented based on the underlying stream
            return null;
        }
    }
}