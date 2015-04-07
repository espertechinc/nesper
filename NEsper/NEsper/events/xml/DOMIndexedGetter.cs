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
    /// Getter for retrieving a value at a certain index.
    /// </summary>
    public class DOMIndexedGetter 
        : EventPropertyGetter
        , DOMPropertyGetter
    {
        private readonly String _propertyName;
        private readonly int _index;
        private readonly FragmentFactory _fragmentFactory;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">index</param>
        /// <param name="fragmentFactory">for creating fragments if required</param>
        public DOMIndexedGetter(String propertyName, int index, FragmentFactory fragmentFactory)
        {
            _propertyName = propertyName;
            _index = index;
            _fragmentFactory = fragmentFactory;
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            return null;
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            return null;
        }

        public Object GetValueAsFragment(XmlNode node)
        {
            if (_fragmentFactory != null)
            {
                var result = GetValueAsNode(node);
                if (result != null)
                {
                    return _fragmentFactory.GetEvent(result);
                }
            }

            return null;
        }

        public object GetValueAsFragment(XObject node)
        {
            if (_fragmentFactory != null)
            {
                var result = GetValueAsNode(node);
                if (result != null)
                {
                    return _fragmentFactory.GetEvent(result);
                }
            }

            return null;
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            var list = node.ChildNodes;
            int count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                var elementName = childNode.LocalName;
                if (elementName != _propertyName)
                {
                    continue;
                }
    
                if (count == _index)
                {
                    return childNode;
                }
                count++;
            }

            return null;
        }

        public XObject GetValueAsNode(XObject node)
        {
            // #1 reason why LINQ beats traditional methods
            var element = node as XElement;
            if (element != null) {
                return element.Nodes()
                    .Where(c => c != null)
                    .Where(c => c.NodeType == XmlNodeType.Element)
                    .Cast<XElement>()
                    .Where(c => c.Name.LocalName == _propertyName)
                    .Skip(_index)
                    .FirstOrDefault();
            }

            return null;
        }

        public Object Get(EventBean eventBean)
        {
            var xnode = eventBean.Underlying as XNode;
            if (xnode == null) {
                var node = eventBean.Underlying as XmlNode;
                if (node == null) {
                    return null;
                }

                return GetValueAsNode(node);
            }

            return GetValueAsNode(xnode);
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            var xnode = eventBean.Underlying as XNode;
            if (xnode == null) {
                var node = eventBean.Underlying as XmlNode;
                if (node == null) {
                    return false;
                }

                return GetValueAsNode(node) != null;
            }

            return GetValueAsNode(xnode) != null;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            var xnode = eventBean.Underlying as XNode;
            if (xnode == null) {
                var node = eventBean.Underlying as XmlNode;
                if (node == null) {
                    return null;
                }

                return GetValueAsFragment(node);
            }

            return GetValueAsFragment(xnode);
        }
    }
}
