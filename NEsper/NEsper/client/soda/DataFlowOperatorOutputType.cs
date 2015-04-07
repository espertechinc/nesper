///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.soda
{
    /// <summary>Represents type information for data flow operators. </summary>
    [Serializable]
    public class DataFlowOperatorOutputType
    {
        /// <summary>Ctor. </summary>
        public DataFlowOperatorOutputType()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="wildcard">true for wildcard type</param>
        /// <param name="typeOrClassname">type name</param>
        /// <param name="typeParameters">optional additional type parameters</param>
        public DataFlowOperatorOutputType(bool wildcard,
                                          String typeOrClassname,
                                          IList<DataFlowOperatorOutputType> typeParameters)
        {
            IsWildcard = wildcard;
            TypeOrClassname = typeOrClassname;
            TypeParameters = typeParameters;
        }

        /// <summary>Returns true for wildcard type. </summary>
        /// <value>wildcard type indicator</value>
        public bool IsWildcard { get; set; }

        /// <summary>Returns the type name or class name. </summary>
        /// <value>name</value>
        public string TypeOrClassname { get; set; }

        /// <summary>Returns optional additional type parameters </summary>
        /// <value>type params</value>
        public IList<DataFlowOperatorOutputType> TypeParameters { get; set; }
    }
}