///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for simple attributes in a DOM node.
    /// </summary>
    public class DOMSimpleAttributeGetter
        : EventPropertyGetter
        , DOMPropertyGetter
    {
        private readonly String _propertyName;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        public DOMSimpleAttributeGetter(String propertyName)
        {
            _propertyName = propertyName;
        }
    
        public Object GetValueAsFragment(XmlNode node)
        {
            return null;
        }

        public object GetValueAsFragment(XObject node)
        {
            return null;
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return null;
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            return null;
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            var namedNodeMap = node.Attributes;
            for (int i = 0; i < namedNodeMap.Count ; i++)
            {
                var attrNode = namedNodeMap.Item(i);
                if (! string.IsNullOrEmpty(attrNode.LocalName))
                {
                    if (_propertyName == attrNode.LocalName)
                    {
                        return attrNode;
                    }
                    continue;
                }

                if (_propertyName == attrNode.Name)
                {
                    return attrNode;
                }
            }
            return null;
        }

        public XObject GetValueAsNode(XObject node)
        {
            var element = node as XElement;
            if (element != null) {
                var namedNodeMap = element.Attributes();
                foreach(var attrNode in namedNodeMap) {
                    if (!string.IsNullOrEmpty(attrNode.Name.LocalName)) {
                        if (_propertyName == attrNode.Name.LocalName) {
                            return attrNode;
                        }
                        continue;
                    }

                    if (_propertyName == attrNode.Name) {
                        return attrNode;
                    }
                }
            }

            return null;
        }

        public Object Get(EventBean eventBean)
        {
            var xnode = eventBean.Underlying as XNode;
            if (xnode == null) {
                var node = eventBean.Underlying as XmlNode;
                if (node == null) {
                    throw new PropertyAccessException(
                        "Mismatched property getter to event bean type, " +
                        "the underlying data object is not of type Node");
                }

                return GetValueAsNode(node);
            }

            return GetValueAsNode(xnode);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;  // Never a fragment
        }
    }
}
