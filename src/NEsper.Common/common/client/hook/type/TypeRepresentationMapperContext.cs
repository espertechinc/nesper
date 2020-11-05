///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    ///     For Avro customized type mapping, use with <seealso cref="TypeRepresentationMapper" />
    /// </summary>
    public class TypeRepresentationMapperContext
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="propertyName">property name</param>
        /// <param name="statementName">statement name</param>
        public TypeRepresentationMapperContext(
            Type clazz,
            string propertyName,
            string statementName)
        {
            Clazz = clazz;
            PropertyName = propertyName;
            StatementName = statementName;
        }

        /// <summary>
        ///     Returns the class.
        /// </summary>
        /// <returns>class</returns>
        public Type Clazz { get; }

        /// <summary>
        ///     Returns the property name
        /// </summary>
        /// <returns>property name</returns>
        public string PropertyName { get; }

        /// <summary>
        ///     Returns the statement name
        /// </summary>
        /// <returns>statement name</returns>
        public string StatementName { get; }
    }
} // end of namespace