///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Property descriptor for use by revision event types to maintain access to
    /// revision event properties.
    /// </summary>
    public class RevisionPropertyTypeDesc
    {
        private readonly EventPropertyGetter revisionGetter;
        private readonly RevisionGetterParameters revisionGetterParams;
        private readonly Object propertyType;  // Can be the {Type|Map|EventType}

        /// <summary>Ctor. </summary>
        /// <param name="revisionGetter">getter to use</param>
        /// <param name="revisionGetterParams">getter parameters</param>
        /// <param name="propertyType">type of the property</param>
        public RevisionPropertyTypeDesc(EventPropertyGetter revisionGetter,
                                        RevisionGetterParameters revisionGetterParams,
                                        Type propertyType)
        {
            this.revisionGetter = revisionGetter;
            this.revisionGetterParams = revisionGetterParams;
            this.propertyType = propertyType;
        }

        /// <summary>Returns the getter for the property on the revision event type. </summary>
        /// <returns>getter</returns>
        public EventPropertyGetter RevisionGetter
        {
            get { return revisionGetter; }
        }

        /// <summary>Returns parameters for the getter for the property on the revision event type. </summary>
        /// <returns>getter parameters</returns>
        public RevisionGetterParameters RevisionGetterParams
        {
            get { return revisionGetterParams; }
        }

        /// <summary>Returns property type. </summary>
        /// <returns>type</returns>
        public object PropertyType
        {
            get { return propertyType; }
        }
    }
}
