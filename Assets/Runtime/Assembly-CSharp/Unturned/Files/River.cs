////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SDG.Unturned
{
	public class River
	{
		private static byte[] buffer = new byte[Block.BUFFER_SIZE];
		private int water;

		private string path;
		private Stream stream;
		private Block block;

		public string readString()
		{
			if (block != null)
			{
				return block.readString();
			}
			else
			{
				int length = readByte();
				if (length > 0)
				{
					stream.Read(buffer, 0, length);
					string value = Encoding.UTF8.GetString(buffer, 0, length);
					return value;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public bool readBoolean()
		{
			if (block != null)
			{
				return block.readBoolean();
			}
			else
			{
				return readByte() > 0;
			}
		}

		public byte readByte()
		{
			if (block != null)
			{
				return block.readByte();
			}
			else
			{
				// readByte can return -1 when the end of the stream is reached, in which case we clamp to zero.
				return MathfEx.ClampToByte(stream.ReadByte());
			}
		}

		public byte[] readBytes()
		{
			if (block != null)
			{
				return block.readByteArray();
			}
			else
			{
				byte[] values = new byte[readUInt16()];
				stream.Read(values, 0, values.Length);

				return values;
			}
		}

		public short readInt16()
		{
			if (block != null)
			{
				return block.readInt16();
			}
			else
			{
				stream.Read(buffer, 0, 2);
				return BitConverter.ToInt16(buffer, 0);
			}
		}

		public ushort readUInt16()
		{
			if (block != null)
			{
				return block.readUInt16();
			}
			else
			{
				stream.Read(buffer, 0, 2);
				return BitConverter.ToUInt16(buffer, 0);
			}
		}

		public int readInt32()
		{
			if (block != null)
			{
				return block.readInt32();
			}
			else
			{
				stream.Read(buffer, 0, 4);
				return BitConverter.ToInt32(buffer, 0);
			}
		}

		public uint readUInt32()
		{
			if (block != null)
			{
				return block.readUInt32();
			}
			else
			{
				stream.Read(buffer, 0, 4);
				return BitConverter.ToUInt32(buffer, 0);
			}
		}

		public float readSingle()
		{
			if (block != null)
			{
				return block.readSingle();
			}
			else
			{
				stream.Read(buffer, 0, 4);
				return BitConverter.ToSingle(buffer, 0);
			}
		}

		public long readInt64()
		{
			if (block != null)
			{
				return block.readInt64();
			}
			else
			{
				stream.Read(buffer, 0, 8);
				return BitConverter.ToInt64(buffer, 0);
			}
		}

		public ulong readUInt64()
		{
			if (block != null)
			{
				return block.readUInt64();
			}
			else
			{
				stream.Read(buffer, 0, 8);
				return BitConverter.ToUInt64(buffer, 0);
			}
		}

		public CSteamID readSteamID()
		{
			return new CSteamID(readUInt64());
		}

		public System.Guid readGUID()
		{
			if (block != null)
			{
				return block.readGUID();
			}
			else
			{
				GuidBuffer buffer = new GuidBuffer();
				buffer.Read(readBytes(), 0);
				return buffer.GUID;
			}
		}

		public Vector3 readSingleVector3()
		{
			return new Vector3(readSingle(), readSingle(), readSingle());
		}

		public Quaternion readSingleQuaternion()
		{
			return Quaternion.Euler(readSingle(), readSingle(), readSingle());
		}

		public Color readColor()
		{
			return new Color(readByte() / 255f, readByte() / 255f, readByte() / 255f);
		}

		public void writeString(string value)
		{
			if (block != null)
			{
				block.writeString(value);
			}
			else
			{
				// Nelson 2024-04-30: Looks like UTF8.GetBytes throws an exception if input string is null, so I'm going
				// through and ensuring we never pass null to it.
				if (value == null)
				{
					value = string.Empty;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(value);
				byte bytesCount = MathfEx.ClampToByte(bytes.Length);
				stream.WriteByte(bytesCount);
				stream.Write(bytes, 0, bytesCount);

				water += 1 + bytesCount;
			}
		}

		public void writeBoolean(bool value)
		{
			if (block != null)
			{
				block.writeBoolean(value);
			}
			else
			{
				stream.WriteByte(value ? (byte) 1 : (byte) 0);

				water++;
			}
		}

		public void writeByte(byte value)
		{
			if (block != null)
			{
				block.writeByte(value);
			}
			else
			{
				stream.WriteByte(value);

				water++;
			}
		}

		public void writeBytes(byte[] values)
		{
			if (block != null)
			{
				block.writeByteArray(values);
			}
			else
			{
				ushort valuesCount = MathfEx.ClampToUShort(values.Length);
				writeUInt16(valuesCount);
				stream.Write(values, 0, valuesCount);

				water += valuesCount;
			}
		}

		public void writeInt16(short value)
		{
			if (block != null)
			{
				block.writeInt16(value);
			}
			else
			{
				byte[] bytes = BitConverter.GetBytes(value);
				stream.Write(bytes, 0, 2);

				water += 2;
			}
		}

		public void writeUInt16(ushort value)
		{
			if (block != null)
			{
				block.writeUInt16(value);
			}
			else
			{
				byte[] bytes = BitConverter.GetBytes(value);
				stream.Write(bytes, 0, 2);

				water += 2;
			}
		}

		public void writeInt32(int value)
		{
			if (block != null)
			{
				block.writeInt32(value);
			}
			else
			{
				byte[] bytes = BitConverter.GetBytes(value);
				stream.Write(bytes, 0, 4);

				water += 4;
			}
		}

		public void writeUInt32(uint value)
		{
			if (block != null)
			{
				block.writeUInt32(value);
			}
			else
			{
				byte[] bytes = BitConverter.GetBytes(value);
				stream.Write(bytes, 0, 4);

				water += 4;
			}
		}

		public void writeSingle(float value)
		{
			if (block != null)
			{
				block.writeSingle(value);
			}
			else
			{
				byte[] bytes = BitConverter.GetBytes(value);
				stream.Write(bytes, 0, 4);

				water += 4;
			}
		}

		public void writeInt64(long value)
		{
			if (block != null)
			{
				block.writeInt64(value);
			}
			else
			{
				byte[] bytes = BitConverter.GetBytes(value);
				stream.Write(bytes, 0, 8);

				water += 8;
			}
		}

		public void writeUInt64(ulong value)
		{
			if (block != null)
			{
				block.writeUInt64(value);
			}
			else
			{
				byte[] bytes = BitConverter.GetBytes(value);
				stream.Write(bytes, 0, 8);

				water += 8;
			}
		}

		public void writeSteamID(CSteamID steamID)
		{
			writeUInt64(steamID.m_SteamID);
		}

		public void writeGUID(System.Guid GUID)
		{
			GuidBuffer buffer = new GuidBuffer(GUID);
			buffer.Write(GuidBuffer.GUID_BUFFER, 0);
			writeBytes(GuidBuffer.GUID_BUFFER);
		}

		public void writeSingleVector3(Vector3 value)
		{
			writeSingle(value.x);
			writeSingle(value.y);
			writeSingle(value.z);
		}

		public void writeSingleQuaternion(Quaternion value)
		{
			Vector3 angles = value.eulerAngles;

			writeSingle(angles.x);
			writeSingle(angles.y);
			writeSingle(angles.z);
		}

		public void writeColor(Color value)
		{
			writeByte((byte) (value.r * 255));
			writeByte((byte) (value.g * 255));
			writeByte((byte) (value.b * 255));
		}

		public byte[] getHash()
		{
			stream.Position = 0;
			return Hash.SHA1(stream);
		}

		public void closeRiver()
		{
			if (block != null)
			{
				ReadWrite.writeBlock(path, true, block);
			}
			else
			{
				if (water > 0)
				{
					//				if(step > 0)
					//				{
					//					stream.Write(buffer, 0, step);
					//					
					//					water += step;
					//					step = 0;
					//				}

					stream.SetLength(water);
				}

				stream.Flush();
				stream.Close();
				stream.Dispose();
			}
		}

		//		private void read(int size)
		//		{
		//			if(water == 0)
		//			{
		//				stream.Read(buffer, 0, buffer.Length);
		//
		//				water = -1;
		//				step = 0;
		//			}
		//			else if(step + size >= buffer.Length)
		//			{
		//				Buffer.BlockCopy(buffer, step, buffer, 0, buffer.Length - step);
		//				stream.Read(buffer, buffer.Length - step, step);
		//
		//				step = 0;
		//			}
		//		}
		//
		//		private void write(int size)
		//		{
		//			if(step + size >= buffer.Length)
		//			{
		//				stream.Write(buffer, 0, step);
		//
		//				water += step;
		//				step = 0;
		//			}
		//		}

		public River(string newPath)
		{
			path = ReadWrite.PATH + newPath;

			if (!Directory.Exists(Path.GetDirectoryName(path)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			//			step = 0;
			water = 0;
		}

		public River(string newPath, bool usePath)
		{
			path = newPath;

			if (usePath)
			{
				path = ReadWrite.PATH + path;
			}

			if (!Directory.Exists(Path.GetDirectoryName(path)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			//			step = 0;
			water = 0;
		}

		public River(string newPath, bool usePath, bool useCloud, bool isReading)
		{
			path = newPath;

			if (useCloud)
			{
				if (isReading)
				{
					block = ReadWrite.readBlock(path, useCloud, 0);
				}

				if (block == null)
				{
					block = new Block();
				}
			}
			else
			{
				if (usePath)
				{
					path = ReadWrite.PATH + path;
				}

				if (!Directory.Exists(Path.GetDirectoryName(path)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(path));
				}

				stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
				//			step = 0;
				water = 0;
			}
		}
	}
}
