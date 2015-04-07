///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for nested properties in a DOM tree.
    /// </summary>
    public class DOMNestedPropertyGetter
        : EventPropertyGetter
        , DOMPropertyGetter
    {
        private readonly DOMPropertyGetter[] _domGetterChain;
        private readonly FragmentFactory _fragmentFactory;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getterChain">is the chain of getters to retrieve each nested property</param>
        /// <param name="fragmentFactory">for creating fragments</param>
        public DOMNestedPropertyGetter(ICollection<EventPropertyGetter> getterChain, FragmentFactory fragmentFactory)
        {
            this._domGetterChain = new DOMPropertyGetter[getterChain.Count];
            this._fragmentFactory = fragmentFactory;
    
            int count = 0;
            foreach (EventPropertyGetter getter in getterChain)
            {
                _domGetterChain[count++] = (DOMPropertyGetter) getter;
            }        
        }

        public Object GetValueAsFragment(XmlNode node)
        {
            var result = GetValueAsNode(node);
            return result == null ? null : _fragmentFactory.GetEvent(result);
        }

        public object GetValueAsFragment(XObject node)
        {
            var result = GetValueAsNode(node);
            return result == null ? null : _fragmentFactory.GetEvent(result);
        }

        public XmlNode[] GetValueAsNodeArray(XmlNode node)
        {
            var value = node;
            for (int i = 0; i < _domGetterChain.Length - 1; i++)
            {
                value = _domGetterChain[i].GetValueAsNode(value);
    
                if (value == null)
                {
                    return null;
                }
            }
    
            return _domGetterChain[_domGetterChain.Length - 1].GetValueAsNodeArray(value);
        }

        public XObject[] GetValueAsNodeArray(XObject node)
        {
            XObject value = node;
            for (int i = 0; i < _domGetterChain.Length - 1; i++)
            {
                value = _domGetterChain[i].GetValueAsNode(value);

                if (value == null)
                {
                    return null;
                }
            }

            return _domGetterChain[_domGetterChain.Length - 1].GetValueAsNodeArray(value);
        }

        public XmlNode GetValueAsNode(XmlNode node)
        {
            XmlNode value = node;
    
            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                value = _domGetterChain[i].GetValueAsNode(value);
                if (value == null)
                {
                    return null;
                }
            }
    
            return value;
        }

        public XObject GetValueAsNode(XObject node)
        {
            XObject value = node;

            for (int i = 0; i < _domGetterChain.Length; i++)
            {
                value = _domGetterChain[i].GetValueAsNode(value);
                if (value == null)
                {
                    return null;
                }
            }

            return value;
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

        public bool IsExistsProperty(EventBean obj)
        {
            var xnode = obj.Underlying as XNode;
            if (xnode == null) {
                var node = obj.Underlying as XmlNode;
                if (node == null) {
                    throw new PropertyAccessException(
                        "Mismatched property getter to event bean type, " +
                        "the underlying data object is not of type Node");
                }

                for (int i = 0; i < _domGetterChain.Length; i++) {
                    XmlNode value = _domGetterChain[i].GetValueAsNode(node);
                    if (value == null) {
                        return false;
                    }
                }

                return true;
            }

            for (int i = 0; i < _domGetterChain.Length; i++) {
                XObject value = _domGetterChain[i].GetValueAsNode(xnode);
                if (value == null) {
                    return false;
                }
            }

            return true;
        }

        public Object GetFragment(EventBean obj)
        {
            var xnode = obj.Underlying as XNode;
            if (xnode == null) {
                var node = obj.Underlying as XmlNode;
                if (node == null)
                {
                    throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                      "the underlying data object is not of type Node");
                }

                var value = node;
                for (int i = 0; i < _domGetterChain.Length - 1; i++) {
                    value = _domGetterChain[i].GetValueAsNode(node);

                    if (value == null) {
                        return false;
                    }
                }

                return _domGetterChain[_domGetterChain.Length - 1].GetValueAsFragment(value);
            }
            else
            {
                XObject value = xnode;
                for (int i = 0; i < _domGetterChain.Length - 1; i++)
                {
                    value = _domGetterChain[i].GetValueAsNode(xnode);
                    if (value == null)
                    {
                        return false;
                    }
                }

                return _domGetterChain[_domGetterChain.Length - 1].GetValueAsFragment(value);
            }
        }
    }
}
