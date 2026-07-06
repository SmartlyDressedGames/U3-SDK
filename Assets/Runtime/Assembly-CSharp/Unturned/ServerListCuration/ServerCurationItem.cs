////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal abstract class ServerCurationItem
	{
		public event System.Action OnSortOrderChanged;

		/// <summary>
		/// Invoked when web item is first loaded or reloaded.
		/// </summary>
		public event System.Action OnDataChanged;

		protected bool _isActive;
		public bool IsActive
		{
			get => _isActive;
			set
			{
				if (_isActive != value)
				{
					_isActive = value;
					SaveActive();
					curation.MarkDirty();
				}
			}
		}

		public abstract string DisplayName
		{
			get;
		}
		public abstract string DisplayOrigin
		{
			get;
		}
		public abstract Texture2D Icon
		{
			get;
		}
		public abstract string IconUrl
		{
			get;
		}
		public string ErrorMessage
		{
			get;
			internal set;
		}
		public abstract bool IsDeletable
		{
			get;
		}
		public abstract int LatestBlockedServerCount
		{
			get;
		}

		public bool IsAtFrontOfList => _sortOrder == 0;
		public bool IsAtBackOfList => _sortOrder == curation.GetItems().Count - 1;

		private int _sortOrder = -1;
		public int SortOrder
		{
			get => _sortOrder;
			set
			{
				bool changed = (_sortOrder != -1 && _sortOrder != value);
				_sortOrder = value;
				if (changed)
				{
					OnSortOrderChanged?.Invoke();
				}
			}
		}

		public abstract void Reload();
		public abstract void Delete();
		public abstract List<ServerListCurationRule> GetRules();
		public abstract void ResetBlockedServerCounts();

		protected abstract void SaveActive();

		protected void InvokeDataChanged()
		{
			OnDataChanged?.TryInvoke("OnDataChanged");
		}

		public ServerCurationItem(ServerListCuration curation)
		{
			this.curation = curation;
		}

		protected ServerListCuration curation;
	}
}
