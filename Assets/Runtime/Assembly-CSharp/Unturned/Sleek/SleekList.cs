////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekList<T> : SleekWrapper where T : class
	{
		public int itemHeight;
		public int itemPadding;

		public delegate ISleekElement CreateElement(T item);
		public CreateElement onCreateElement;
		/// <summary>
		/// Allows pooling elements.
		/// If set, this is called rather than removing element from scroll view.
		/// </summary>
		public System.Action<ISleekElement> OnRemoveElement;

		/// <summary>
		/// Kind of hacky... Used by player list for group connections.
		/// </summary>
		public int IndexOfCreateElementItem
		{
			get;
			private set;
		}

		public int ElementCount => visibleEntries.Count;

		public ISleekElement GetElement(int index)
		{
			return visibleEntries[index].element;
		}

		public void ForEachElement<TElement>(System.Action<TElement> action) where TElement : ISleekElement
		{
			foreach (VisibleEntry entry in visibleEntries)
			{
				if (entry.element is TElement typedElement)
				{
					action.Invoke(typedElement);
				}
			}
		}

		public void SetData(List<T> data)
		{
			ValidateNotDestroyed();
			this.data = data;
			NotifyDataChanged();
		}

		public void NotifyDataChanged()
		{
			ValidateNotDestroyed();
			int totalHeight;
			if (data != null)
			{
				totalHeight = data.Count * itemHeight;
				if (data.Count > 1)
				{
					totalHeight += (data.Count - 1) * itemPadding;
				}
			}
			else
			{
				totalHeight = 0;
			}

			scrollView.ContentSizeOffset = new Vector2(0.0f, totalHeight);
			UpdateVisibleRange();
		}

		private void DestroyAllChildren()
		{
			if (OnRemoveElement != null)
			{
				foreach (VisibleEntry entry in visibleEntries)
				{
					OnRemoveElement.Invoke(entry.element);
				}
			}
			else
			{
				scrollView.RemoveAllChildren();
			}
		}

		public void ForceRebuildElements()
		{
			ValidateNotDestroyed();
			DestroyAllChildren();
			visibleEntries.Clear();
			NotifyDataChanged();
		}

		public override void OnUpdate()
		{
			if (data != null && data.Count > 0)
			{
				// Data may have been set before transform was initialized, or screen may have resized.
				int visibleItemsCount = CalculateVisibleItemsCount();
				if (oldVisibleItemsCount != visibleItemsCount)
				{
					oldVisibleItemsCount = visibleItemsCount;
					UpdateVisibleRange();
				}
			}
		}

		public SleekList() : base()
		{
			scrollView = Glazier.Get().CreateScrollView();
			scrollView.SizeScale_X = 1.0f;
			scrollView.SizeScale_Y = 1.0f;
			scrollView.ScaleContentToWidth = true;
			scrollView.OnNormalizedValueChanged += onValueChanged;
			AddChild(scrollView);
		}

		private int IndexOfItemWithinRange(T item, int minIndex, int maxIndex)
		{
			for (int itemIndex = minIndex; itemIndex <= maxIndex; ++itemIndex)
			{
				if (ReferenceEquals(data[itemIndex], item))
				{
					return itemIndex;
				}
			}

			return -1;
		}

		private bool HasElementForItem(T item)
		{
			foreach (VisibleEntry visibleEntry in visibleEntries)
			{
				if (ReferenceEquals(visibleEntry.item, item))
				{
					return true;
				}
			}

			return false;
		}

		private void UpdateVisibleRange(float normalizedValue)
		{
			if (data == null || data.Count == 0 || onCreateElement == null)
			{
				DestroyAllChildren();
				visibleEntries.Clear();
				return;
			}

			int visibleItemsCount = CalculateVisibleItemsCount();
			oldVisibleItemsCount = visibleItemsCount;
			int minIndex = Mathf.Max(0, Mathf.FloorToInt(normalizedValue * (data.Count - visibleItemsCount)));
			int maxIndex = Mathf.Min(data.Count - 1, minIndex + visibleItemsCount);

			for (int visibleEntryIndex = visibleEntries.Count - 1; visibleEntryIndex >= 0; --visibleEntryIndex)
			{
				VisibleEntry visibleEntry = visibleEntries[visibleEntryIndex];
				int itemIndex = IndexOfItemWithinRange(visibleEntry.item, minIndex, maxIndex);
				if (itemIndex == -1)
				{
					if (OnRemoveElement != null)
					{
						OnRemoveElement.Invoke(visibleEntry.element);
					}
					else
					{
						scrollView.RemoveChild(visibleEntry.element);
					}
					visibleEntries.RemoveAtFast(visibleEntryIndex);
				}
				else
				{
					visibleEntry.element.PositionOffset_Y = itemIndex * (itemHeight + itemPadding);
				}
			}

			for (int itemIndex = minIndex; itemIndex <= maxIndex; ++itemIndex)
			{
				T item = data[itemIndex];
				if (!HasElementForItem(item))
				{
					IndexOfCreateElementItem = itemIndex;
					ISleekElement element = onCreateElement.Invoke(item);
					element.SizeOffset_Y = itemHeight;
					element.SizeScale_X = 1.0f;
					element.PositionOffset_Y = itemIndex * (itemHeight + itemPadding);
					// Parent may already be this if pooled.
					if (element.Parent != this)
					{
						scrollView.AddChild(element);
					}
					visibleEntries.Add(new VisibleEntry(item, element));
				}
			}
		}

		private void UpdateVisibleRange()
		{
			UpdateVisibleRange(scrollView.NormalizedVerticalPosition);
		}

		private int CalculateVisibleItemsCount()
		{
			return data != null ? Mathf.CeilToInt(scrollView.NormalizedViewportHeight * data.Count) : 0;
		}

		private void onValueChanged(Vector2 value)
		{
			UpdateVisibleRange(value.y);
		}

		public ISleekScrollView scrollView
		{
			get;
			private set;
		}
		private List<T> data;
		private List<VisibleEntry> visibleEntries = new List<VisibleEntry>();
		private int oldVisibleItemsCount = 0;

		private struct VisibleEntry
		{
			public VisibleEntry(T item, ISleekElement element)
			{
				this.item = item;
				this.element = element;
			}

			public T item;
			public ISleekElement element;
		}
	}
}
