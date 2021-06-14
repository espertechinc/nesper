///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Describes an annotation.
    /// </summary>
    [Serializable]
    public class AnnotationDesc
    {
        // Map of Identifier and value={constant, array of value (Object[]), AnnotationDesc} (exclusive with value)

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">name of annotation</param>
        /// <param name="attributes">are the attribute values</param>
        public AnnotationDesc(
            string name,
            IList<Pair<string, object>> attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnnotationDesc" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public AnnotationDesc(
            string name,
            string value)
            : this(
                name,
                Collections.SingletonList(
                    new Pair<string, object>("Value", value)))
        {
        }

        /// <summary>
        ///     Returns annotation interface class name.
        /// </summary>
        /// <returns>
        ///     name of class, can be fully qualified
        /// </returns>
        public string Name { get; }

        /// <summary>
        ///     Returns annotation attributes.
        /// </summary>
        public IList<Pair<string, object>> Attributes { get; }
    }
}