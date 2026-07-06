////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class AnimalSpawn
	{
		private ushort _animal;
		public ushort animal => _animal;

		public AnimalSpawn(ushort newAnimal)
		{
			_animal = newAnimal;
		}
	}
}