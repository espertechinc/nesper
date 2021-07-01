///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.runtime.client.plugin;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportPluginLoader : PluginLoader
    {
        private Properties properties;

        public static IList<Properties> Props { get; } = new List<Properties>();

        public static IList<string> Names { get; } = new List<string>();

        public static IList<DateTimeEx> PostInitializes { get; } = new List<DateTimeEx>();

        public static IList<Properties> Destroys { get; } = new List<Properties>();

        public void PostInitialize()
        {
            PostInitializes.Add(DateTimeEx.NowLocal());
        }

        public void Init(PluginLoaderInitContext context)
        {
            Names.Add(context.Name);
            Props.Add(context.Properties);
            properties = context.Properties;
        }

        public void Dispose()
        {
            Destroys.Add(properties);
        }

        public static void Reset()
        {
            Names.Clear();
            Props.Clear();
            PostInitializes.Clear();
            Destroys.Clear();
        }
    }
} // end of namespace