///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Controls how output column names get reflected in the event type.
    /// </summary>
    public enum ColumnChangeCaseEnum
    {
        /// <summary>
        ///     Leave the column names the way the database driver represents the column.
        /// </summary>
        NONE,

        /// <summary>
        ///     Change case to lowercase on any column names returned by statement metadata.
        /// </summary>
        LOWERCASE,

        /// <summary>
        ///     Change case to uppercase on any column names returned by statement metadata.
        /// </summary>
        UPPERCASE
    }
}