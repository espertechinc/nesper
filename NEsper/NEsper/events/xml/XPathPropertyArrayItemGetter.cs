///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for XPath explicit properties returning an element in an array.
    /// </summary>
    public class XPathPropertyArrayItemGetter : EventPropertyGetter
    {
        private readonly FragmentFactory fragmentFactory;
        private readonly EventPropertyGetter getter;
        private readonly int index;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">property getter returning the parent node</param>
        /// <param name="index">to get item at</param>
        /// <param name="fragmentFactory">for creating fragments, or null if not creating fragments</param>
        public XPathPropertyArrayItemGetter(EventPropertyGetter getter, int index, FragmentFactory fragmentFactory)
        {
            this.getter = getter;
            this.index = index;
            this.fragmentFactory = fragmentFactory;
        }

        #region EventPropertyGetter Members

        public Object Get(EventBean eventBean)
        {
            Object result = getter.Get(eventBean);
            if (result is XmlNodeList) {
                var nodeList = (XmlNodeList) result;
                if (nodeList.Count <= index) {
                    return null;
                }
                return nodeList.Item(index);
            }

            if (result is string) {
                var asString = (string) result;
                if (asString.Length <= index) {
                    return null;
                }

                return asString[index];
            }

            return null;
        }

        public Object GetFragment(EventBean eventBean)
        {
            if (fragmentFactory == null) {
                return null;
            }
            var result = (XmlNode) Get(eventBean);
            if (result == null) {
                return null;
            }
            return fragmentFactory.GetEvent(result);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        #endregion
    }
}
