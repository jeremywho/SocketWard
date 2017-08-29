using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketWard
{
    public class SocketToMe
    {
        private const int MaxBufferSize = 2048;
        private readonly Socket _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Func<byte[], Task> Received { get; set; }

        public async Task Open(EndPoint ep)
        {
            await _client.ConnectTaskAsync(ep);
        }

        public async Task<int> Write(byte[] data)
        {
            return await _client.SendTaskAsync(data, 0, data.Length, SocketFlags.None);
        }

        public async Task<int> Write(byte[] data, int offset, int len)
        {
            return await _client.SendTaskAsync(data, offset, len, SocketFlags.None);
        }

        public async Task StartReading(WebSocket webSocket)
        {
            var buffer = new byte[MaxBufferSize];
            var bytesRead = -1;
            while (bytesRead != 0)
            {
                bytesRead = await _client.ReceiveTaskAsync(buffer, 0, MaxBufferSize);
                if (bytesRead == 0)
                {
                    // this means the connection closed
                    continue;
                }

                var readData = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, readData, 0, bytesRead);
                await Received(readData);
                //await webSocket.SendAsync(new ArraySegment<byte>(readData), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }

    // these extensions turn begin/end style callbacks into tasks that can be awaited
    public static class Extensions
    {
        public static Task ConnectTaskAsync(this Socket socket, EndPoint endpoint)
        {
            return Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endpoint, null);
        }

        public static Task<int> SendTaskAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags)
        {
            void NullOp(IAsyncResult i) { }

            return Task<int>.Factory.FromAsync(socket.BeginSend(buffer, offset, size, flags, NullOp, null) ?? throw new InvalidOperationException(), socket.EndSend);
        }

        public static Task<int> ReceiveTaskAsync(this Socket socket, byte[] buffer, int offset, int count)
        {
            return Task.Factory.FromAsync(
                socket.BeginReceive(buffer, offset, count, SocketFlags.None, null, socket) ?? throw new InvalidOperationException(),
                socket.EndReceive);
        }
    }
}
