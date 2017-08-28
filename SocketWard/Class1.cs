using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketWard
{
    public class Ward
    {
        private readonly Socket _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public async Task Open(EndPoint ep)
        {
            await _client.ConnectTaskAsync(ep);
        }
    }

    public static class Extensions
    {
        public static Task ConnectTaskAsync(this Socket socket, EndPoint endpoint)
        {            
            return Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endpoint, null);
        }

        public static Task<int> SendTaskAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags)
        {
            void NullOp(IAsyncResult i)
            {
            }

            return Task<int>.Factory.FromAsync(socket.BeginSend(buffer, offset, size, flags, NullOp, null),socket.EndSend);
            //return Task<int>.Factory.FromAsync(socket.BeginSend, socket.EndSend, buffer, offset, size, flags);
        }

        public static Task<int> ReceiveTaskAsync(this Socket socket, byte[] buffer, int offset, int count)
        {
            return Task.Factory.FromAsync<int>(
                socket.BeginReceive(buffer, offset, count, SocketFlags.None, null, socket),
                socket.EndReceive);
        }
    }
}
