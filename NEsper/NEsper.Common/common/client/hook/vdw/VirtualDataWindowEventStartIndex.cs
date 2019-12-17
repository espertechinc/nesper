///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    ///     Event raised when an index gets created or started via the "create index" syntax.
    /// </summary>
    public class VirtualDataWindowEventStartIndex : VirtualDataWindowEvent
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="namedWindowName">named window name</param>
        /// <param name="indexName">index name</param>
        /// <param name="fields">index fields</param>
        /// <param name="unique">for unique indexes</param>
        public VirtualDataWindowEventStartIndex(
            string namedWindowName,
            string indexName,
            IList<VDWCreateIndexField> fields,
            bool unique)
        {
            NamedWindowName = namedWindowName;
            IndexName = indexName;
            Fields = fields;
            IsUnique = unique;
        }

        /// <summary>
        ///     Returns the index name.
        /// </summary>
        /// <returns>index name</returns>
        public string IndexName { get; }

        /// <summary>
        ///     Returns a list of fields that are part of the index.
        /// </summary>
        /// <returns>list of index fields</returns>
        public IList<VDWCreateIndexField> Fields { get; }

        /// <summary>
        ///     Returns the named window name.
        /// </summary>
        /// <returns>named window name</returns>
        public string NamedWindowName { get; }

        /// <summary>
        ///     Returns indictor for unique index
        /// </summary>
        /// <returns>unique index indicator</returns>
        public bool IsUnique { get; }

        /// <summary>
        ///     Captures virtual data window indexed field information.
        /// </summary>
        public class VDWCreateIndexField
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="name">field name</param>
            /// <param name="type">type</param>
            public VDWCreateIndexField(
                string name,
                string type)
            {
                Name = name;
                Type = type;
            }

            /// <summary>
            ///     Returns field name
            /// </summary>
            /// <returns>name</returns>
            public string Name { get; }

            /// <summary>
            ///     Returns index type
            /// </summary>
            /// <returns>type</returns>
            public string Type { get; }
        }
    }
} // end of namespace