///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// For Avro customized type mapping, use with <seealso cref="TypeRepresentationMapper" />
    /// </summary>
    public class TypeRepresentationMapperContext {
        private readonly Type clazz;
        private readonly string propertyName;
        private readonly string statementName;
        private readonly string engineURI;
    
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="propertyName">property name</param>
        /// <param name="statementName">statement name</param>
        /// <param name="engineURI">engine URI</param>
        public TypeRepresentationMapperContext(Type clazz, string propertyName, string statementName, string engineURI) {
            this.clazz = clazz;
            this.propertyName = propertyName;
            this.statementName = statementName;
            this.engineURI = engineURI;
        }
    
        /// <summary>
        /// Returns the class.
        /// </summary>
        /// <returns>class</returns>
        public Type Clazz {
            get => clazz;        }
    
        /// <summary>
        /// Returns the property name
        /// </summary>
        /// <returns>property name</returns>
        public string PropertyName {
            get => propertyName;        }
    
        /// <summary>
        /// Returns the statement name
        /// </summary>
        /// <returns>statement name</returns>
        public string StatementName {
            get => statementName;        }
    
        /// <summary>
        /// Returns the engine URI
        /// </summary>
        /// <returns>engine URI</returns>
        public string EngineURI {
            get => engineURI;        }
    }
} // end of namespace
