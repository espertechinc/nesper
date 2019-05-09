///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    [Serializable]
    public class AnnotationAttribute
    {
        private string name;
        private object value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationAttribute"/> class.
        /// </summary>
        public AnnotationAttribute()
        {
        }

        /// <summary>
        ///   <para>
        ///  Initializes a new instance of the <see cref="AnnotationAttribute"/> class.
        /// </para>
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public AnnotationAttribute(
            string name,
            object value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name {
            get => name;
            set => name = value;
        }

        public object Value {
            get => value;
            set => this.value = value;
        }
    }
}