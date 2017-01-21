///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Specification for a variant event stream.
    /// </summary>
    public class VariantSpec
    {
        private readonly String variantStreamName;
        private readonly EventType[] eventTypes;
        private readonly TypeVarianceEnum typeVariance;
    
        /// <summary>Ctor. </summary>
        /// <param name="variantStreamName">name of variant stream</param>
        /// <param name="eventTypes">types of events for variant stream, or empty list</param>
        /// <param name="typeVariance">enum specifying type variance</param>
        public VariantSpec(String variantStreamName, EventType[] eventTypes, TypeVarianceEnum typeVariance)
        {
            this.variantStreamName = variantStreamName;
            this.eventTypes = eventTypes;
            this.typeVariance = typeVariance;
        }

        /// <summary>Returns name of variant stream. </summary>
        /// <returns>name</returns>
        public string VariantStreamName
        {
            get { return variantStreamName; }
        }

        /// <summary>Returns types allowed for variant streams. </summary>
        /// <returns>types</returns>
        public EventType[] EventTypes
        {
            get { return eventTypes; }
        }

        /// <summary>Returns the type variance enum. </summary>
        /// <returns>type variance</returns>
        public TypeVarianceEnum TypeVariance
        {
            get { return typeVariance; }
        }
    }
}
