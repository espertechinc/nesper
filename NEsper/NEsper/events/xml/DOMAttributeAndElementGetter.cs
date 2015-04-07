///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for both attribute and element values, attributes are checked first.
    /// </summary>
    public class DOMAttributeAndElementGetter
        : EventPropertyGetter
        , DOMPropertyGetter
    {
        private readonly String _propertyName;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        public DOMAttributeAndElementGetter(String propertyName)
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
            if (namedNodeMap != null)
            {
                for (int i = 0; i < namedNodeMap.Count; i++)
                {
                    var attrNode = namedNodeMap[i];
                    if (!string.IsNullOrEmpty(attrNode.LocalName))
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
            }
    
            var list = node.ChildNodes;
            for (int i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                if (!string.IsNullOrEmpty(childNode.LocalName)) {
                    if (_propertyName == childNode.LocalName) {
                        return childNode;
                    }
                    continue;
                }

                if (childNode.Name == _propertyName) {
                    return childNode;
                }
            }

            return null;
        }

        public XObject GetValueAsNode(XObject node)
        {
            var element = (XElement) node;
            var namedNodeMap = element.Attributes();
            foreach(var attrNode in namedNodeMap)
            {
                if (!string.IsNullOrEmpty(attrNode.Name.LocalName))
                {
                    if (_propertyName == attrNode.Name.LocalName)
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

            var list = element.Nodes()
                .Where(c => c != null)
                .Where(c => c.NodeType == XmlNodeType.Element)
                .Cast<XElement>();

            foreach(var childNode in list)
            {
                if (!string.IsNullOrEmpty(childNode.Name.LocalName))
                {
                    if (_propertyName == childNode.Name.LocalName)
                    {
                        return childNode;
                    }
                    continue;
                }

                if (childNode.Name == _propertyName)
                {
                    return childNode;
                }
            }

            return null;
        }

        public Object Get(EventBean eventBean)
        {
            XmlNode node = eventBean.Underlying as XmlNode;
            if ( node == null )
            {
                throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                        "the underlying data object is not of type Node");
            }
    
            return GetValueAsNode(node);
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
