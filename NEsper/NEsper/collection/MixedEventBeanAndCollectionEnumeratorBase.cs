///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.collection
{
    public abstract class MixedEventBeanAndCollectionEnumeratorBase
        : IEnumerator<EventBean>
    {
        private readonly IEnumerator<EventBean> _eventBeanEnum;

        protected abstract Object GetValue(Object keyValue);

        protected MixedEventBeanAndCollectionEnumeratorBase(IEnumerable<Object> keyEnumerator)
            : this(keyEnumerator.GetEnumerator())
        {
        }

        protected MixedEventBeanAndCollectionEnumeratorBase(IEnumerator<Object> keyEnumerator)
        {
            _eventBeanEnum = MakeEnumerator(keyEnumerator);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public void Reset()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            return _eventBeanEnum.MoveNext();
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>The current.</value>
        object IEnumerator.Current
        {
            get { return _eventBeanEnum.Current; }
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>The current.</value>
        public EventBean Current
        {
            get { return _eventBeanEnum.Current; }
        }

        private IEnumerator<EventBean> MakeEnumerator(IEnumerator<Object> keyEnumerator)
        {
            while (keyEnumerator.MoveNext())
            {
                var entry = GetValue(keyEnumerator.Current);
                if (entry is ICollection<EventBean>)
                {
                    var collection = (ICollection<EventBean>) entry;
                    foreach (var eventBean in collection)
                    {
                        yield return eventBean;
                    }
                }
                else if (entry is EventBean)
                {
                    yield return ((EventBean) entry);
                }
                else if (entry == null)
                {
                }
                else
                {
                    throw new IllegalStateException();
                }
            }
        }
    }

    public class MixedEventBeanAndCollectionEnumerator 
        : MixedEventBeanAndCollectionEnumeratorBase
    {
        public Func<object, object> ProcGetValue { get; set; }

        public MixedEventBeanAndCollectionEnumerator(IEnumerator<object> keyEnumerator, Func<object, object> procGetValue)
            : base(keyEnumerator)
        {
            ProcGetValue = procGetValue;
        }

        public MixedEventBeanAndCollectionEnumerator(IEnumerable<object> keyEnumerator, Func<object, object> procGetValue) : base(keyEnumerator)
        {
            ProcGetValue = procGetValue;
        }

        public MixedEventBeanAndCollectionEnumerator(IEnumerator<object> keyEnumerator) : base(keyEnumerator)
        {
        }

        public MixedEventBeanAndCollectionEnumerator(IEnumerable<object> keyEnumerator) : base(keyEnumerator)
        {
        }

        protected override object GetValue(object keyValue)
        {
            return ProcGetValue(keyValue);
        }
    }
}
