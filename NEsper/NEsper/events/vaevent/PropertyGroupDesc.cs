///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// For use with building groups of event properties to reduce overhead in maintaining versions.
    /// </summary>
    public class PropertyGroupDesc 
    {
        private readonly int groupNum;
        private readonly IDictionary<EventType, String> types;
        private readonly String[] properties;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="groupNum">the group number</param>
        /// <param name="nameTypeSet">the event types and their names whose totality of properties fully falls within this group.</param>
        /// <param name="properties">is the properties in the group</param>
        public PropertyGroupDesc(int groupNum, IDictionary<EventType, String> nameTypeSet, String[] properties)
        {
            this.groupNum = groupNum;
            this.types = nameTypeSet;
            this.properties = properties;
        }

        /// <summary>Returns the group number. </summary>
        /// <returns>group number</returns>
        public int GroupNum
        {
            get { return groupNum; }
        }

        /// <summary>Returns the types. </summary>
        /// <returns>types</returns>
        public IDictionary<EventType, string> Types
        {
            get { return types; }
        }

        /// <summary>Returns the properties. </summary>
        /// <returns>properties</returns>
        public ICollection<string> Properties
        {
            get { return properties; }
        }

        public override String ToString()
        {
            return "groupNum=" + groupNum +
                   " properties=" + properties.Render() +
                   " nameTypes=" + types;
        }
    }
}
