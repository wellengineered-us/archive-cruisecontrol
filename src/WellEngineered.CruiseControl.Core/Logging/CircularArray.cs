using System;
using System.Collections;
using System.Text;

namespace WellEngineered.CruiseControl.Core.Logging
{
    /// <summary>
    /// 	
    /// </summary>
	public class CircularArray : IEnumerable
	{
		private const int AssumedAverageLineLength = 80;
		private static readonly EnumeratorDirection DefaultDirection = EnumeratorDirection.Forward;
		private int currentIndex/* = 0*/;
		private object[] items;
		private bool isFull;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularArray" /> class.	
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <remarks></remarks>
		public CircularArray(int capacity)
		{
			this.items = new object[capacity];
		}

        /// <summary>
        /// Adds the specified item.	
        /// </summary>
        /// <param name="item">The item.</param>
        /// <remarks></remarks>
		public void Add(object item)
		{
			this.items[this.currentIndex] = item;
			this.currentIndex = this.IncrementIndex(this.currentIndex);
		}

        /// <summary>
        /// Gets the <see cref="object" /> at the specified index.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public object this[int index]
		{
			get { return this.items[index]; }
		}

		private int IncrementIndex(int index)
		{
			int nextIndex = (index + 1)%this.items.Length;
			if (nextIndex == 0) this.isFull = true;
			return nextIndex;
		}

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ToString()
		{
			return this.ToString(DefaultDirection);
		}

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string ToString(EnumeratorDirection direction)
		{
			StringBuilder builder = new StringBuilder(this.items.GetUpperBound(0)*AssumedAverageLineLength);
			IEnumerator enumerator = new CircularArrayEnumerator(this, direction);
			while (enumerator.MoveNext())
			{
				if (builder.Length > 0) builder.Append(Environment.NewLine);
				builder.Append(enumerator.Current);
			}
			return builder.ToString();
		}

        /// <summary>
        /// Gets the enumerator.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public IEnumerator GetEnumerator()
		{
			return new CircularArrayEnumerator(this, DefaultDirection);
		}
	
		internal class CircularArrayEnumerator : IEnumerator
		{
			private const int InitialIndex = -1;
			private readonly CircularArray array;
			private readonly EnumeratorDirection direction;
			private int index = InitialIndex;

			public CircularArrayEnumerator(CircularArray array, EnumeratorDirection direction)
			{
				this.array = array;
				this.direction = direction;
			}

			public bool MoveNext()
			{
				if (this.array.currentIndex == 0 && ! this.array.isFull) return false;	// array is empty
				if (this.index == InitialIndex)
				{
					this.index = this.StartIndex();
					return true;
				}

				if (this.direction == EnumeratorDirection.Backward)
				{
					this.index = this.Decrement(this.index);
					return this.index != this.StartIndex();					
				}
				else
				{
					this.index = this.Increment(this.index);
					return this.index != this.StartIndex();										
				}
			}

			private int StartIndex()
			{
				return (this.direction == EnumeratorDirection.Backward) ? this.Decrement(this.array.currentIndex) : this.Increment(this.array.currentIndex - 1);
			}

			private int Increment(int index)
			{
				if (! this.array.isFull && index == this.array.currentIndex - 1) return 0;
				return (index + 1) % this.array.items.Length;
			}

			private int Decrement(int currentIndex)
			{
				if (! this.array.isFull && currentIndex == 0) return this.array.currentIndex - 1;
				return (currentIndex - 1 + this.array.items.Length) % this.array.items.Length;
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}

			public object Current
			{
				get { return this.array[this.index]; }
			}
		}
	}
}