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
using com.espertech.esper.util;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Getter for parsing node content to a desired type.
    /// </summary>
    public class DOMConvertingGetter : EventPropertyGetter
    {
        private readonly DOMPropertyGetter _getter;
        private readonly SimpleTypeParser _parser;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyExpression">property name</param>
        /// <param name="domPropertyGetter">getter</param>
        /// <param name="returnType">desired result type</param>
        public DOMConvertingGetter(String propertyExpression, DOMPropertyGetter domPropertyGetter, Type returnType)
        {
            _getter = domPropertyGetter;
            _parser = SimpleTypeParserFactory.GetParser(returnType);
        }
    
        public Object Get(EventBean eventBean)
        {
            var asXNode = eventBean.Underlying as XNode;
            if (asXNode == null) {
                var asXml = eventBean.Underlying as XmlNode;
                if (asXml == null)
                {
                    throw new PropertyAccessException("Mismatched property getter to event bean type, " +
                                                      "the underlying data object is not of type Node");
                }

                XmlNode result = _getter.GetValueAsNode(asXml);
                if (result == null)
                {
                    return null;
                }

                return _parser.Invoke(result.InnerText);
            }
            else
            {
                XObject result = _getter.GetValueAsNode(asXNode);
                if (result == null)
                {
                    return null;
                }

                if (result is XElement) {
                    return _parser.Invoke(((XElement) result).Value);
                }

                return _parser.Invoke(result.ToString());
            }
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
