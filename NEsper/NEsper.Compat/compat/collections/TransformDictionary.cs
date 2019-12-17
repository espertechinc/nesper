using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.compat.collections
{
    public class TransformDictionary<TK1,TV1,TK2,TV2> : IDictionary<TK1, TV1>
    {
        /// <summary>
        /// Gets or sets the sub dictionary.
        /// </summary>
        /// <value>
        /// The sub dictionary.
        /// </value>
        public IDictionary<TK2, TV2> SubDictionary { get; set; }

        /// <summary>
        /// Gets or sets the key out transform.
        /// </summary>
        /// <value>
        /// The key transform.
        /// </value>
        public Func<TK2, TK1> KeyOut { get; set; }

        /// <summary>
        /// Gets or sets the key out transform.
        /// </summary>
        /// <value>
        /// The key transform.
        /// </value>
        public Func<TK1, TK2> KeyIn { get; set; }

        /// <summary>
        /// Gets or sets the value transform.
        /// </summary>
        /// <value>
        /// The value transform.
        /// </value>
        public Func<TV2, TV1> ValueOut { get; set; }

        /// <summary>
        /// Gets or sets the value transform.
        /// </summary>
        /// <value>
        /// The value transform.
        /// </value>
        public Func<TV1, TV2> ValueIn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformDictionary{TK1, TV1, TK2, TV2}"/> class.
        /// </summary>
        public TransformDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformDictionary{TK1, TV1, TK2, TV2}"/> class.
        /// </summary>
        /// <param name="subDictionary">The sub dictionary.</param>
        public TransformDictionary(IDictionary<TK2, TV2> subDictionary)
        {
            SubDictionary = subDictionary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformDictionary{TK1, TV1, TK2, TV2}"/> class.
        /// </summary>
        /// <param name="subDictionary">The sub dictionary.</param>
        /// <param name="keyOut">The key out.</param>
        /// <param name="keyIn">The key in.</param>
        /// <param name="valueOut">The value out.</param>
        /// <param name="valueIn">The value in.</param>
        public TransformDictionary(IDictionary<TK2, TV2> subDictionary, Func<TK2, TK1> keyOut, Func<TK1, TK2> keyIn, Func<TV2, TV1> valueOut, Func<TV1, TV2> valueIn)
        {
            SubDictionary = subDictionary;
            KeyOut = keyOut;
            KeyIn = keyIn;
            ValueOut = valueOut;
            ValueIn = valueIn;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator<KeyValuePair<TK1, TV1>> GetEnumerator()
        {
            return SubDictionary.Select(ExtCast).GetEnumerator();
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<TK1> Keys => new TransformCollection<TK2, TK1>(
            SubDictionary.Keys, KeyIn, KeyOut);

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<TV1> Values => new TransformCollection<TV2, TV1>(
            SubDictionary.Values, ValueIn, ValueOut);

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Add(KeyValuePair<TK1, TV1> item)
        {
            SubDictionary.Add(IntCast(item));
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Clear()
        {
            SubDictionary.Clear();
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => SubDictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => SubDictionary.IsReadOnly;

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Add(TK1 key, TV1 value)
        {
            SubDictionary.Add(IntCast(key), IntCast(value));
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Contains(KeyValuePair<TK1, TV1> item)
        {
            return SubDictionary.Contains(IntCast(item));
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Remove(KeyValuePair<TK1, TV1> item)
        {
            return SubDictionary.Remove(IntCast(item));
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Remove(TK1 key)
        {
            return SubDictionary.Remove(IntCast(key));
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool ContainsKey(TK1 key)
        {
            return SubDictionary.ContainsKey(IntCast(key));
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TV1 this[TK1 key]
        {
            get => ExtCast(SubDictionary[IntCast(key)]);
            set { SubDictionary[IntCast(key)] = IntCast(value); }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that : <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool TryGetValue(TK1 key, out TV1 value)
        {
            TV2 evalue;

            if (SubDictionary.TryGetValue(IntCast(key), out evalue))
            {
                value = ExtCast(evalue);
                return true;
            }

            value = default(TV1);
            return false;
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void CopyTo(KeyValuePair<TK1, TV1>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the item from the "internal" repesentation to the "external" representation.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private KeyValuePair<TK1, TV1> ExtCast(KeyValuePair<TK2, TV2> item)
        {
            return new KeyValuePair<TK1, TV1>(KeyOut(item.Key), ValueOut(item.Value));
        }

        /// <summary>
        /// Converts the item from the "internal" repesentation to the "external" representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private TK1 ExtCast(TK2 value)
        {
            return KeyOut(value);
        }

        /// <summary>
        /// Converts the item from the "internal" repesentation to the "external" representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private TV1 ExtCast(TV2 value)
        {
            return ValueOut(value);
        }

        /// <summary>
        /// Converts the item from the "external" repesentation to the "internal" representation.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private KeyValuePair<TK2, TV2> IntCast(KeyValuePair<TK1, TV1> item)
        {
            return new KeyValuePair<TK2, TV2>(KeyIn(item.Key), ValueIn(item.Value));
        }

        /// <summary>
        /// Converts the item from the "external" repesentation to the "internal" representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private TK2 IntCast(TK1 value)
        {
            return KeyIn(value);
        }

        /// <summary>
        /// Converts the item from the "external" repesentation to the "internal" representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private TV2 IntCast(TV1 value)
        {
            return ValueIn(value);
        }
    }
}
