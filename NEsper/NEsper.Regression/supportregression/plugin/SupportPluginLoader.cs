///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.plugin;



namespace com.espertech.esper.supportregression.plugin
{
    public class SupportPluginLoader : PluginLoader
    {
        internal static readonly List<String> Names = new List<String>();
        internal static readonly List<Properties> Props = new List<Properties>();
        internal static readonly List<DateTime> PostInitializes = new List<DateTime>();
        internal static readonly List<Properties> Destroys = new List<Properties>();
    
        private Properties _properties;
    
        public static void Reset()
        {
            Names.Clear();
            Props.Clear();
            PostInitializes.Clear();
            Destroys.Clear();        
        }
    
        public static List<Properties> GetProps()
        {
            return Props;
        }
    
        public static List<String> GetNames()
        {
            return Names;
        }
    
        public static List<DateTime> GetPostInitializes()
        {
            return PostInitializes;
        }
    
        public static List<Properties> GetDestroys()
        {
            return Destroys;
        }
    
        public void PostInitialize()
        {
            PostInitializes.Add(new DateTime());
        }
    
        public void Dispose()
        {
            Destroys.Add(_properties);
        }
    
        public void Init(PluginLoaderInitContext context)
        {
            Names.Add(context.Name);
            Props.Add(context.Properties);
            _properties = context.Properties;
        }
    }
}
