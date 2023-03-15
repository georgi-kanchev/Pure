// Copyright (c) 2018-2020, Yves Goergen, https://unclassified.software
//
// Copying and distribution of this file, with or without modification, are permitted provided the
// copyright notice and this notice are preserved. This file is offered as-is, without any warranty.

namespace Pure.LAN;

internal class ByteBuffer
{
	public int Count
	{
		get
		{
			lock (syncObj)
			{
				if (isEmpty)
				{
					return 0;
				}
				if (tail >= head)
				{
					return tail - head + 1;
				}
				return Capacity - head + tail + 1;
			}
		}
	}
	public byte[] Buffer
	{
		get
		{
			lock (syncObj)
			{
				byte[] bytes = new byte[Count];
				if (!isEmpty)
				{
					if (tail >= head)
					{
						Array.Copy(buffer, head, bytes, 0, tail - head + 1);
					}
					else
					{
						Array.Copy(buffer, head, bytes, 0, Capacity - head);
						Array.Copy(buffer, 0, bytes, Capacity - head, tail + 1);
					}
				}
				return bytes;
			}
		}
	}
	public int Capacity => buffer.Length;
	public bool AutoTrim { get; set; } = true;
	public int AutoTrimMinCapacity { get; set; } = DefaultCapacity;

	public ByteBuffer() { }
	public ByteBuffer(byte[] bytes)
	{
		Enqueue(bytes);
	}
	public ByteBuffer(int capacity)
	{
		AutoTrimMinCapacity = capacity;
		SetCapacity(capacity);
	}

	public void Clear()
	{
		lock (syncObj)
		{
			head = 0;
			tail = -1;
			isEmpty = true;
			Reset(ref availableTcs);
		}
	}
	public void SetCapacity(int capacity)
	{
		if (capacity < 0)
			throw new ArgumentOutOfRangeException(nameof(capacity), "The capacity must not be negative.");

		lock (syncObj)
		{
			int count = Count;
			if (capacity < count)
				throw new ArgumentOutOfRangeException(nameof(capacity), "The capacity is too small to hold the current buffer content.");

			if (capacity != buffer.Length)
			{
				byte[] newBuffer = new byte[capacity];
				Array.Copy(Buffer, newBuffer, count);
				buffer = newBuffer;
				head = 0;
				tail = count - 1;
			}
		}
	}
	public void TrimExcess()
	{
		lock (syncObj)
		{
			if (Count < Capacity * 0.9)
			{
				SetCapacity(Count);
			}
		}
	}

	public byte[] Peek(int maxCount)
	{
		return DequeueInternal(maxCount, peek: true);
	}

