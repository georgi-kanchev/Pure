// MIT License
// 
// Copyright (c) 2019 - 2023 Ivan Shynkarenka
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Pure.Engine.LocalAreaNetwork;

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// TCP session is used to read and write data from the connected TCP client
/// </summary>
/// <remarks>Thread-safe</remarks>
internal class TcpSession : IDisposable
{
    /// <summary>
    /// Initialize the session with a given server
    /// </summary>
    /// <param name="server">TCP server</param>
    public TcpSession(TcpServer server)
    {
        Id = Guid.NewGuid();
        Server = server;
        OptionReceiveBufferSize = server.OptionReceiveBufferSize;
        OptionSendBufferSize = server.OptionSendBufferSize;
    }

    /// <summary>
    /// Session Id
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Server
    /// </summary>
    public TcpServer Server { get; }
    /// <summary>
    /// Socket
    /// </summary>
    public Socket Socket { get; private set; }

    /// <summary>
    /// Number of bytes pending sent by the session
    /// </summary>
    public long BytesPending { get; private set; }
    /// <summary>
    /// Number of bytes sending by the session
    /// </summary>
    public long BytesSending { get; private set; }
    /// <summary>
    /// Number of bytes sent by the session
    /// </summary>
    public long BytesSent { get; private set; }
    /// <summary>
    /// Number of bytes received by the session
    /// </summary>
    public long BytesReceived { get; private set; }

    /// <summary>
    /// Option: receive buffer limit
    /// </summary>
    public int OptionReceiveBufferLimit { get; set; } = 0;
    /// <summary>
    /// Option: receive buffer size
    /// </summary>
    public int OptionReceiveBufferSize { get; set; }
    /// <summary>
    /// Option: send buffer limit
    /// </summary>
    public int OptionSendBufferLimit { get; set; } = 0;
    /// <summary>
    /// Option: send buffer size
    /// </summary>
    public int OptionSendBufferSize { get; set; }

    #region Connect/Disconnect session
    /// <summary>
    /// Is the session connected?
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Connect the session
    /// </summary>
    /// <param name="socket">Session socket</param>
    internal void Connect(Socket socket)
    {
        Socket = socket;

        // Update the session socket disposed flag
        IsSocketDisposed = false;

        // Setup buffers
        receiveBuffer = new Buffer();
        sendBufferMain = new Buffer();
        sendBufferFlush = new Buffer();

        // Setup event args
        receiveEventArg = new SocketAsyncEventArgs();
        receiveEventArg.Completed += OnAsyncCompleted;
        sendEventArg = new SocketAsyncEventArgs();
        sendEventArg.Completed += OnAsyncCompleted;

        // Apply the option: keep alive
        if (Server.OptionKeepAlive)
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        if (Server.OptionTcpKeepAliveTime >= 0)
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime,
                Server.OptionTcpKeepAliveTime);

