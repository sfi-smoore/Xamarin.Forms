﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Data;

namespace Xamarin.Forms.Platform.UWP
{
	// This is a facade we put between the "real" contents of the CarouselView and the 
	// UWP ListViewBase to fool the ListViewBase into scrolling through the same set of
	// items over and over again for a very long time; it's the best version of "looping" 
	// we can manage until we replace the ListViewBase with something more flexible.

	internal class LoopableCollectionView : ICollectionView
	{
		const int FakeCount = 655360; // 640k ought to be enough for anybody
		readonly ICollectionView _internal;

		public LoopableCollectionView(ICollectionView @internal)
		{
			_internal = @internal;
		}

		internal bool IsLoopingEnabled { get; set; }
		internal bool CenterMode { get; set; }

		// This is just a facade to fool the ListView; we very definitely will not be modifying it
		public bool IsReadOnly => true;

		public int IndexOf(object item)
		{
			if (IsLoopingEnabled && CenterMode)
			{
				// The renderer will hit this on initial setup to move the ListView into the 
				// "middle" of the giant set of totally fake items. That way moving forward and
				// backward in the list will appear to "loop" for a very long time

				var realIndex = _internal.IndexOf(item);

				var roughlyTheMiddle = FakeCount / 2;
				var middleOffset = roughlyTheMiddle % _internal.Count;
				var adjustedMiddle = roughlyTheMiddle - middleOffset + 1;
				return adjustedMiddle + realIndex;
			}

			return _internal.IndexOf(item);
		}

		public int Count
		{
			get
			{
				if (!IsLoopingEnabled)
				{
					return _internal.Count;
				}

				// Pretend that the ListView has vast number of virtual items in it so that it can 
				// scroll a long, long way in either direction to simulate looping
				return FakeCount;
			}
		}

		public object this[int index]
		{
			get
			{
				if (!IsLoopingEnabled)
				{
					return _internal[index];
				}

				return _internal[index % _internal.Count];
			}

			set
			{
				_internal[index] = value;
			}
		}

		// Everything after this is just deferring to the internal ICollectionView
		public bool MoveCurrentTo(object item)
		{
			return _internal.MoveCurrentTo(item);
		}

		public bool MoveCurrentToPosition(int index)
		{
			return _internal.MoveCurrentToPosition(index);
		}

		public bool MoveCurrentToFirst()
		{
			return _internal.MoveCurrentToFirst();
		}

		public bool MoveCurrentToLast()
		{
			return _internal.MoveCurrentToLast();
		}

		public bool MoveCurrentToNext()
		{
			return _internal.MoveCurrentToNext();
		}

		public bool MoveCurrentToPrevious()
		{
			return _internal.MoveCurrentToPrevious();
		}

		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
		{
			return _internal.LoadMoreItemsAsync(count);
		}

		public IObservableVector<object> CollectionGroups => _internal.CollectionGroups;

		public object CurrentItem => _internal.CurrentItem;

		public int CurrentPosition => _internal.CurrentPosition;

		public bool HasMoreItems => _internal.HasMoreItems;

		public bool IsCurrentAfterLast => _internal.IsCurrentAfterLast;

		public bool IsCurrentBeforeFirst => _internal.IsCurrentBeforeFirst;

		public void Insert(int index, object item)
		{
			_internal.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_internal.RemoveAt(index);
		}

		public void Add(object item)
		{
			_internal.Add(item);
		}

		public void Clear()
		{
			_internal.Clear();
		}

		public bool Contains(object item)
		{
			return _internal.Contains(item);
		}

		public void CopyTo(object[] array, int arrayIndex)
		{
			_internal.CopyTo(array, arrayIndex);
		}

		public bool Remove(object item)
		{
			return _internal.Remove(item);
		}



		public IEnumerator<object> GetEnumerator()
		{
			return _internal.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_internal).GetEnumerator();
		}

		EventRegistrationTokenTable<EventHandler<object>> _currentChangedTokenTable = null;

		event EventHandler<object> ICollectionView.CurrentChanged
		{
			add
			{
				return EventRegistrationTokenTable<EventHandler<object>>
					.GetOrCreateEventRegistrationTokenTable(ref _currentChangedTokenTable)
					.AddEventHandler(value);
			}
			remove
			{
				EventRegistrationTokenTable<EventHandler<object>>
					.GetOrCreateEventRegistrationTokenTable(ref _currentChangedTokenTable)
					.RemoveEventHandler(value);
			}
		}

		EventRegistrationTokenTable<CurrentChangingEventHandler> _currentChangingTokenTable = null;

		event CurrentChangingEventHandler ICollectionView.CurrentChanging
		{
			add
			{
				return EventRegistrationTokenTable<CurrentChangingEventHandler>
					.GetOrCreateEventRegistrationTokenTable(ref _currentChangingTokenTable)
					.AddEventHandler(value);
			}
			remove
			{
				EventRegistrationTokenTable<CurrentChangingEventHandler>
					.GetOrCreateEventRegistrationTokenTable(ref _currentChangingTokenTable)
					.RemoveEventHandler(value);
			}
		}

		EventRegistrationTokenTable<VectorChangedEventHandler<object>> _vectorChangedTokenTable = null;

		event VectorChangedEventHandler<object> IObservableVector<object>.VectorChanged
		{
			add
			{
				return EventRegistrationTokenTable<VectorChangedEventHandler<object>>
					.GetOrCreateEventRegistrationTokenTable(ref _vectorChangedTokenTable)
					.AddEventHandler(value);
			}
			remove
			{
				EventRegistrationTokenTable<VectorChangedEventHandler<object>>
					.GetOrCreateEventRegistrationTokenTable(ref _vectorChangedTokenTable)
					.RemoveEventHandler(value);
			}
		}
	}
}


