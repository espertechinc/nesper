///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Enumerator over DOM nodes that positions between elements.
    /// </summary>
    public class DOMElementEnumerator
    {
        public static IEnumerator<XmlElement> Create(XmlNodeList nodeList)
        {
            foreach (var node in nodeList) {
                if (node is XmlElement element) {
                    yield return element;
                }
            }
        }

        public static IEnumerable<XmlElement> For(XmlNodeList nodeList)
        {
            foreach (var node in nodeList) {
                if (node is XmlElement element) {
                    yield return element;
                }
            }
        }
    }
} // end of namespace