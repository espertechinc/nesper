///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

#region

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using com.espertech.esper.client;

#endregion

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// DOM getter for Map-property.
    /// </summary>
    public class DOMMapGetter
        : EventPropertyGetter
        , DOMPropertyGetter
    {
        private readonly FragmentFactory _fragmentFactory;
        private readonly String _mapKey;
        private readonly String _propertyMap;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="mapKey">key in map</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        public DOMMapGetter(String propertyName, String mapKey, FragmentFactory fragmentFactory)
        {
            _propertyMap = propertyName;
            _mapKey = mapKey;
            _fragmentFactory = fragmentFactory;
        }

        #region EventPropertyGetter Members

        public Object Get(EventBean eventBean)
        {
            var asXNode = eventBean.Underlying as XNode;
            if (asXNode == null) {
                var node = eventBean.Underlying as XmlNode;
                return node == null ? null : GetValueAsNode(node);
            }

            return GetValueAsNode(asXNode);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var asXNode = eventBean.Underlying as XElement;
            if (asXNode == null) {
                var node = eventBean.Underlying as XmlNode;
                if (node == null) {
                    return false;
                }

                XmlNodeList list = node.ChildNodes;
                for (int i = 0; i < list.Count; i++) {
                    XmlNode childNode = list.Item(i);
                    if (childNode == null) {
                        continue;
                    }

                    if (childNode.NodeType != XmlNodeType.Element) {
                        continue;
                    }

                    string elementName = childNode.LocalName ?? childNode.Name;
                    if (_propertyMap != elementName) {
                        continue;
                    }

                    XmlNode attribute = childNode.Attributes.GetNamedItem(_mapKey);
                    if (attribute == null) {
                        continue;
                    }
                    if (attribute.InnerText != _mapKey) {
                        continue;
                    }

                    return true;
                }
            }
            else
            {
                return asXNode.Elements()
                    .Where(e => e.Name.LocalName == _propertyMap)
                    .Select(element => element.Attributes().FirstOrDefault(attr => attr.Name.LocalName == _mapKey))
                    .Where(attribute => attribute != null)
                    .Any(attribute => attribute.Value == _mapKey);
            }

            return false;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        #endregion

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
            if (_fragmentFactory == null) {
                return null;
            }

            XmlNode result = GetValueAsNode(node);
            return result == null ? null : _fragmentFactory.GetEvent(result);
        }

        public object GetValueAsFragment(XObject node)
        {
            if (_fragmentFactory == null)
            {
                return null;
            }

            XObject result = GetValueAsNode(node);
            return result == null ? null : _fragmentFactory.GetEvent(result);
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            XmlNodeList list = node.ChildNodes;
            for (int i = 0; i < list.Count; i++) {
                XmlNode childNode = list.Item(i);
                if (childNode == null) {
                    continue;
                }
                if (childNode.NodeType != XmlNodeType.Element) {
                    continue;
                }
                if (childNode.Name != _propertyMap) {
                    continue;
                }

                XmlNode attribute = childNode.Attributes.GetNamedItem("id");
                if (attribute == null) {
                    continue;
                }
                if (attribute.InnerText != _mapKey) {
                    continue;
                }

                return childNode;
            }
            return null;
        }

        public XObject GetValueAsNode(XObject node)
        {
            var element = node as XElement;
            if (element != null) {
                var list = element.Elements().Where(e => e.Name.LocalName == _propertyMap);
                return (from subElement in list
                        let attribute = subElement.Attributes(XName.Get("id")).FirstOrDefault()
                        where attribute != null
                        where attribute.Value == _mapKey
                        select subElement).FirstOrDefault();
            }

            return null;
        }
    }
}
