////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public interface ISleekProxyImplementation : ISleekElement
	{
		public SleekWrapper GetWrapper();

		public T GetWrapper<T>() where T : SleekWrapper
		{
			return GetWrapper() as T;
		}
	}
}
