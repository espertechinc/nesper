///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Factory for event fragments for use with DOM getters.
    /// </summary>
    public interface FragmentFactory
    {
        /// <summary>
        ///     Returns a fragment for the node.
        /// </summary>
        /// <param name="result">node to fragment</param>
        /// <returns>fragment</returns>
        EventBean GetEvent(XmlNode result);
    }
} // end of namespace