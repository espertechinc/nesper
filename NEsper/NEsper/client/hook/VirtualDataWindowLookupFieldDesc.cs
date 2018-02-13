///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// As part of a lookup context, see <see cref="VirtualDataWindowLookupContext" />, this object 
    /// encapsulates information about a single property in a correlated where-clause.
    /// </summary>
    public class VirtualDataWindowLookupFieldDesc
    {
        /// <summary>Ctor. </summary>
        /// <param name="propertyName">property name queried in where-clause</param>
        /// <param name="operator">operator</param>
        /// <param name="lookupValueType">lookup key type</param>
        public VirtualDataWindowLookupFieldDesc(
            string propertyName,
            VirtualDataWindowLookupOp? @operator, 
            Type lookupValueType)
        {
            PropertyName = propertyName;
            Operator = @operator;
            LookupValueType = lookupValueType;
        }

        /// <summary>Returns the property name queried in the where-clause. </summary>
        /// <value>property name.</value>
        public string PropertyName { get; private set; }

        /// <summary>Returns the type of lookup value provided. </summary>
        /// <value>lookup value type (aka. key type)</value>
        public Type LookupValueType { get; set; }

        /// <summary>Returns the operator. </summary>
        /// <value>operator</value>
        public VirtualDataWindowLookupOp? Operator { get; set; }
    }
}
