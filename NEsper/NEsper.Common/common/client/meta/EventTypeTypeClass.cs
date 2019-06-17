///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.meta
{
    /// <summary>
    ///     Metatype.
    /// </summary>
    public enum EventTypeTypeClass
    {
        /// <summary>
        ///     A type that represents the information made available via insert-into.
        /// </summary>
        STREAM,

        /// <summary>
        ///     A revision event type.
        /// </summary>
        REVISION,

        /// <summary>
        ///     A pattern-derived stream event type.
        /// </summary>
        PATTERNDERIVED,

        /// <summary>
        ///     A match-recognized-derived stream event type.
        /// </summary>
        MATCHRECOGDERIVED,

        /// <summary>
        ///     A variant stream event type.
        /// </summary>
        VARIANT,

        /// <summary>
        ///     An application-defined event type such as native, XML or Map.
        /// </summary>
        APPLICATION,

        /// <summary>
        ///     An application-defined event type such as native, XML or Map.
        /// </summary>
        STATEMENTOUT,

        /// <summary>
        ///     An derived-value-view-defined event type.
        /// </summary>
        VIEWDERIVED,

        /// <summary>
        ///     An enum-method derived event type.
        /// </summary>
        ENUMDERIVED,

        /// <summary>
        ///     A create-context for context properties event type.
        /// </summary>
        CONTEXTPROPDERIVED,

        /// <summary>
        ///     A bean-derived event type.
        /// </summary>
        BEAN_INCIDENTAL,

        /// <summary>
        ///     An UDF-method derived event type.
        /// </summary>
        UDFDERIVED,

        /// <summary>
        ///     A subquery-method derived event type.
        /// </summary>
        SUBQDERIVED,

        /// <summary>
        ///     A DB-access derived event type.
        /// </summary>
        DBDERIVED,

        /// <summary>
        ///     A Dataflow derived event type.
        /// </summary>
        DATAFLOWDERIVED,

        /// <summary>
        ///     A From-clause-method derived event type.
        /// </summary>
        METHODPOLLDERIVED,

        /// <summary>
        ///     A type representing a named window.
        /// </summary>
        NAMED_WINDOW,

        /// <summary>
        ///     A type representing a table.
        /// </summary>
        TABLE_PUBLIC,

        /// <summary>
        ///     A type for internal use with tables
        /// </summary>
        TABLE_INTERNAL,

        /// <summary>
        ///     An event type for exclude-plan evaluation.
        /// </summary>
        EXCLUDEPLANHINTDERIVED
    }
} // end of namespace