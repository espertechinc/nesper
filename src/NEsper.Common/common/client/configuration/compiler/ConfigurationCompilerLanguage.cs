///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Language settings in the runtime are for string comparisons.
    /// </summary>
    public class ConfigurationCompilerLanguage
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCompilerLanguage()
        {
            IsSortUsingCollator = false;
        }

        /// <summary>
        ///     Returns true to indicate to perform locale-independent string comparisons using Collator.
        ///     <para />
        ///     By default this setting is false, i.e. string comparisons use the compare method.
        /// </summary>
        /// <value>indicator whether to use Collator for string comparisons</value>
        public bool IsSortUsingCollator { get; private set; }

        /// <summary>
        ///     Set to true to indicate to perform locale-independent string comparisons using Collator.
        ///     <para />
        ///     Set to false to perform string comparisons via the compare method (the default).
        /// </summary>
        /// <value>indicator whether to use Collator for string comparisons</value>
        public bool SortUsingCollator {
            get => IsSortUsingCollator;
            set => IsSortUsingCollator = value;
        }
    }
} // end of namespace