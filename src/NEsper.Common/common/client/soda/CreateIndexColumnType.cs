///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Enumeration to represents the index type. </summary>
    public enum CreateIndexColumnType
    {
        /// <summary>Hash-index. </summary>
        HASH,

        /// <summary>Binary-tree (sorted) index. </summary>
        BTREE
    }
}