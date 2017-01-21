///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.service
{
    /// <summary>Type of the statement. </summary>
    public enum StatementType
    {
        /// <summary>Pattern statement. </summary>
        PATTERN,
    
        /// <summary>Select statement that may contain one or more patterns. </summary>
        SELECT,
    
        /// <summary>Insert-into statement. </summary>
        INSERT_INTO,
    
        /// <summary>Create a named window statement. </summary>
        CREATE_WINDOW,
    
        /// <summary>Create a variable statement. </summary>
        CREATE_VARIABLE,

        /// <summary>Create a table statement. </summary>
        CREATE_TABLE,

        /// <summary>Create-schema statement. </summary>
        CREATE_SCHEMA,
    
        /// <summary>Create-index statement. </summary>
        CREATE_INDEX,
    
        /// <summary>Create-context statement. </summary>
        CREATE_CONTEXT,
    
        /// <summary>Create-graph statement. </summary>
        CREATE_DATAFLOW,
    
        /// <summary>Create-expression statement. </summary>
        CREATE_EXPRESSION,
    
        /// <summary>On-merge statement. </summary>
        ON_MERGE,
    
        /// <summary>On-merge statement. </summary>
        ON_SPLITSTREAM,
    
        /// <summary>On-delete statement. </summary>
        ON_DELETE,
    
        /// <summary>On-select statement. </summary>
        ON_SELECT,
    
        /// <summary>On-insert statement. </summary>
        ON_INSERT,
    
        /// <summary>On-set statement. </summary>
        ON_SET,
    
        /// <summary>On-Update statement. </summary>
        ON_UPDATE,
    
        /// <summary>Update statement. </summary>
        UPDATE,

        /// <summary>EsperIO </summary>
        ESPERIO
    }
}
