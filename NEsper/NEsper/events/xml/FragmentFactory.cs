///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Factory for event fragments for use with DOM getters.
    /// </summary>
    public interface FragmentFactory
    {
        /// <summary>
        /// Gets the event.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        EventBean GetEvent(XObject result);

        /// <summary>
        /// Returns a fragment for the node.
        /// </summary>
        /// <param name="result">node to fragment</param>
        /// <returns>
        /// fragment
        /// </returns>
        EventBean GetEvent(XmlNode result);
    }
}
