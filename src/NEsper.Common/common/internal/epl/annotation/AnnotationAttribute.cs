///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.annotation
{
    /// <summary>Represents a attribute of an annotation. </summary>
    public class AnnotationAttribute
    {
        private readonly string _name;
        private readonly Type _type;
        private readonly object _defaultValue;
        private readonly bool _isRequired;

        /// <summary>Ctor. </summary>
        /// <param name="name">name of attribute</param>
        /// <param name="type">attribute type</param>
        /// <param name="defaultValue">default value, if any is specified</param>
        /// <param name="isRequired">indicates if the value is required</param>
        public AnnotationAttribute(
            string name,
            Type type,
            object defaultValue,
            bool isRequired)
        {
            _name = name;
            _type = type;
            _defaultValue = defaultValue;
            _isRequired = isRequired;
        }

        /// <summary>Returns attribute name. </summary>
        /// <value>attribute name</value>
        public string Name => _name;

        /// <summary>Returns attribute type. </summary>
        /// <value>attribute type</value>
        public Type AnnotationType => _type;

        /// <summary>Returns default value of annotation. </summary>
        /// <value>default value</value>
        public object DefaultValue => _defaultValue;

        /// <summary>
        /// Returns true if this attribute value must be specified.
        /// </summary>
        public bool IsRequired => _isRequired;
    }
}