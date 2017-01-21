///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

#region

using System;
using System.Xml;
using com.espertech.esper.client;
using com.espertech.esper.util;

#endregion

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for converting a Node child nodes into an array.
    /// </summary>
    public class DOMConvertingArrayGetter : EventPropertyGetter
    {
        private readonly Type componentType;
        private readonly DOMPropertyGetter getter;
        private readonly SimpleTypeParser parser;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="domPropertyGetter">getter</param>
        /// <param name="returnType">component type</param>
        public DOMConvertingArrayGetter(DOMPropertyGetter domPropertyGetter, Type returnType)
        {
            getter = domPropertyGetter;
            componentType = returnType;
            parser = SimpleTypeParserFactory.GetParser(returnType);
        }

        #region EventPropertyGetter Members

        public Object Get(EventBean eventBean)
        {
            var asXml = eventBean.Underlying as XmlNode;
            if (asXml == null) {
                throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                  "the underlying data object is not of type Node");
            }

            var result = getter.GetValueAsNodeArray(asXml);
            if (result == null) {
                return null;
            }

            var array = Array.CreateInstance(componentType, result.Length);
            for (int i = 0; i < result.Length; i++) {
                var text = result[i].InnerText;
                if (string.IsNullOrEmpty(text)) {
                    continue;
                }

                var parseResult = parser.Invoke(text);
                array.SetValue(parseResult, i);
            }

            return array;
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        #endregion
    }
}
