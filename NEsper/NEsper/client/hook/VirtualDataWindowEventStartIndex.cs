///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
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
            /// <summary>
            /// Initializes a new instance of the <see cref="VDWCreateIndexField"/> class.
            /// </summary>
            /// <param name="expressions">The expressions.</param>
            /// <param name="type">The type.</param>
            /// <param name="parameters">The parameters.</param>
            public VDWCreateIndexField(IList<ExprNode> expressions, String type, IList<ExprNode> parameters)
            {
                Expressions = expressions;
                Type = type;
                Parameters = parameters;
            }

            /// <summary>
            /// Gets the index expressions.
            /// </summary>
            public IList<ExprNode> Expressions { get; private set; }
            
            /// <summary>
            /// Gets the index type.
            /// </summary>
            public String Type { get; private set; }

            /// <summary>
            /// Gets the index field parameters if any.
            /// </summary>
            public IList<ExprNode> Parameters { get; private set; }
        }

        #endregion
    }
}