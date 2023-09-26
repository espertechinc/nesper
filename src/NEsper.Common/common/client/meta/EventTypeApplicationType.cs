///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.meta
{
    /// <summary>
    /// Application type.
    /// </summary>
    public enum EventTypeApplicationType
    {
        /// <summary>
        /// Xml type.
        /// </summary>
        XML,

        /// <summary>
        /// Map type.
        /// </summary>
        MAP,

        /// <summary>
        /// Object Array type.
        /// </summary>
        OBJECTARR,

        /// <summary>
        /// Class type.
        /// </summary>
        CLASS,

        /// <summary>
        /// Avro type.
        /// </summary>
        AVRO,

        /// <summary>
        /// Json type.
        /// </summary>
        JSON,

        /// <summary>
        /// Wrapper type.
        /// </summary>
        WRAPPER,

        /// <summary>
        /// Variant type.
        /// </summary>
        VARIANT
    }
} // end of namespace