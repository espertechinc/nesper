///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace com.espertech.esper.compat.xml
{
    public class XPathIteratorNodeList : XmlNodeList
    {
        private readonly List<XmlNode> _nodeList;

        public XPathIteratorNodeList(XPathNodeIterator iterator)
        {
            _nodeList = new List<XmlNode>(iterator.Count);
            while (iterator.MoveNext()) {
                var value = iterator.Current;
                if (value is IHasXmlNode) {
                    _nodeList.Add(((IHasXmlNode) value).GetNode());
                } else {
                    throw new ArgumentException("unable to handle node type");
                }
            }
        }

        #region Overrides of XmlNodeList

        public override XmlNode Item(int index)
        {
            return _nodeList[index];
        }

        public override IEnumerator GetEnumerator()
        {
            return _nodeList.GetEnumerator();
        }

        public override int Count
        {
            get { return _nodeList.Count; }
        }

        #endregion
    }
}
