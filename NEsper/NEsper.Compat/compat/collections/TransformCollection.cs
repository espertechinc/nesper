using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.compat.collections
{
    public class TransformCollection<TInt,TExt> : ICollection<TExt>
    {
        /// <summary>
        /// Underlying collection
        /// </summary>
        private readonly ICollection<TInt> _trueCollection;
        /// <summary>
        /// Function that transforms items from the "external" type to the "internal" type
        /// </summary>
        private readonly Func<TExt, TInt> _transformExtInt;

        /// <summary>
        /// Function that transforms items from the "internal" type to the "external" type
        /// </summary>
        private readonly Func<TInt, TExt> _transformIntExt;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformCollection&lt;TInt, TExt&gt;"/> class.
        /// </summary>
        /// <param name="trueCollection">The true collection.</param>
        /// <param name="transformExtInt">The transform ext int.</param>
        /// <param name="transformIntExt">The transform int ext.</param>
        public TransformCollection(ICollection<TInt> trueCollection,
                                   Func<TExt, TInt> transformExtInt,
                                   Func<TInt, TExt> transformIntExt)
        {
            _trueCollection = trueCollection;
            _transformExtInt = transformExtInt;
            _transformIntExt = transformIntExt;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TExt> GetEnumerator()
        {
#if true
            var enumerator = _trueCollection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return _transformIntExt.Invoke(enumerator.Current);
            }
#else
            return _trueCollection.Select(item => _transformIntExt.Invoke(item)).GetEnumerator();
#endif
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(TExt item)
        {
            _trueCollection.Add(_transformExtInt(item));
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _trueCollection.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TExt item)
        {
            return _trueCollection.Contains(_transformExtInt(item));
        }

        public void CopyTo(TExt[] array, int arrayIndex)
        {
            var arrayLength = array.Length;
            var trueEnum = _trueCollection.GetEnumerator();

            while(trueEnum.MoveNext() && arrayIndex < arrayLength)
            {
                array[arrayIndex++] = _transformIntExt(trueEnum.Current);
            }
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(TExt item)
        {
            return _trueCollection.Remove(_transformExtInt(item));
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _trueCollection.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly => _trueCollection.IsReadOnly;
    }
}
