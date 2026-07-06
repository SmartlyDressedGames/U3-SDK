////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Debug
{
	public struct InspectableFilePath : IInspectablePath
	{
		public string absolutePath
		{
			get;
			set;
		}

		public string extension
		{
			get;
			private set;
		}

		public bool isValid => !string.IsNullOrEmpty(absolutePath);

		public override string ToString()
		{
			return absolutePath;
		}

		public InspectableFilePath(string newExtension)
		{
			absolutePath = string.Empty;
			extension = newExtension;
		}
	}
}
