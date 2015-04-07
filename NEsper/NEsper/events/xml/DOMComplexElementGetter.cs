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
    /// Getter for a DOM complex element.
    /// </summary>
    public class DOMComplexElementGetter
        : EventPropertyGetter
        , DOMPropertyGetter
    {
        private readonly FragmentFactory _fragmentFactory;
        private readonly bool _isArray;
        private readonly String _propertyName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        /// <param name="isArray">if this is an array property</param>
        public DOMComplexElementGetter(String propertyName, FragmentFactory fragmentFactory, bool isArray)
        {
            _propertyName = propertyName;
            _fragmentFactory = fragmentFactory;
            _isArray = isArray;
        }

        #region EventPropertyGetter Members

        public Object Get(EventBean eventBean)
        {
            var asXNode = eventBean.Underlying as XNode;
            if (asXNode == null) {
                var asXml = eventBean.Underlying as XmlNode;
                if (asXml == null) {
                    throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                      "the underlying data object is not of type Node");
                }

                return _isArray ? (object) GetValueAsNodeArray(asXml) : GetValueAsNode(asXml);
            }

            return _isArray ? (object)GetValueAsNodeArray(asXNode) : GetValueAsNode(asXNode);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean obj)
        {
            var asXNode = obj.Underlying as XNode;
            if (asXNode == null) {
                var asXml = obj.Underlying as XmlNode;
                if (asXml == null) {
                    throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                      "the underlying data object is not of type Node");
                }

                return GetValueAsFragment(asXml);
            }

            return GetValueAsFragment(asXNode);
        }

        #endregion

        public Object GetValueAsFragment(XmlNode node)
        {
            if (!_isArray) {
                var result = GetValueAsNode(node);
                if (result == null) {
                    return result;
                }

                return _fragmentFactory.GetEvent(result);
            }
            else {
                var result = GetValueAsNodeArray(node);
                if ((result == null) || (result.Length == 0)) {
                    return new EventBean[0];
                }

                var events = new EventBean[result.Length];
                int count = 0;
                for (int i = 0; i < result.Length; i++) {
                    events[count++] = _fragmentFactory.GetEvent(result[i]);
                }
                return events;
            }
        }

        public object GetValueAsFragment(XObject node)
        {
            if (!_isArray)
            {
                var result = GetValueAsNode(node);
                if (result == null)
                {
                    return result;
                }

                return _fragmentFactory.GetEvent(result);
            }
            else
            {
                var result = GetValueAsNodeArray(node);
                if ((result == null) || (result.Length == 0))
                {
                    return new EventBean[0];
                }

                var events = new EventBean[result.Length];
                int count = 0;
                for (int i = 0; i < result.Length; i++)
                {
                    events[count++] = _fragmentFactory.GetEvent(result[i]);
                }
                return events;
            }
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            var list = node.ChildNodes;
            for (int i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                if (_propertyName == childNode.LocalName) {
                    return childNode;
                }
            }
            return null;
        }

        public XObject GetValueAsNode(XObject node)
        {
            var element = node as XElement;
            if (element != null) {
                foreach(var childNode in element.Elements()) {
                    if (childNode.Name.LocalName != null) {
                        if (_propertyName == childNode.Name.LocalName) {
                            return childNode;
                        }
                        continue;
                    }
                    if (_propertyName == childNode.Name) {
                        return childNode;
                    }
                }
            }

            return null;
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            var list = node.ChildNodes;

            int count = 0;
            for (int i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }

                if (childNode.NodeType == XmlNodeType.Element) {
                    count++;
                }
            }

            if (count == 0) {
                return new XmlNode[0];
            }

            var nodes = new XmlNode[count];
            int realized = 0;
            for (int i = 0; i < list.Count; i++) {
                var childNode = list.Item(i);
                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }

                if (_propertyName.Equals(childNode.LocalName)) {
                    nodes[realized++] = childNode;
                }
            }

            if (realized == count) {
                return nodes;
            }
            if (realized == 0) {
                return new XmlNode[0];
            }

            var shrunk = new XmlNode[realized];
            Array.Copy(nodes, 0, shrunk, 0, realized);
            return shrunk;
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            var element = node as XElement;
            if (element != null) {
                return element.Elements()
                    .Where(childNode => childNode.Name.LocalName == _propertyName)
                    .Cast<XObject>()
                    .ToArray();
            }

            return null;
        }
    }
}
