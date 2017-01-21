///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represents a single annotation attribute, the value of which may itself be a single
    /// value, array or further annotations. 
    /// </summary>
    [Serializable]
    public class AnnotationAttribute
    {
        /// <summary>Ctor. </summary>
        public AnnotationAttribute() {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="name">annotation name</param>
        /// <param name="value">annotation value, could be a primitive, array or another annotation</param>
        public AnnotationAttribute(String name, Object value) {
            Name = name;
            Value = value;
        }

        /// <summary>Returns annotation name. </summary>
        /// <returns>name</returns>
        public string Name { get; set; }

        /// <summary>Returns annotation value. </summary>
        /// <returns>value</returns>
        public object Value { get; set; }
    }
}
