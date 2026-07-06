////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using System;

namespace SDG.Unturned
{
	public interface ITypeReference
	{
		/// <summary>
		/// GUID of the asset this is referring to.
		/// </summary>

		string assemblyQualifiedName
		{
			get;
			set;
		}

		Type type
		{
			get;
		}

		bool isValid
		{
			get;
		}
	}

	public struct TypeReference<T> : ITypeReference, IFormattedFileReadable, IFormattedFileWritable, IDatParseable, IEquatable<TypeReference<T>>
	{
		public static TypeReference<T> invalid = new TypeReference<T>((string) null);

		public string assemblyQualifiedName
		{
			get;
			set;
		}

		public Type type => (string.IsNullOrEmpty(assemblyQualifiedName) || assemblyQualifiedName.IndexOfAny(SDG.Unturned.DatValue.INVALID_TYPE_CHARS) >= 0) ? null : Type.GetType(assemblyQualifiedName);

		/// <summary>
		/// Whether the type has been asigned. Note that this doesn't mean an asset with <see cref="assemblyQualifiedName"/> exists.
		/// </summary>
		public bool isValid => !string.IsNullOrEmpty(assemblyQualifiedName);

		/// <summary>
		/// True if resovling this type reference would get that type.
		/// </summary>
		public bool isReferenceTo(Type type)
		{
			return type != null && assemblyQualifiedName == type.FullName;
		}

		public bool TryParse(IDatNode node)
		{
			if (node is IDatValue value)
			{
				assemblyQualifiedName = value.Value;
				return true;
			}
			else if (node is IDatDictionary dictionary)
			{
				assemblyQualifiedName = dictionary.GetString("Type");
				return true;
			}
			else
			{
				return false;
			}
		}

		public void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();
			if (reader == null)
			{
				return;
			}

			assemblyQualifiedName = reader.readValue("Type");
			assemblyQualifiedName = KeyValueTableTypeRedirectorRegistry.chase(assemblyQualifiedName);
		}

		public void write(IFormattedFileWriter writer)
		{
			writer.beginObject();

			writer.writeValue("Type", assemblyQualifiedName);

			writer.endObject();
		}

		public static bool operator ==(TypeReference<T> a, TypeReference<T> b)
		{
			return a.assemblyQualifiedName == b.assemblyQualifiedName;
		}

		public static bool operator !=(TypeReference<T> a, TypeReference<T> b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return assemblyQualifiedName.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			TypeReference<T> other = (TypeReference<T>) obj;
			return assemblyQualifiedName == other.assemblyQualifiedName;
		}

		public override string ToString()
		{
			return assemblyQualifiedName;
		}

		public bool Equals(TypeReference<T> other)
		{
			return assemblyQualifiedName == other.assemblyQualifiedName;
		}

		public TypeReference(string assemblyQualifiedName)
		{
			this.assemblyQualifiedName = assemblyQualifiedName;
		}

		public TypeReference(ITypeReference typeReference)
		{
			assemblyQualifiedName = typeReference.assemblyQualifiedName;
		}
	}
}
