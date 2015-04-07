///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.hook
{
    /// <summary>Event raised when an index gets created or started via the "create index" syntax. </summary>
    public class VirtualDataWindowEventStartIndex : VirtualDataWindowEvent
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="namedWindowName">named window name</param>
        /// <param name="indexName">index name</param>
        /// <param name="fields">index fields</param>
        /// <param name="isUnique">if set to <c>true</c> [is unique].</param>
        public VirtualDataWindowEventStartIndex(String namedWindowName,
                                                String indexName,
                                                IList<VDWCreateIndexField> fields,
                                                bool isUnique)
        {
            NamedWindowName = namedWindowName;
            IndexName = indexName;
            Fields = fields;
            IsUnique = isUnique;
        }

        /// <summary>Returns the index name. </summary>
        /// <value>index name</value>
        public string IndexName { get; private set; }

        /// <summary>Returns a list of fields that are part of the index. </summary>
        /// <value>list of index fields</value>
        public IList<VDWCreateIndexField> Fields { get; private set; }

        /// <summary>Returns the named window name. </summary>
        /// <value>named window name</value>
        public string NamedWindowName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is unique.
        /// </summary>
        public bool IsUnique { get; private set; }


        #region Nested type: VDWCreateIndexField

        /// <summary>Captures virtual data window indexed field informaion. </summary>
        public class VDWCreateIndexField
        {
            /// <summary>Ctor. </summary>
            /// <param name="name">named window name</param>
            /// <param name="hash">true for hash-based index, false for btree index</param>
            public VDWCreateIndexField(String name,
                                       bool hash)
            {
                Name = name;
                IsHash = hash;
            }

            /// <summary>Name of the indexed field. </summary>
            /// <value>field name</value>
            public string Name { get; private set; }

            /// <summary>Indicate whether the index is hash or btree, true for hash. </summary>
            /// <value>index type indicator</value>
            public bool IsHash { get; private set; }
        }

        #endregion
    }
}