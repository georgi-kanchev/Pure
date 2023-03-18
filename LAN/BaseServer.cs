using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Pure.LAN
{
	public class BaseServer
	{
		public void Start(int port)
		{
			server?.Dispose();
			server = new(this, IPAddress.Any, port);
			server.Start();
		}
		public void Restart() => server.Restart();
		public void Stop()
		{
			server.Stop();
			server.Dispose();
			server = null;
		}

		public void SendMessage(string message)
		{
			server.Multicast(message);
		}

		protected virtual void OnError(string error) { }
		protected virtual void OnMessageReceive(string clientIP, string message) { }
		protected virtual void OnClientDisconnect(string clientIP) { }
		protected virtual void OnClientConnect(string clientIP) { }

		#region Backend
		private class _Server : TcpServer
		{
			public _Server(BaseServer parent, IPAddress address, int port) : base(address, port)
			{
				this.parent = parent;
			}

			protected override TcpSession CreateSession() => new _Session(this);
			protected override void OnError(SocketError error) => parent.OnError(error.ToString());

			#region Backend
			internal BaseServer parent;
			#endregion
		}
		private class _Session : TcpSession
		{
			public _Session(_Server parent) : base(parent) => this.parent = parent;

			protected override void OnConnected()
			{
				parent.parent.OnClientConnect(ClientIP);
			}
			protected override void OnDisconnected()
			{
				parent.parent.OnClientDisconnect(ClientIP);
			}
			protected override void OnReceived(byte[] buffer, long offset, long size)
			{
				var message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
				parent.parent.OnMessageReceive(ClientIP, message);

			}
			protected override void OnError(SocketError error)
			{
				parent.parent.OnError(error.ToString());
			}

			#region Backend
			private _Server parent;

			private string ClientIP => base.Id.ToString();
			#endregion
		}

		private _Server server;
		#endregion
	}
}