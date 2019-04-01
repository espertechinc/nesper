///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.pattern;
using com.espertech.esper.util;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This class represents a 'in' filter parameter in an <seealso cref="com.espertech.esper.filter.FilterSpecCompiled" /> filter specification.
    /// <para>
    /// The 'in' checks for a list of values.
    /// </para>
    /// </summary>
    public sealed class FilterSpecParamIn : FilterSpecParam{
        private readonly IList<FilterSpecParamInValue> _listOfValues;
        private readonly MultiKeyUntyped _inListConstantsOnly;
        private readonly bool _hasCollMapOrArray;
        private readonly InValueAdder[] _adders;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupable">is the event property or function</param>
        /// <param name="filterOperator">is expected to be the IN-list operator</param>
        /// <param name="listofValues">is a list of constants and event property names</param>
        /// <exception cref="ArgumentException">for illegal args</exception>
        public FilterSpecParamIn(
            FilterSpecLookupable lookupable,
            FilterOperator filterOperator,
            IList<FilterSpecParamInValue> listofValues)
            : base(lookupable, filterOperator)
        {
            _listOfValues = listofValues;

            foreach (FilterSpecParamInValue value in listofValues)
            {
                Type returnType = value.ReturnType;
                if (TypeHelper.IsCollectionMapOrArray(returnType))
                {
                    _hasCollMapOrArray = true;
                    break;
                }
            }

            if (_hasCollMapOrArray)
            {
                _adders = new InValueAdder[listofValues.Count];
                for (int i = 0; i < listofValues.Count; i++)
                {
                    Type returnType = listofValues[0].ReturnType;
                    if (returnType == null)
                    {
                        _adders[i] = InValueAdderPlain.INSTANCE;
                    }
                    else if (returnType.IsArray)
                    {
                        _adders[i] = InValueAdderArray.INSTANCE;
                    }
                    else if (returnType.IsGenericDictionary())
                    {
                        _adders[i] = InValueAdderMap.INSTANCE;
                    }
                    else if (returnType.IsGenericCollection())
                    {
                        _adders[i] = InValueAdderColl.INSTANCE;
                    }
                    else
                    {
                        _adders[i] = InValueAdderPlain.INSTANCE;
                    }
                }
            }

            bool isAllConstants = true;
            foreach (FilterSpecParamInValue value in listofValues)
            {
                if (!value.IsConstant)
                {
                    isAllConstants = false;
                    break;
                }
            }

            if (isAllConstants)
            {
                _inListConstantsOnly = GetFilterValues(null, null);
            }

            if ((filterOperator != FilterOperator.IN_LIST_OF_VALUES) &&
                ((filterOperator != FilterOperator.NOT_IN_LIST_OF_VALUES)))
            {
                throw new ArgumentException(
                    "Illegal filter operator " + filterOperator + " supplied to " +
                    "in-values filter parameter");
            }
        }

        public override Object GetFilterValue(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext)
        {
            // If the list of values consists of all-constants and no event properties, then use cached version
            if (_inListConstantsOnly != null) {
                return _inListConstantsOnly;
            }
            return GetFilterValues(matchedEvents, agentInstanceContext);
        }

        /// <summary>
        /// Returns the list of values we are asking to match.
        /// </summary>
        /// <value>list of filter values</value>
        public IList<FilterSpecParamInValue> ListOfValues
        {
            get { return _listOfValues; }
        }

        public override String ToString()
        {
            return base.ToString() + "  in=(listOfValues=" + _listOfValues.ToString() + ')';
        }
    
        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }
    
            if (!(obj is FilterSpecParamIn)) {
                return false;
            }
    
            var other = (FilterSpecParamIn) obj;
            if (!base.Equals(other)) {
                return false;
            }
    
            if (_listOfValues.Count != other._listOfValues.Count) {
                return false;
            }
    
            if (!(CompatExtensions.DeepEquals(_listOfValues, other._listOfValues))) {
                return false;
            }
            return true;
        }
    
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result = 31 * result + (_listOfValues != null ? ListOfValues.GetHashCode() : 0);
            return result;
        }
    
        private MultiKeyUntyped GetFilterValues(MatchedEventMap matchedEvents, AgentInstanceContext agentInstanceContext)
        {
            if (!_hasCollMapOrArray) {
                var constantsX = new Object[_listOfValues.Count];
                int countX = 0;
                foreach (FilterSpecParamInValue valuePlaceholder in _listOfValues) {
                    constantsX[countX++] = valuePlaceholder.GetFilterValue(matchedEvents, agentInstanceContext);
                }
                return new MultiKeyUntyped(constantsX);
            }
    
            var constants = new ArrayDeque<object>(_listOfValues.Count);
            int count = 0;
            foreach (FilterSpecParamInValue valuePlaceholder in _listOfValues) {
                Object value = valuePlaceholder.GetFilterValue(matchedEvents, agentInstanceContext);
                if (value != null) {
                    _adders[count].Add(constants, value);
                }
                count++;
            }
            return new MultiKeyUntyped(constants.ToArray());
        }
    
        private interface InValueAdder
        {
            void Add(ICollection<Object> constants, Object value);
        }

        private class InValueAdderArray : InValueAdder
        {
            internal static readonly InValueAdderArray INSTANCE = new InValueAdderArray();

            private InValueAdderArray()
            {
            }

            public void Add(ICollection<Object> constants, Object value)
            {
                var asArray = (Array) value;
                var len = asArray.Length;
                for (int i = 0; i < len; i++)
                {
                    constants.Add(asArray.GetValue(i));
                }
            }
        }

        private class InValueAdderMap : InValueAdder
        {
            internal static readonly InValueAdderMap INSTANCE = new InValueAdderMap();

            private InValueAdderMap()
            {
            }

            public void Add(ICollection<Object> constants, Object value)
            {
                IEnumerable<object> mapKeys;

                if (value.GetType().IsGenericDictionary())
                    mapKeys = MagicMarker.GetDictionaryFactory(value.GetType()).Invoke(value).Keys;
                else
                    throw new ArgumentException("invalid value", nameof(value));

                constants.AddAll(mapKeys);
            }
        }

        private class InValueAdderColl : InValueAdder
        {
            internal static readonly InValueAdderColl INSTANCE = new InValueAdderColl();

            private InValueAdderColl()
            {
            }

            public void Add(ICollection<Object> constants, Object value)
            {
                ICollection<object> collection;

                if (value is ICollection<object>)
                    collection = (ICollection<object>)value;
                else if (value.GetType().IsGenericCollection())
                    collection = MagicMarker.GetCollectionFactory(value.GetType()).Invoke(value);
                else
                    throw new ArgumentException("invalid value", nameof(value));

                constants.AddAll(collection);
            }
        }

        private class InValueAdderPlain : InValueAdder
        {
            internal static readonly InValueAdderPlain INSTANCE = new InValueAdderPlain();
    
            private InValueAdderPlain()
            {
            }
    
            public void Add(ICollection<Object> constants, Object value)
            {
                constants.Add(value);
            }
        }
    }
} // end of namespace
