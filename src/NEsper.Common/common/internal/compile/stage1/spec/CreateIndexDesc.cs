///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Specification for creating a named window. </summary>
    public class CreateIndexDesc
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isUnique">indicator whether unique or not</param>
        /// <param name="indexName">index name</param>
        /// <param name="windowName">window name</param>
        /// <param name="columns">properties to index</param>
        public CreateIndexDesc(
            bool isUnique,
            string indexName,
            string windowName,
            IList<CreateIndexItem> columns)
        {
            IsUnique = isUnique;
            IndexName = indexName;
            WindowName = windowName;
            Columns = columns;
        }

        /// <summary>Returns index name. </summary>
        /// <value>index name</value>
        public string IndexName { get; private set; }

        /// <summary>Returns window name. </summary>
        /// <value>window name</value>
        public string WindowName { get; private set; }

        /// <summary>Returns columns. </summary>
        /// <value>columns</value>
        public IList<CreateIndexItem> Columns { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unique.
        /// </summary>
        /// <value><c>true</c> if this instance is unique; otherwise, <c>false</c>.</value>
        public bool IsUnique { get; private set; }
    }
}