////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Debug
{
	public delegate void InspectableListAddedHandler(IInspectableList list, object instance);
	public delegate void InspectableListRemovedHandler(IInspectableList list, object instance);
	public delegate void InspectableListChangedHandler(IInspectableList list);

	public interface IInspectableList
	{
		event InspectableListAddedHandler inspectorAdded;
		event InspectableListRemovedHandler inspectorRemoved;
		event InspectableListChangedHandler inspectorChanged;

		/// <summary>
		/// Called when the inspector adds an element.
		/// </summary>
		void inspectorAdd(object instance);

		/// <summary>
		/// Called when the inspector removes an element.
		/// </summary>
		void inspectorRemove(object instance);

		/// <summary>
		/// Called when the inspector sets an element to a different value.
		/// </summary>
		void inspectorSet(int index);

		/// <summary>
		/// Whether add can be called from the inspector.
		/// </summary>
		bool canInspectorAdd
		{
			get;
			set;
		}

		/// <summary>
		/// Whether remove can be called from the inspector.
		/// </summary>
		bool canInspectorRemove
		{
			get;
			set;
		}
	}
}
