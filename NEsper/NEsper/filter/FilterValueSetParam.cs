///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// This interface represents one filter parameter in an <seealso cref="FilterValueSet" /> filter 
    /// specification. 
    /// <para/> 
    /// Each filtering parameter has a lookup-able and operator type, and a value to filter for. </summary>
    public interface FilterValueSetParam
    {
        /// <summary>Returns the lookup-able for the filter parameter. </summary>
        /// <value>lookup-able</value>
        FilterSpecLookupable Lookupable { get; }

        /// <summary>Returns the filter operator type. </summary>
        /// <value>filter operator type</value>
        FilterOperator FilterOperator { get; }

        /// <summary>Return the filter parameter constant to filter for. </summary>
        /// <value>filter parameter constant&apos;s value</value>
        object FilterForValue { get; }

        void AppendTo(TextWriter writer);
    }
}
