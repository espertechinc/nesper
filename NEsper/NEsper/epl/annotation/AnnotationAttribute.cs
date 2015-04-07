///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.annotation
{
    /// <summary>Represents a attribute of an annotation. </summary>
    public class AnnotationAttribute
    {
        private readonly Object _defaultValue;
        private readonly String _name;
        private readonly Type _type;

        /// <summary>Ctor. </summary>
        /// <param name="name">name of attribute</param>
        /// <param name="type">attribute type</param>
        /// <param name="defaultValue">default value, if any is specified</param>
        public AnnotationAttribute(String name,
                                   Type type,
                                   Object defaultValue)
        {
            _name = name;
            _type = type;
            _defaultValue = defaultValue;
        }

        /// <summary>Returns attribute name. </summary>
        /// <value>attribute name</value>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>Returns attribute type. </summary>
        /// <value>attribute type</value>
        public Type AnnotationType
        {
            get { return _type; }
        }

        /// <summary>Returns default value of annotation. </summary>
        /// <value>default value</value>
        public object DefaultValue
        {
            get { return _defaultValue; }
        }
    }
}