///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Holds default settings for stream selection in the select-clause.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerStreamSelection
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        public ConfigurationCompilerStreamSelection()
        {
            DefaultStreamSelector = StreamSelector.ISTREAM_ONLY;
        }

        /// <summary>
        ///     Returns the default stream selector.
        ///     <para />
        ///     Block that select data from streams and that do not use one of the explicit stream
        ///     selection keywords (istream/rstream/irstream), by default,
        ///     generate selection results for the insert stream only, and not for the remove stream.
        ///     <para />
        ///     This setting can be used to change the default behavior: Use the RSTREAM_ISTREAM_BOTH
        ///     value to have your statements generate both insert and remove stream results
        ///     without the use of the "irstream" keyword in the select clause.
        /// </summary>
        /// <value>default stream selector, which is ISTREAM_ONLY unless changed</value>
        public StreamSelector DefaultStreamSelector { get; set; }
    }
} // end of namespace