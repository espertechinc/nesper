///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Filter parameter value defining the event property to filter, the filter operator, and the filter value.
    /// </summary>
    public class FilterValueSetParamImpl : FilterValueSetParam
    {
        private readonly FilterSpecLookupable _lookupable;
        private readonly FilterOperator _filterOperator;
        private readonly Object _filterValue;
    
        /// <summary>Ctor. </summary>
        /// <param name="lookupable">stuff to use to interrogate</param>
        /// <param name="filterOperator">operator to apply</param>
        /// <param name="filterValue">value to look for</param>
        public FilterValueSetParamImpl(FilterSpecLookupable lookupable, FilterOperator filterOperator, Object filterValue)
        {
            _lookupable = lookupable;
            _filterOperator = filterOperator;
            _filterValue = filterValue;
        }

        public FilterSpecLookupable Lookupable
        {
            get { return _lookupable; }
        }

        public FilterOperator FilterOperator
        {
            get { return _filterOperator; }
        }

        public object FilterForValue
        {
            get { return _filterValue; }
        }

        public override String ToString() {
            return "FilterValueSetParamImpl{" +
                    "lookupable='" + _lookupable + '\'' +
                    ", filterOperator=" + _filterOperator +
                    ", filterValue=" + _filterValue +
                    '}';
        }

        public void AppendTo(TextWriter writer)
        {
            _lookupable.AppendTo(writer);
            writer.Write(_filterOperator.GetTextualOp());
            writer.Write(_filterValue == null ? "null" : _filterValue.ToString());
        }
    }
}
