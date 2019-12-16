using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.collection
{
    /// <summary>
    /// Introduced to compat the rampant type erasure problems with collection usage in codegen.
    /// </summary>
    public class FlexCollection : ICollection<object>
    {
        private readonly ICollection<EventBean> _eventBeanCollection;
        private readonly ICollection<object> _objectCollection;

        public static readonly FlexCollection Empty = new FlexCollection(new EmptyList<EventBean>()); 

        public FlexCollection(ICollection<EventBean> eventBeanCollection)
        {
            _eventBeanCollection = eventBeanCollection;
        }

        public FlexCollection(ICollection<object> objectCollection)
        {
            _objectCollection = objectCollection;
        }

        public bool IsEventBeanCollection {
            get => _eventBeanCollection != null;
        }

        public bool IsObjectCollection {
            get => _objectCollection != null;
        }

        public ICollection<EventBean> EventBeanCollection {
            get {
                if (_eventBeanCollection != null) {
                    return _eventBeanCollection;
                }
                else if (_objectCollection != null) {
                    throw new IllegalStateException("cannot use object collection as eventBean collection");
                }
                else {
                    return null;
                }
            }
        }

        public ICollection<object> ObjectCollection {
            get {
                if (_objectCollection != null) {
                    return _objectCollection;
                }
                else if (_eventBeanCollection != null) {
                    // Under type erasure, ICollection<EventBean> would be equivalent to ICollection<object>.  In the CLR
                    // this is not true and we must account for this.
                    return _eventBeanCollection.Unwrap<object>();
                }
                else {
                    return null;
                }
            }
        }

        public object Underlying {
            get {
                if (_eventBeanCollection != null) {
                    return _eventBeanCollection;
                }
                else if (_objectCollection != null) {
                    return _objectCollection;
                }
                else {
                    return null;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            if (_eventBeanCollection != null) {
                return _eventBeanCollection.Cast<object>().GetEnumerator();
            }
            else if (_objectCollection != null) {
                return _objectCollection.GetEnumerator();
            }
            else {
                return EnumerationHelper.Empty<object>();
            }
            
        }

        public void Add(object item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object item)
        {
            if (_eventBeanCollection != null) {
                if (item is EventBean eventBean) {
                    return _eventBeanCollection.Contains(eventBean);
                }
            }
            else if (_objectCollection != null) {
                return _objectCollection.Contains(item);
            }

            return false;
        }

        public void CopyTo(
            object[] array,
            int arrayIndex)
        {
            if (_eventBeanCollection != null) {
                var asList = _eventBeanCollection.Cast<object>().ToList();
                // yes, this is not efficient
                asList.CopyTo(array, arrayIndex);
            }
            else if (_objectCollection != null) {
                _objectCollection.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(object item)
        {
            throw new NotSupportedException();
        }

        public int Count {
            get {
                if (_eventBeanCollection != null) {
                    return _eventBeanCollection.Count;
                }
                else if (_objectCollection != null) {
                    return _objectCollection.Count;
                }
                else {
                    return 0;
                }
            }
        }

        public bool IsReadOnly => true;
        
        public static FlexCollection OfEvent(EventBean eventBean)
        {
            return new FlexCollection(Collections.SingletonList<EventBean>(eventBean));
        }

        public static FlexCollection OfObject(object value)
        {
            return new FlexCollection(Collections.SingletonList<object>(value));
        }

        public static FlexCollection Of(object value)
        {
            if (value == null) {
                return null;
            }
            else if (value is FlexCollection flexCollection) {
                return flexCollection;
            }
            else if (value is ICollection<EventBean> eventBeanCollection) {
                return new FlexCollection(eventBeanCollection);
            }
            else if (value is ICollection<object> objectCollection) {
                return new FlexCollection(objectCollection);
            }
            else if (value.GetType().IsGenericCollection()) {
                return new FlexCollection(value.AsObjectCollection());
            }

            throw new ArgumentException($"Unable to convert from unknown type \"{value.GetType().CleanName()}\"");
        }
    }
    
    public static class FlexCollectionExtensions {
        public static FlexCollection AsFlexCollection(this object value)
        {
            return FlexCollection.Of(value);
        }
    }
}