	public void Enqueue(byte[] bytes)
	{
		if (bytes == null)
			throw new ArgumentNullException(nameof(bytes));

		Enqueue(bytes, 0, bytes.Length);
	}
	public void Enqueue(ArraySegment<byte> segment)
	{
		if (segment.Array == null)
			return;

		Enqueue(segment.Array, segment.Offset, segment.Count);
	}
	public void Enqueue(byte[] bytes, int offset, int count)
	{
		if (bytes == null)
			throw new ArgumentNullException(nameof(bytes));
		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset));
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(offset));
		if (offset + count > bytes.Length)
			throw new ArgumentOutOfRangeException(nameof(count));

		if (count == 0)
			return;   // Nothing to do

		lock (syncObj)
		{
			if (Count + count > Capacity)
			{
				SetCapacity(Math.Max(Capacity * 2, Count + count));
			}

			int tailCount;
			int wrapCount;
			if (tail >= head || isEmpty)
			{
				tailCount = Math.Min(Capacity - 1 - tail, count);
				wrapCount = count - tailCount;
			}
			else
			{
				tailCount = Math.Min(head - 1 - tail, count);
				wrapCount = 0;
			}

			if (tailCount > 0)
			{
				Array.Copy(bytes, offset, buffer, tail + 1, tailCount);
			}
			if (wrapCount > 0)
			{
				Array.Copy(bytes, offset + tailCount, buffer, 0, wrapCount);
			}
			tail = (tail + count) % Capacity;
			isEmpty = false;
			Set(dequeueManualTcs);
			Set(availableTcs);
		}
	}
	public byte[] Dequeue(int maxCount)
	{
		return DequeueInternal(maxCount, peek: false);
	}
	public int Dequeue(byte[] buffer, int offset, int maxCount)
	{
		return DequeueInternal(buffer, offset, maxCount, peek: false);
	}

	public async Task<byte[]> DequeueAsync(int count, CancellationToken cancellationToken = default)
	{
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "The count must not be negative.");

		while (true)
		{
			TaskCompletionSource<bool> myDequeueManualTcs;
			lock (syncObj)
			{
				if (count <= Count)
				{
					return Dequeue(count);
				}
				myDequeueManualTcs = Reset(ref dequeueManualTcs);
			}
			await AwaitAsync(myDequeueManualTcs, cancellationToken);
		}
	}
	public async Task DequeueAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
	{
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "The count must not be negative.");
		if (buffer.Length < offset + count)
			throw new ArgumentException("The buffer is too small for the requested data.", nameof(buffer));

		while (true)
		{
			TaskCompletionSource<bool> myDequeueManualTcs;
			lock (syncObj)
			{
				if (count <= Count)
				{
					Dequeue(buffer, offset, count);
				}
				myDequeueManualTcs = Reset(ref dequeueManualTcs);
			}
			await AwaitAsync(myDequeueManualTcs, cancellationToken);
		}
	}
	public async Task WaitAsync(CancellationToken cancellationToken = default)
	{
		TaskCompletionSource<bool> myAvailableTcs;
		lock (syncObj)
		{
			if (Count > 0)
			{
				return;
			}
			myAvailableTcs = Reset(ref availableTcs);
		}
		await AwaitAsync(myAvailableTcs, cancellationToken);
	}

	#region Backend
	private const int DefaultCapacity = 1024;
	private readonly object syncObj = new object();
	private byte[] buffer = new byte[DefaultCapacity];
	private int head;
	private int tail = -1;
	private bool isEmpty = true;

	private TaskCompletionSource<bool> dequeueManualTcs =
		new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	private TaskCompletionSource<bool> availableTcs =
		new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	private byte[] DequeueInternal(int count, bool peek)
	{
		if (count > Count)
			count = Count;
		byte[] bytes = new byte[count];
		DequeueInternal(bytes, 0, count, peek);
		return bytes;
	}
	private int DequeueInternal(byte[] bytes, int offset, int count, bool peek)
	{
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "The count must not be negative.");
		if (count == 0)
			return count;   // Easy
		if (bytes.Length < offset + count)
			throw new ArgumentException("The buffer is too small for the requested data.", nameof(bytes));

		lock (syncObj)
		{
			if (count > Count)
				count = Count;

			if (tail >= head)
			{
				Array.Copy(buffer, head, bytes, offset, count);
			}
			else
			{
				if (count <= Capacity - head)
				{
					Array.Copy(buffer, head, bytes, offset, count);
				}
				else
				{
					int headCount = Capacity - head;
					Array.Copy(buffer, head, bytes, offset, headCount);
					int wrapCount = count - headCount;
					Array.Copy(buffer, 0, bytes, offset + headCount, wrapCount);
				}
			}
			if (!peek)
			{
				if (count == Count)
				{
					isEmpty = true;
					head = 0;
					tail = -1;
					Reset(ref availableTcs);
				}
				else
				{
					head = (head + count) % Capacity;
				}

				if (AutoTrim && Capacity > AutoTrimMinCapacity && Count <= Capacity / 2)
				{
					int newCapacity = Count;
					if (newCapacity < AutoTrimMinCapacity)
					{
						newCapacity = AutoTrimMinCapacity;
					}
					if (newCapacity < Capacity)
					{
						SetCapacity(newCapacity);
					}
				}
			}
			return count;
		}
	}

	private TaskCompletionSource<bool> Reset(ref TaskCompletionSource<bool> tcs)
	{
		if (tcs.Task.IsCompleted)
		{
			tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		}
		return tcs;
	}
	private void Set(TaskCompletionSource<bool> tcs)
	{
		tcs.TrySetResult(true);
	}
	private async Task AwaitAsync(TaskCompletionSource<bool> tcs, CancellationToken cancellationToken)
	{
		if (await Task.WhenAny(tcs.Task, Task.Delay(-1, cancellationToken)) == tcs.Task)
		{
			await tcs.Task;   // Already completed
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
	}
	#endregion
}