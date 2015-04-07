///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Index for filter parameter constants to match using the equals (=) operator. The implementation is based on a 
    /// regular HashMap.
    /// </summary>
    public abstract class FilterParamIndexEqualsBase : FilterParamIndexLookupableBase
    {
        protected readonly IDictionary<Object, EventEvaluator> ConstantsMap;
        protected readonly IReaderWriterLock ConstantsMapRwLock;

        protected FilterParamIndexEqualsBase(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock, FilterOperator filterOperator)
            : base(filterOperator, lookupable)
        {
            ConstantsMap = new Dictionary<Object, EventEvaluator>().WithNullSupport();
            ConstantsMapRwLock = readWriteLock;
        }

        protected internal override EventEvaluator Get(Object filterConstant)
        {
            return ConstantsMap.Get(filterConstant);
        }

        protected internal override void Put(Object filterConstant, EventEvaluator evaluator)
        {
            ConstantsMap.Put(filterConstant, evaluator);
        }

        public override bool Remove(Object filterConstant)
        {
            return ConstantsMap.Remove(filterConstant);
        }

        public override int Count
        {
            get { return ConstantsMap.Count; }
        }

        public override IReaderWriterLock ReadWriteLock
        {
            get { return ConstantsMapRwLock; }
        }
    }
}