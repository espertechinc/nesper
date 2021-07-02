using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.util;
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
        private readonly ICollection<object> _valueCollection;
        private readonly Type _valueCollectionType;

        protected FlexCollection(FlexCollection source)
        {
            _eventBeanCollection = source._eventBeanCollection;
            _valueCollection = source._valueCollection;
            _valueCollectionType = source._valueCollectionType;
        }

        protected FlexCollection(ICollection<EventBean> eventBeanCollection)
        {
            _eventBeanCollection = eventBeanCollection;
        }

        protected FlexCollection(ICollection<object> valueCollection, Type valueCollectionType)
        {
            _valueCollection = valueCollection;
            _valueCollectionType = valueCollectionType;
        }

        public bool IsEventBeanCollection {
            get => _eventBeanCollection != null;
        }

        public bool IsValueCollection {
            get => _valueCollection != null;
        }

        public ICollection<EventBean> EventBeanCollection {
            get {
                if (_eventBeanCollection != null) {
                    return _eventBeanCollection;
                }
                else if (_valueCollection != null) {
                    throw new IllegalStateException("cannot use object collection as eventBean collection");
                }
                else {
                    return null;
                }
            }
        }

        public ICollection<object> ValueCollection {
            get {
                if (_valueCollection != null) {
                    return _valueCollection;
                }
                else {
                    // Under type erasure, ICollection<EventBean> would be equivalent to ICollection<object>.  In the CLR
                    // this is not true and we must account for this.
                    return _eventBeanCollection?.Unwrap<object>();
                }
            }
        }

        public ICollection<T> Unpack<T>()
        {
            if (_valueCollection != null) {
                return _valueCollection.Unwrap<T>();
            }
            else {
                // Under type erasure, ICollection<EventBean> would be equivalent to ICollection<object>.  In the CLR
                // this is not true and we must account for this.
                return _eventBeanCollection?.Unwrap<T>();
            }
        }

        public object Underlying {
            get {
                if (_eventBeanCollection != null) {
                    return _eventBeanCollection;
                }
                else {
                    return _valueCollection;
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
            else if (_valueCollection != null) {
                return _valueCollection.GetEnumerator();
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
            else if (_valueCollection != null) {
                return _valueCollection.Contains(item);
            }

            return false;
        }

        public void CopyTo(
            object[] array,
            int arrayIndex)
        {
            if (_eventBeanCollection != null) {
                var asList = _eventBeanCollection.OfType<object>().ToList();
                // yes, this is not efficient
                asList.CopyTo(array, arrayIndex);
            }
            else {
                _valueCollection?.CopyTo(array, arrayIndex);
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
                else if (_valueCollection != null) {
                    return _valueCollection.Count;
                }
                else {
                    return 0;
                }
            }
        }

        public bool IsReadOnly => true;
        
        public static ICollection<T> Unpack<T>(object value)
        {
            if (value == null) {
                return null;
            }
            else if (value is FlexCollection<T> flexCollection) {
                return flexCollection.Unpack<T>();
            }
            else if (value is ICollection<T> valueCollection) {
                return valueCollection;
            }

            return value.Unwrap<T>();
        }
        
        public static FlexCollection OfEvent(EventBean eventBean)
        {
            return new FlexCollection(Collections.SingletonList<EventBean>(eventBean));
        }

        public static FlexCollection OfObject<T>(T value)
        {
            return new FlexCollection<T>(Collections.SingletonList<object>(value), typeof(T));
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
                return new FlexCollection(objectCollection, typeof(object));
            }
            else if (value.GetType().IsGenericCollection()) {
                var valueCollectionType = GenericExtensions.GetComponentType(value.GetType());
                return new FlexCollection(value.AsObjectCollection(), valueCollectionType);
            }

            throw new ArgumentException($"Unable to convert from unknown type \"{value.GetType().TypeSafeName()}\"");
        }

        public static readonly FlexCollection Empty = new FlexCollection(EmptyList<EventBean>.Instance);
    }

    public class FlexCollection<T> : FlexCollection
    {
        public FlexCollection(ICollection<EventBean> eventBeanCollection) : base(eventBeanCollection)
        {
        }

        public FlexCollection(
            ICollection<object> valueCollection,
            Type valueCollectionType) : base(valueCollection, valueCollectionType)
        {
        }
    }
    
    public static class FlexCollectionExtensions {
        public static FlexCollection AsFlexCollection(this object value)
        {
            return FlexCollection.Of(value);
        }

        public static bool IsFlexCollection(this Type type)
        {
            if (type == typeof(FlexCollection)) {
                return true;
            } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FlexCollection<>)) {
                return true;
            }

            return false;
        }

        public static Type Flexify(this Type type)
        {
            return IsFlexCollection(type) ? typeof(FlexCollection) : type;
        }
    }
}