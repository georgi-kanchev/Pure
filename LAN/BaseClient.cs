using System.Net.Sockets;
using System.Text;

namespace Pure.LAN
{
	public class BaseClient
	{
		public void Connect(string serverIP, int port)
		{
			client?.Dispose();
			client = new(this, serverIP, port);
			client.Connect();
		}
		public void Disconnect()
		{
			client.DisconnectAndStop();
			client.Disconnect();
			client = null;
		}

		public void SendMessage(string message)
		{
			client.SendAsync(message);
		}

		protected virtual void OnError(string error) { }
		protected virtual void OnMessageReceive(string clientIP, string message) { }
		protected virtual void OnClientConnect(string clientIP) { }
		protected virtual void OnClientDisconnect(string clientIP) { }

		private class _Client : TcpClient
		{
			public _Client(BaseClient parent, string address, int port) : base(address, port)
			{
				this.parent = parent;
			}

			public void DisconnectAndStop()
			{
				shouldDisconnect = true;
				DisconnectAsync();

				while (IsConnected)
					Thread.Yield();
			}

			protected override void OnConnected()
			{
				parent.OnClientConnect(ServerIP);
			}
			protected override void OnDisconnected()
			{
				parent.OnClientDisconnect(ServerIP);

				// Wait for a while...
				Thread.Sleep(1000);

				// Try to connect again
				if (shouldDisconnect == false)
					ConnectAsync();
			}
			protected override void OnReceived(byte[] buffer, long offset, long size)
			{
				var message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
				parent.OnMessageReceive(ServerIP, message);
			}
			protected override void OnError(SocketError error)
			{
				parent.OnError(error.ToString());
			}

			#region Backend
			private BaseClient parent;
			private bool shouldDisconnect;

			private string ServerIP => base.Id.ToString();
			#endregion
		}

		#region Backend
		private _Client client;
		#endregion
	}
}