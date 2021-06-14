///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.db
{
    /// <summary>
    ///     Hold a raw SQL-statements parameter information that were specified in the form ${name}.
    /// </summary>
    public class SQLParameterDesc
    {
        /// <summary>Ctor.</summary>
        /// <param name="parameters">is the name of parameters</param>
        /// <param name="builtinIdentifiers">is the names of built-in predefined values</param>
        public SQLParameterDesc(
            IList<string> parameters,
            IList<string> builtinIdentifiers)
        {
            Parameters = parameters;
            BuiltinIdentifiers = builtinIdentifiers;
        }

        /// <summary>Returns parameter names.</summary>
        /// <returns>parameter names</returns>
        public IList<string> Parameters { get; }

        /// <summary>Returns built-in identifiers.</summary>
        /// <returns>built-in identifiers</returns>
        public IList<string> BuiltinIdentifiers { get; }

        public override string ToString()
        {
            return string.Format(
                "params={0} builtin={1}",
                CompatExtensions.RenderAny(Parameters),
                CompatExtensions.RenderAny(BuiltinIdentifiers));
        }
    }
} // End of namespace