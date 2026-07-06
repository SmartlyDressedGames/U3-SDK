////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// HUD with projected labels for teammates.
	/// </summary>
	internal class PlayerGroupUI : SleekWrapper
	{
		private List<ISleekLabel> _groups;
		public List<ISleekLabel> groups => _groups;

		private void addGroup(SteamPlayer player)
		{
			ISleekLabel group = Glazier.Get().CreateLabel();
			group.PositionOffset_X = -100;
			group.PositionOffset_Y = -15;
			group.SizeOffset_X = 200;
			group.SizeOffset_Y = 30;
			group.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			group.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
			AddChild(group);
			group.IsVisible = false;

			groups.Add(group);
		}

		private void onEnemyConnected(SteamPlayer player)
		{
			addGroup(player);
		}

		private void onEnemyDisconnected(SteamPlayer player)
		{
			for (int index = 0; index < Provider.clients.Count; index++)
			{
				if (Provider.clients[index] == player)
				{
					RemoveChild(groups[index]);
					groups.RemoveAt(index);
					break;
				}
			}
		}

		public override void OnDestroy()
		{
			Provider.onEnemyConnected -= onEnemyConnected;
			Provider.onEnemyDisconnected -= onEnemyDisconnected;
		}

		public PlayerGroupUI()
		{
			_groups = new List<ISleekLabel>();

			for (int index = 0; index < Provider.clients.Count; index++)
			{
				addGroup(Provider.clients[index]);
			}

			Provider.onEnemyConnected += onEnemyConnected;
			Provider.onEnemyDisconnected += onEnemyDisconnected;
		}
	}
}
