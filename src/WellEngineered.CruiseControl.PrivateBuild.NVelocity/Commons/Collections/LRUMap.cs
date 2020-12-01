// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Commons.Collections
{
	/// <summary>
	/// A keyed list with a fixed maximum size which removes
	/// the least recently used entry if an entry is added when full.
	/// </summary>
	[Serializable]
	public class LRUMap : ICollection, IDictionary, IEnumerable
	{
		private Hashtable objectTable = new Hashtable();
		private ArrayList objectList = new ArrayList();

		/// <summary>
		/// Default maximum size 
		/// </summary>
		protected internal const int DEFAULT_MAX_SIZE = 100;

		/// <summary>
		/// Maximum size 
		/// </summary>
		[NonSerialized] private int maxSize;

		public LRUMap() : this(DEFAULT_MAX_SIZE)
		{
		}

		public LRUMap(Int32 maxSize)
		{
			this.maxSize = maxSize;
		}

		public virtual void Add(object key, object value)
		{
			if (this.objectList.Count == this.maxSize)
			{
				this.RemoveLRU();
			}

			this.objectTable.Add(key, value);
			this.objectList.Insert(0, new DictionaryEntry(key, value));
		}

		public virtual void Clear()
		{
			this.objectTable.Clear();
			this.objectList.Clear();
		}

		public virtual bool Contains(object key)
		{
			return this.objectTable.Contains(key);
		}

		public virtual void CopyTo(Array array, int idx)
		{
			this.objectTable.CopyTo(array, idx);
		}

		public virtual void Remove(object key)
		{
			this.objectTable.Remove(key);
			this.objectList.RemoveAt(this.IndexOf(key));
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new KeyedListEnumerator(this.objectList);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new KeyedListEnumerator(this.objectList);
		}

		public virtual int Count
		{
			get { return this.objectList.Count; }
		}

		public virtual bool IsFixedSize
		{
			get { return true; }
		}

		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		public virtual bool IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the maximum size of the map (the bound).
		/// </summary>
		public Int32 MaxSize
		{
			get { return this.maxSize; }
		}

		//	public object this[int idx] {
		//	    Get { return ((DictionaryEntry) objectList[idx]).Value; }
		//	    set {
		//		if (idx < 0 || idx >= Count)
		//		    throw new ArgumentOutOfRangeException ("index");
		//
		//		object key = ((DictionaryEntry) objectList[idx]).Key;
		//		objectList[idx] = new DictionaryEntry (key, value);
		//		objectTable[key] = value;
		//	    }
		//	}

		public virtual object this[object key]
		{
			get
			{
				this.MoveToMRU(key);
				return this.objectTable[key];
			}
			set
			{
				if (this.objectTable.Contains(key))
				{
					this.Remove(key);
				}
				this.Add(key, value);
			}
		}

		public virtual ICollection Keys
		{
			get
			{
				ArrayList retList = new ArrayList();
				for(int i = 0; i < this.objectList.Count; i++)
				{
					retList.Add(((DictionaryEntry) this.objectList[i]).Key);
				}
				return retList;
			}
		}

		public virtual ICollection Values
		{
			get
			{
				ArrayList retList = new ArrayList();
				for(int i = 0; i < this.objectList.Count; i++)
				{
					retList.Add(((DictionaryEntry) this.objectList[i]).Value);
				}
				return retList;
			}
		}

		public virtual object SyncRoot
		{
			get { return this; }
		}

		public void AddAll(IDictionary dictionary)
		{
			foreach(DictionaryEntry entry in dictionary)
			{
				this.Add(entry.Key, entry.Value);
			}
		}

		private int IndexOf(object key)
		{
			for(int i = 0; i < this.objectList.Count; i++)
			{
				if (((DictionaryEntry) this.objectList[i]).Key.Equals(key))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Remove the least recently used entry (the last one in the list)
		/// </summary>
		private void RemoveLRU()
		{
			Object key = ((DictionaryEntry) this.objectList[this.objectList.Count - 1]).Key;
			this.objectTable.Remove(key);
			this.objectList.RemoveAt(this.objectList.Count - 1);
		}

		private void MoveToMRU(Object key)
		{
			Int32 i = this.IndexOf(key);

			// only move if found and not already first
			if (i > 0)
			{
				Object value = this.objectList[i];
				this.objectList.RemoveAt(i);
				this.objectList.Insert(0, value);
			}
		}

		// Returns a thread-safe wrapper for a LRUMap.
		//
		public static LRUMap Synchronized(LRUMap table)
		{
			if (table == null)
			{
				throw new ArgumentNullException("table");
			}
			return new SyncLRUMap(table);
		}

		// Synchronized wrapper for LRUMap
		[Serializable()]
		private class SyncLRUMap : LRUMap, IDictionary, IEnumerable
		{
			protected LRUMap _table;

			internal SyncLRUMap(LRUMap table)
			{
				this._table = table;
			}

//	    internal SyncLRUMap(SerializationInfo Info, StreamingContext context) : base (Info, context) {
//		_table = (LRUMap)Info.GetValue("ParentTable", typeof(LRUMap));
//		if (_table==null) {
//		    throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
//		}
//	    }


			/*================================GetObjectData=================================
	     **Action: Return a serialization Info containing a reference to _table.  We need
	     **        to implement this because our parent HT does and we don't want to actually
	     **        serialize all of it's values (just a reference to the table, which will then
	     **        be serialized separately.)
	     **Returns: void
	     **Arguments: Info -- the SerializationInfo into which to store the data.
	     **           context -- the StreamingContext for the current serialization (ignored)
	     **Exceptions: ArgumentNullException if Info is null.
	     ==============================================================================*/
//	    public override void GetObjectData(SerializationInfo Info, StreamingContext context) {
//		if (Info==null) {
//		    throw new ArgumentNullException("Info");
//		}
//		Info.AddValue("ParentTable", _table, typeof(Hashtable));
//	    }

			public override int Count
			{
				get { return this._table.Count; }
			}

			public override bool IsReadOnly
			{
				get { return this._table.IsReadOnly; }
			}

			public override bool IsFixedSize
			{
				get { return this._table.IsFixedSize; }
			}

			public override bool IsSynchronized
			{
				get { return true; }
			}

			public override Object this[Object key]
			{
				get { return this._table[key]; }
				set
				{
					lock(this._table.SyncRoot)
					{
						this._table[key] = value;
					}
				}
			}

			public override Object SyncRoot
			{
				get { return this._table.SyncRoot; }
			}

			public override void Add(Object key, Object value)
			{
				lock(this._table.SyncRoot)
				{
					this._table.Add(key, value);
				}
			}

			public override void Clear()
			{
				lock(this._table.SyncRoot)
				{
					this._table.Clear();
				}
			}

			public override bool Contains(Object key)
			{
				return this._table.Contains(key);
			}

//	    public override bool ContainsKey(Object key) {
//		return _table.ContainsKey(key);
//	    }

//	    public override bool ContainsValue(Object key) {
//		return _table.ContainsValue(key);
//	    }

			public override void CopyTo(Array array, int arrayIndex)
			{
				this._table.CopyTo(array, arrayIndex);
			}



			IDictionaryEnumerator IDictionary.GetEnumerator()
			{
				return ((IDictionary) this._table).GetEnumerator();
			}

//	    protected override int GetHash(Object key) {
//		return _table.GetHash(key);
//	    }

//	    protected override bool KeyEquals(Object item, Object key) {
//		return _table.KeyEquals(item, key);
//	    }

			public override ICollection Keys
			{
				get
				{
					lock(this._table.SyncRoot)
					{
						return this._table.Keys;
					}
				}
			}

			public override ICollection Values
			{
				get
				{
					lock(this._table.SyncRoot)
					{
						return this._table.Values;
					}
				}
			}

			public override void Remove(Object key)
			{
				lock(this._table.SyncRoot)
				{
					this._table.Remove(key);
				}
			}

			/*==============================OnDeserialization===============================
	     **Action: Does nothing.  We have to implement this because our parent HT implements it,
	     **        but it doesn't do anything meaningful.  The real work will be done when we
	     **        call OnDeserialization on our parent table.
	     **Returns: void
	     **Arguments: None
	     **Exceptions: None
	     ==============================================================================*/
//	    public override void OnDeserialization(Object sender) {
//		return;
//	    }
		}
	}
}
