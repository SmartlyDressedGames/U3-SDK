////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Net.Sockets;

namespace SDG.NetTransport.SystemSockets
{
	/// <summary>
	/// Implements message boundaries on top of a TCP stream socket.
	/// </summary>
	internal class SocketMessageLayer
	{
		private static byte[] sizeBuffer = new byte[2];
		public void SendMessage(Socket socket, byte[] buffer, int size)
		{
			sizeBuffer[0] = (byte) ((size >> 8) & 0xFF);
			sizeBuffer[1] = (byte) (size & 0xFF);

			socket.Send(sizeBuffer);
			SocketError error;
			int sentSize = socket.Send(buffer, 0, size, default, out error);
			if (error != SocketError.Success)
			{
				TransportBase_SystemSockets.Log("Socket encountered error {0} when sending message to {1}", error, socket.RemoteEndPoint);
			}
			TransportBase_SystemSockets.Log("Socket sent {0} byte message to {1}", sentSize, socket.RemoteEndPoint);
		}

		private static byte[] internalBuffer = new byte[1200];
		public void ReceiveMessages(Socket socket)
		{
			if (socket.Available < 1)
				return;

			SocketError error;
			int receivedSize = socket.Receive(internalBuffer, 0, internalBuffer.Length, default, out error);
			if (error == SocketError.WouldBlock)
				return;

			if (error != SocketError.Success)
			{
				TransportBase_SystemSockets.Log("Socket encountered error {0} when receiving message from {1}", error, socket.RemoteEndPoint);
				return;
			}

			if (receivedSize < 1)
				return;

			int offset = 0;
			while (offset < receivedSize)
			{
				if (pendingMessage == null)
				{
					if (pendingMessageSizeParts < 2)
					{
						switch (pendingMessageSizeParts)
						{
							case 0:
							{
								pendingMessageTotalSize += internalBuffer[offset] << 8;
							} break;

							case 1:
							{
								pendingMessageTotalSize += internalBuffer[offset];
							} break;
						}

						++pendingMessageSizeParts;
						++offset;
					}
					else
					{
						pendingMessage = new byte[pendingMessageTotalSize];
						pendingMessageOffset = 0;
					}
				}
				else
				{
					int remainingReceivedSize = receivedSize - offset;
					int remainingMessageSize = pendingMessage.Length - pendingMessageOffset;
					if (remainingReceivedSize < remainingMessageSize)
					{
						System.Array.Copy(internalBuffer, offset, pendingMessage, pendingMessageOffset, remainingReceivedSize);
						pendingMessageOffset += remainingReceivedSize;
						offset += remainingReceivedSize;
					}
					else
					{
						System.Array.Copy(internalBuffer, offset, pendingMessage, pendingMessageOffset, remainingMessageSize);
						offset += remainingMessageSize;
						messageQueue.Enqueue(pendingMessage);
						TransportBase_SystemSockets.Log("Socket received {0} byte message from {1}", pendingMessage.Length, socket.RemoteEndPoint);
						pendingMessage = null;
						pendingMessageTotalSize = 0;
						pendingMessageSizeParts = 0;
					}
				}
			}
		}

		public bool DequeueMessage(out byte[] buffer)
		{
			if (messageQueue.Count > 0)
			{
				buffer = messageQueue.Dequeue();
				return true;
			}
			else
			{
				buffer = null;
				return false;
			}
		}

		private Queue<byte[]> messageQueue = new Queue<byte[]>();
		private byte[] pendingMessage;
		private int pendingMessageTotalSize = 0;
		private int pendingMessageSizeParts = 0;
		private int pendingMessageOffset;
	}
}
