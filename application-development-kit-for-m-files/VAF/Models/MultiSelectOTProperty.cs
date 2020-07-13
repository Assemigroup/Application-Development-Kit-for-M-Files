//------------------------------------------------------------------------------------
//
// Manual changes to this file may cause unexpected behavior in your application.
//
//------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using MFiles.VAF.Common;
using MFilesAPI;

namespace VAF
{
	public class MultiSelectOTProperty<T> : IList<T>, ICollection<T>, ICollection, IEnumerable<T>, IEnumerable where T : IObjVerEx, new()
	{
		private List<T> _list;
		private PropertyValue _pv;

		public MultiSelectOTProperty()
		{
			_list = new List<T>();
		}
		/// <summary>
		/// Instantiate and initialize the OT property value
		/// </summary>
		public MultiSelectOTProperty(IObjVerEx ot, int property_id, IList<T> list_value = null)
		{
			_list = new List<T>();

			//get property value
			_pv = ot.objVerEx.GetProperty(property_id);
			if (_pv == null) {
				ot.objVerEx.SetProperty(property_id, MFDataType.MFDatatypeMultiSelectLookup, null);
				_pv = ot.objVerEx.GetProperty(property_id);
			}

			if (list_value != null) {
				//set value using list_value parameter
				this.Clear();
				this.AddRange(list_value);
			} else {
				//get the current value from OT object
				if (ot.objVerEx.HasValue(property_id)) {
					Lookups lookups = _pv.Value.GetValueAsLookups();
					foreach (Lookup lookup in lookups) {
						if (lookup.Deleted)
							continue;
						ObjVerEx objVEx = new ObjVerEx(ot.objVerEx.Vault, lookup.ObjectType, lookup.Item, lookup.Version);
						T item = new T() { objVerEx = objVEx };
						_list.Add(item);
					}
				}
			}
		}

		public static implicit operator MultiSelectOTProperty<T>(List<T> d)
		{
			MultiSelectOTProperty<T> t = new MultiSelectOTProperty<T>();
			t.AddRange(d);
			return t;
		}
		public static implicit operator List<T>(MultiSelectOTProperty<T> d)
		{
			return d.ToList();
		}

		public void Add(T item)
		{
			if (item == null || Contains(item))
				return;
			if (_pv != null)
				_pv.AddLookup(item.objVerEx.ID);
			_list.Add(item);
		}
		public void AddRange(IEnumerable<T> collection)
		{
			if (collection == null)
				return;
			foreach (T t in collection)
				Add(t);
		}
		public void Clear()
		{

			if (_pv != null)
				_pv.TypedValue.SetValueToNULL(MFDataType.MFDatatypeMultiSelectLookup);
			_list.Clear();
		}
		public bool Remove(T item)
		{
			if (item == null)
				return false;
			if (_pv != null)
				_pv.RemoveLookup(item.objVerEx.ID);
			return _list.Remove(item);
		}

		/// <summary>
		/// Removes the item found in the given index
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			Remove(_list.ElementAt(index));
		}
		/// <summary>
		/// Calls the Add. For implementation only
		/// </summary>
		public void Insert(int index, T item)
		{
			Add(item);
		}

		#region Interface implementation via _list member

		public int Count => ((IList<T>)_list).Count;
		public void ForEach(Action<T> action)
		{
			_list.ForEach(action);
		}
		public bool IsReadOnly => ((IList<T>)_list).IsReadOnly;
		public object SyncRoot => ((ICollection)_list).SyncRoot;
		public bool IsSynchronized => ((ICollection)_list).IsSynchronized;
		public T this[int index] { get => ((IList<T>)_list)[index]; set => ((IList<T>)_list)[index] = value; }
		public int IndexOf(T item)
		{
			return ((IList<T>)_list).IndexOf(item);
		}
		public bool Contains(T item)
		{
			return ((IList<T>)_list).Contains(item);
		}
		public IEnumerator<T> GetEnumerator()
		{
			return ((IList<T>)_list).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IList<T>)_list).GetEnumerator();
		}
		public void CopyTo(Array array, int index)
		{
			((ICollection)_list).CopyTo(array, index);
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
			((ICollection<T>)_list).CopyTo(array, arrayIndex);
		}
		#endregion
	}
}