        if (Server.OptionTcpKeepAliveInterval >= 0)
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval,
                Server.OptionTcpKeepAliveInterval);

        if (Server.OptionTcpKeepAliveRetryCount >= 0)
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount,
                Server.OptionTcpKeepAliveRetryCount);

        // Apply the option: no delay
        if (Server.OptionNoDelay)
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

        // Prepare receive & send buffers
        receiveBuffer.Reserve(OptionReceiveBufferSize);
        sendBufferMain.Reserve(OptionSendBufferSize);
        sendBufferFlush.Reserve(OptionSendBufferSize);

        // Reset statistic
        BytesPending = 0;
        BytesSending = 0;
        BytesSent = 0;
        BytesReceived = 0;

        // Call the session connecting handler
        OnConnecting();

        // Call the session connecting handler in the server
        Server.OnConnectingInternal(this);

        // Update the connected flag
        IsConnected = true;

        // Try to receive something from the client
        TryReceive();

        // Check the socket disposed state: in some rare cases it might be disconnected while receiving!
        if (IsSocketDisposed)
            return;

        // Call the session connected handler
        OnConnected();

        // Call the session connected handler in the server
        Server.OnConnectedInternal(this);

        // Call the empty send buffer handler
        if (sendBufferMain.IsEmpty)
            OnEmpty();
    }

    /// <summary>
    /// Disconnect the session
    /// </summary>
    /// <returns>'true' if the section was successfully disconnected, 'false' if the section is already disconnected</returns>
    public virtual bool Disconnect()
    {
        if (!IsConnected)
            return false;

        // Reset event args
        receiveEventArg.Completed -= OnAsyncCompleted;
        sendEventArg.Completed -= OnAsyncCompleted;

        // Call the session disconnecting handler
        OnDisconnecting();

        // Call the session disconnecting handler in the server
        Server.OnDisconnectingInternal(this);

        try
        {
            try
            {
                // Shutdown the socket associated with the client
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
            }

            // Close the session socket
            Socket.Close();

            // Dispose the session socket
            Socket.Dispose();

            // Dispose event arguments
            receiveEventArg.Dispose();
            sendEventArg.Dispose();

            // Update the session socket disposed flag
            IsSocketDisposed = true;
        }
        catch (ObjectDisposedException)
        {
        }

        // Update the connected flag
        IsConnected = false;

        // Update sending/receiving flags
        receiving = false;
        sending = false;

        // Clear send/receive buffers
        ClearBuffers();

        // Call the session disconnected handler
        OnDisconnected();

        // Call the session disconnected handler in the server
        Server.OnDisconnectedInternal(this);

        // Unregister session
        Server.UnregisterSession(Id);

        return true;
    }
    #endregion

    #region Send/Recieve data
    // Receive buffer
    private bool receiving;
    private Buffer receiveBuffer;
    private SocketAsyncEventArgs receiveEventArg;
    // Send buffer
    private readonly object sendLock = new();
    private bool sending;
    private Buffer sendBufferMain;
    private Buffer sendBufferFlush;
    private SocketAsyncEventArgs sendEventArg;
    private long sendBufferFlushOffset;

    /// <summary>
    /// Send data to the client (synchronous)
    /// </summary>
    /// <param name="buffer">Buffer to send</param>
    /// <returns>Size of sent data</returns>
    protected virtual long Send(byte[] buffer)
    {
        return Send(buffer.AsSpan());
    }

    /// <summary>
    /// Send data to the client (synchronous)
    /// </summary>
    /// <param name="buffer">Buffer to send</param>
    /// <param name="offset">Buffer offset</param>
    /// <param name="size">Buffer size</param>
    /// <returns>Size of sent data</returns>
    public virtual long Send(byte[] buffer, long offset, long size)
    {
        return Send(buffer.AsSpan((int)offset, (int)size));
    }

    /// <summary>
    /// Send data to the client (synchronous)
    /// </summary>
    /// <param name="buffer">Buffer to send as a span of bytes</param>
    /// <returns>Size of sent data</returns>
    protected virtual long Send(ReadOnlySpan<byte> buffer)
    {
        if (!IsConnected)
            return 0;

        if (buffer.IsEmpty)
            return 0;

        // Sent data to the client
        long sent = Socket.Send(buffer, SocketFlags.None, out var ec);
        if (sent > 0)
        {
            // Update statistic
            BytesSent += sent;
            Interlocked.Add(ref Server.bytesSent, sent);

            // Call the buffer sent handler
            OnSent(sent, BytesPending + BytesSending);
        }

        // Check for socket error
        if (ec != SocketError.Success)
        {
            SendError(ec);
            Disconnect();
        }

        return sent;
    }

    /// <summary>
    /// Send text to the client (synchronous)
    /// </summary>
    /// <param name="text">Text string to send</param>
    /// <returns>Size of sent data</returns>
    public virtual long Send(string text)
    {
        return Send(Encoding.UTF8.GetBytes(text));
    }

    /// <summary>
    /// Send text to the client (synchronous)
    /// </summary>
    /// <param name="text">Text to send as a span of characters</param>
    /// <returns>Size of sent data</returns>
    public virtual long Send(ReadOnlySpan<char> text)
    {
        return Send(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    /// <summary>
    /// Send data to the client (asynchronous)
    /// </summary>
    /// <param name="buffer">Buffer to send</param>
    /// <returns>'true' if the data was successfully sent, 'false' if the session is not connected</returns>
    protected virtual bool SendAsync(byte[] buffer)
    {
        return SendAsync(buffer.AsSpan());
    }

    /// <summary>
    /// Send data to the client (asynchronous)
    /// </summary>
    /// <param name="buffer">Buffer to send</param>
    /// <param name="offset">Buffer offset</param>
    /// <param name="size">Buffer size</param>
    /// <returns>'true' if the data was successfully sent, 'false' if the session is not connected</returns>
    public virtual bool SendAsync(byte[] buffer, long offset, long size)
    {
        return SendAsync(buffer.AsSpan((int)offset, (int)size));
    }

    /// <summary>
    /// Send data to the client (asynchronous)
    /// </summary>
    /// <param name="buffer">Buffer to send as a span of bytes</param>
    /// <returns>'true' if the data was successfully sent, 'false' if the session is not connected</returns>
    public virtual bool SendAsync(ReadOnlySpan<byte> buffer)
    {
        if (!IsConnected)
            return false;

        if (buffer.IsEmpty)
            return true;

        lock (sendLock)
        {
            // Check the send buffer limit
            if (sendBufferMain.Size + buffer.Length > OptionSendBufferLimit &&
                OptionSendBufferLimit > 0)
            {
                SendError(SocketError.NoBufferSpaceAvailable);
                return false;
            }

            // Fill the main send buffer
            sendBufferMain.Append(buffer);

            // Update statistic
            BytesPending = sendBufferMain.Size;

            // Avoid multiple send handlers
            if (sending)
                return true;
            sending = true;

            // Try to send the main buffer
            TrySend();
        }

        return true;
    }

    /// <summary>
    /// Send text to the client (asynchronous)
    /// </summary>
    /// <param name="text">Text string to send</param>
    /// <returns>'true' if the text was successfully sent, 'false' if the session is not connected</returns>
    public virtual bool SendAsync(string text)
    {
        return SendAsync(Encoding.UTF8.GetBytes(text));
    }

    /// <summary>
    /// Send text to the client (asynchronous)
    /// </summary>
    /// <param name="text">Text to send as a span of characters</param>
    /// <returns>'true' if the text was successfully sent, 'false' if the session is not connected</returns>
    public virtual bool SendAsync(ReadOnlySpan<char> text)
    {
        return SendAsync(Encoding.UTF8.GetBytes(text.ToArray()));
    }

    /// <summary>
    /// Receive data from the client (synchronous)
    /// </summary>
    /// <param name="buffer">Buffer to receive</param>
    /// <returns>Size of received data</returns>
    protected virtual long Receive(byte[] buffer)
    {
        return Receive(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Receive data from the client (synchronous)
    /// </summary>
    /// <param name="buffer">Buffer to receive</param>
    /// <param name="offset">Buffer offset</param>
    /// <param name="size">Buffer size</param>
    /// <returns>Size of received data</returns>
    protected virtual long Receive(byte[] buffer, long offset, long size)
    {
        if (!IsConnected)
            return 0;

        if (size == 0)
            return 0;

        // Receive data from the client
        long received = Socket.Receive(buffer, (int)offset, (int)size, SocketFlags.None, out var ec);
        if (received > 0)
        {
            // Update statistic
            BytesReceived += received;
            Interlocked.Add(ref Server.bytesReceived, received);

            // Call the buffer received handler
            OnReceived(buffer, 0, received);
        }

        // Check for socket error
        if (ec != SocketError.Success)
        {
            SendError(ec);
            Disconnect();
        }

        return received;
    }

    /// <summary>
    /// Receive text from the client (synchronous)
    /// </summary>
    /// <param name="size">Text size to receive</param>
    /// <returns>Received text</returns>
    public virtual string Receive(long size)
    {
        var buffer = new byte[size];
        var length = Receive(buffer);
        return Encoding.UTF8.GetString(buffer, 0, (int)length);
    }

    /// <summary>
    /// Receive data from the client (asynchronous)
    /// </summary>
    public virtual void ReceiveAsync()
    {
        // Try to receive data from the client
        TryReceive();
    }

    /// <summary>
    /// Try to receive new data
    /// </summary>
    private void TryReceive()
    {
        if (receiving)
            return;

        if (!IsConnected)
            return;

        var process = true;

        while (process)
        {
            process = false;

            try
            {
                // Async receive with the receive handler
                receiving = true;
                receiveEventArg.SetBuffer(receiveBuffer.Data, 0, (int)receiveBuffer.Capacity);
                if (!Socket.ReceiveAsync(receiveEventArg))
                    process = ProcessReceive(receiveEventArg);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    /// <summary>
    /// Try to send pending data
    /// </summary>
    private void TrySend()
    {
        if (!IsConnected)
            return;

        var empty = false;
        var process = true;

        while (process)
        {
            process = false;

            lock (sendLock)
                // Is previous socket send in progress?
                if (sendBufferFlush.IsEmpty)
                {
                    // Swap flush and main buffers
                    sendBufferFlush = Interlocked.Exchange(ref sendBufferMain, sendBufferFlush);
                    sendBufferFlushOffset = 0;

                    // Update statistic
                    BytesPending = 0;
                    BytesSending += sendBufferFlush.Size;

                    // Check if the flush buffer is empty
                    if (sendBufferFlush.IsEmpty)
                    {
                        // Need to call empty send buffer handler
                        empty = true;

                        // End sending process
                        sending = false;
                    }
                }
                else
                    return;

            // Call the empty send buffer handler
            if (empty)
            {
                OnEmpty();
                return;
            }

            try
            {
                // Async write with the write handler
                sendEventArg.SetBuffer(sendBufferFlush.Data, (int)sendBufferFlushOffset,
                    (int)(sendBufferFlush.Size - sendBufferFlushOffset));
                if (!Socket.SendAsync(sendEventArg))
                    process = ProcessSend(sendEventArg);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    /// <summary>
    /// Clear send/receive buffers
    /// </summary>
    private void ClearBuffers()
    {
        lock (sendLock)
        {
            // Clear send buffers
            sendBufferMain.Clear();
            sendBufferFlush.Clear();
            sendBufferFlushOffset = 0;

            // Update statistic
            BytesPending = 0;
            BytesSending = 0;
        }
    }
    #endregion

    #region IO processing
    /// <summary>
    /// This method is called whenever a receive or send operation is completed on a socket
    /// </summary>
    private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (IsSocketDisposed)
            return;

        // Determine which type of operation just completed and call the associated handler
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Receive:
                if (ProcessReceive(e))
                    TryReceive();
                break;
            case SocketAsyncOperation.Send:
                if (ProcessSend(e))
                    TrySend();
                break;
            default:
                throw new ArgumentException(
                    "The last operation completed on the socket was not a receive or send");
        }
    }

    /// <summary>
    /// This method is invoked when an asynchronous receive operation completes
    /// </summary>
    private bool ProcessReceive(SocketAsyncEventArgs e)
    {
        if (!IsConnected)
            return false;

        long size = e.BytesTransferred;

        // Received some data from the client
        if (size > 0)
        {
            if (BytesReceived + size > OptionReceiveBufferSize)
                return false;

            // Update statistic
            BytesReceived += size;

            Interlocked.Add(ref Server.bytesReceived, size);

            // Call the buffer received handler
            OnReceived(receiveBuffer.Data, 0, size);

            // If the receive buffer is full increase its size
            if (receiveBuffer.Capacity == size)
            {
                // Check the receive buffer limit
                if (2 * size > OptionReceiveBufferLimit && OptionReceiveBufferLimit > 0)
                {
                    SendError(SocketError.NoBufferSpaceAvailable);
                    Disconnect();
                    return false;
                }

                receiveBuffer.Reserve(2 * size);
            }
        }

        receiving = false;

        // Try to receive again if the session is valid
        if (e.SocketError == SocketError.Success)
        {
            // If zero is returned from a read operation, the remote end has closed the connection
            if (size > 0)
                return true;
            Disconnect();
        }
        else
        {
            SendError(e.SocketError);
            Disconnect();
        }

        return false;
    }

    /// <summary>
    /// This method is invoked when an asynchronous send operation completes
    /// </summary>
    private bool ProcessSend(SocketAsyncEventArgs e)
    {
        if (!IsConnected)
            return false;

        long size = e.BytesTransferred;

        // Send some data to the client
        if (size > 0)
        {
            // Update statistic
            BytesSending -= size;
            BytesSent += size;
            Interlocked.Add(ref Server.bytesSent, size);

            // Increase the flush buffer offset
            sendBufferFlushOffset += size;

            // Successfully send the whole flush buffer
            if (sendBufferFlushOffset == sendBufferFlush.Size)
            {
                // Clear the flush buffer
                sendBufferFlush.Clear();
                sendBufferFlushOffset = 0;
            }

            // Call the buffer sent handler
            OnSent(size, BytesPending + BytesSending);
        }

        // Try to send again if the session is valid
        if (e.SocketError == SocketError.Success)
            return true;
        SendError(e.SocketError);
        Disconnect();
        return false;
    }
    #endregion

    #region Session handlers
    /// <summary>
    /// Handle client connecting notification
    /// </summary>
    protected virtual void OnConnecting()
    {
    }
    /// <summary>
    /// Handle client connected notification
    /// </summary>
    protected virtual void OnConnected()
    {
    }
    /// <summary>
    /// Handle client disconnecting notification
    /// </summary>
    protected virtual void OnDisconnecting()
    {
    }
    /// <summary>
    /// Handle client disconnected notification
    /// </summary>
    protected virtual void OnDisconnected()
    {
    }

    /// <summary>
    /// Handle buffer received notification
    /// </summary>
    /// <param name="buffer">Received buffer</param>
    /// <param name="offset">Received buffer offset</param>
    /// <param name="size">Received buffer size</param>
    /// <remarks>
    /// Notification is called when another chunk of buffer was received from the client
    /// </remarks>
    protected virtual void OnReceived(byte[] buffer, long offset, long size)
    {
    }
    /// <summary>
    /// Handle buffer sent notification
    /// </summary>
    /// <param name="sent">Size of sent buffer</param>
    /// <param name="pending">Size of pending buffer</param>
    /// <remarks>
    /// Notification is called when another chunk of buffer was sent to the client.
    /// This handler could be used to send another buffer to the client for instance when the pending size is zero.
    /// </remarks>
    protected virtual void OnSent(long sent, long pending)
    {
    }

    /// <summary>
    /// Handle empty send buffer notification
    /// </summary>
    /// <remarks>
    /// Notification is called when the send buffer is empty and ready for a new data to send.
    /// This handler could be used to send another buffer to the client.
    /// </remarks>
    protected virtual void OnEmpty()
    {
    }

    /// <summary>
    /// Handle error notification
    /// </summary>
    /// <param name="error">Socket error code</param>
    protected virtual void OnError(SocketError error)
    {
    }
    #endregion

    #region Error handling
    /// <summary>
    /// Send error notification
    /// </summary>
    /// <param name="error">Socket error code</param>
    private void SendError(SocketError error)
    {
        // Skip disconnect errors
        if (error == SocketError.ConnectionAborted ||
            error == SocketError.ConnectionRefused ||
            error == SocketError.ConnectionReset ||
            error == SocketError.OperationAborted ||
            error == SocketError.Shutdown)
            return;

        OnError(error);
    }
    #endregion

    #region IDisposable implementation
    /// <summary>
    /// Disposed flag
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Session socket disposed flag
    /// </summary>
    public bool IsSocketDisposed { get; private set; } = true;

    // Implement IDisposable.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposingManagedResources)
    {
        // The idea here is that Dispose(Boolean) knows whether it is
        // being called to do explicit cleanup (the Boolean is true)
        // versus being called due to a garbage collection (the Boolean
        // is false). This distinction is useful because, when being
        // disposed explicitly, the Dispose(Boolean) method can safely
        // execute code using reference type fields that refer to other
        // objects knowing for sure that these other objects have not been
        // finalized or disposed of yet. When the Boolean is false,
        // the Dispose(Boolean) method should not execute code that
        // refer to reference type fields because those objects may
        // have already been finalized."

        if (!IsDisposed)
        {
            if (disposingManagedResources)
                // Dispose managed resources here...
                Disconnect();

            // Dispose unmanaged resources here...

            // Set large fields to null here...

            // Mark as disposed.
            IsDisposed = true;
        }
    }
    #endregion
}