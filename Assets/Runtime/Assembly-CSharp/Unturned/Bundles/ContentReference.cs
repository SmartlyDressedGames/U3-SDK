////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using System;

namespace SDG.Unturned
{
	public interface IContentReference
	{
		/// <summary>
		/// Name of the asset bundle.
		/// </summary>
		/// <example>core.content</example>

		string name
		{
			get;
			set;
		}

		/// <summary>
		/// Path within the asset bundle.
		/// </summary>

		string path
		{
			get;
			set;
		}

		bool isValid
		{
			get;
		}
	}

	public struct ContentReference<T> : IContentReference, IFormattedFileReadable, IFormattedFileWritable, IDatParseable, IEquatable<ContentReference<T>> where T : UnityEngine.Object
	{
		public static ContentReference<T> invalid = new ContentReference<T>(null, null);

		public string name
		{
			get;
			set;
		}

		public string path
		{
			get;
			set;
		}

		public bool isValid => !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(path);

		public bool TryParse(IDatNode node)
		{
			if (node is IDatValue value)
			{
				if (string.IsNullOrEmpty(value.Value))
				{
					return false;
				}

				if (value.Value.Length < 2)
				{
					// 2023-04-17: there seem to be a lot of copy-pasted material palette assets used
					// on curated maps with the same typo of missing a closing ']', so the final '}'
					// is getting parsed as a content reference. As a workaround we ignore 1-char strings.
					return false;
				}

				int delimiterIndex = value.Value.IndexOf(':');
				if (delimiterIndex < 0)
				{
					// Ideally we should have a warning if null. :(
					if (Assets.currentMasterBundle != null)
					{
						name = Assets.currentMasterBundle.assetBundleName;
					}
					path = value.Value;
				}
				else
				{
					name = value.Value.Substring(0, delimiterIndex);
					path = value.Value.Substring(delimiterIndex + 1);
				}
				return true;
			}
			else if (node is IDatDictionary dictionary)
			{
				name = dictionary.GetString("Name");
				path = dictionary.GetString("Path");
				return true;
			}
			else
			{
				return false;
			}
		}

		public void read(IFormattedFileReader reader)
		{
			IFormattedFileReader nestedReader = reader.readObject();
			if (nestedReader == null)
			{
				if (Assets.currentMasterBundle != null)
				{
					name = Assets.currentMasterBundle.assetBundleName;
				}
				path = reader.readValue();
				return;
			}

			name = nestedReader.readValue("Name");
			path = nestedReader.readValue("Path");
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.beginObject();

			writer.writeValue("Name", name);
			writer.writeValue("Path", path);

			writer.endObject();
		}

		public static bool operator ==(ContentReference<T> a, ContentReference<T> b)
		{
			return a.name == b.name && a.path == b.path;
		}

		public static bool operator !=(ContentReference<T> a, ContentReference<T> b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return name.GetHashCode() ^ path.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ContentReference<T> other = (ContentReference<T>) obj;
			return name == other.name && path == other.path;
		}

		public override string ToString()
		{
			return "#" + name + "::" + path;
		}

		public bool Equals(ContentReference<T> other)
		{
			return name == other.name && path == other.path;
		}

		public ContentReference(string newName, string newPath)
		{
			name = newName;
			path = newPath;
		}

		public ContentReference(IContentReference contentReference)
		{
			name = contentReference.name;
			path = contentReference.path;
		}
	}
}
