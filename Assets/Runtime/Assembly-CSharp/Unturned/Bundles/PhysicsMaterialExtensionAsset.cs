////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Adds custom data to base physics material asset.
	/// For example how a vanilla material should respond to custom laser guns.
	/// </summary>
	public class PhysicsMaterialExtensionAsset : PhysicsMaterialAssetBase
	{
		public AssetReference<PhysicsMaterialAsset> baseRef;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			baseRef = p.data.ParseStruct<AssetReference<PhysicsMaterialAsset>>("Base");

			PhysicMaterialCustomData.RegisterAsset(this);
		}
	}
}
