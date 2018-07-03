///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Repository for pluggable objects of different types that follow a "namespace:name" notation.
    /// </summary>
    public class PluggableObjectCollection
    {
        /// <summary>
        /// The underlying nested map of namespace keys and name-to-object maps.
        /// </summary>
        /// <value>pluggable object collected</value>
        public IDictionary<string, IDictionary<string, Pair<Type, PluggableObjectEntry>>> Pluggables { get; private set; }

        /// <summary>Ctor. </summary>
        public PluggableObjectCollection()
        {
            Pluggables = new Dictionary<String, IDictionary<String, Pair<Type, PluggableObjectEntry>>>();
        }

        /// <summary>
        /// Add a plug-in view.
        /// </summary>
        /// <param name="configurationPlugInViews">is a list of configured plug-in view objects.</param>
        /// <param name="configurationPlugInVirtualDW">The configuration plug in virtual DW.</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <throws>ConfigurationException if the configured views don't resolve</throws>
        public void AddViews(
            IList<ConfigurationPlugInView> configurationPlugInViews,
            IList<ConfigurationPlugInVirtualDataWindow> configurationPlugInVirtualDW,
            EngineImportService engineImportService)
        {
            InitViews(configurationPlugInViews, engineImportService);
            InitVirtualDW(configurationPlugInVirtualDW, engineImportService);
        }

        /// <summary>
        /// Add a plug-in pattern object.
        /// </summary>
        /// <param name="configPattern">is a list of configured plug-in pattern objects.</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <throws>ConfigurationException if the configured patterns don't resolve</throws>
        public void AddPatternObjects(
            IList<ConfigurationPlugInPatternObject> configPattern,
            EngineImportService engineImportService)
        {
            InitPatterns(configPattern, engineImportService);
        }

        /// <summary>Add the plug-in objects for another collection. </summary>
        /// <param name="other">is the collection to add</param>
        public void AddObjects(PluggableObjectCollection other)
        {
            foreach (KeyValuePair<String, IDictionary<String, Pair<Type, PluggableObjectEntry>>> entry in other.Pluggables)
            {
                var namespaceMap = Pluggables.Get(entry.Key);
                if (namespaceMap == null)
                {
                    namespaceMap = new Dictionary<String, Pair<Type, PluggableObjectEntry>>();
                    Pluggables.Put(entry.Key, namespaceMap);
                }

                foreach (var name in entry.Value.Keys)
                {
                    if (namespaceMap.ContainsKey(name))
                    {
                        throw new ConfigurationException("Duplicate object detected in namespace '" + entry.Key +
                                    "' by name '" + name + "'");
                    }
                }

                namespaceMap.PutAll(entry.Value);
            }
        }

        /// <summary>Add a single object to the collection. </summary>
        /// <param name="namespace">is the object's namespace</param>
        /// <param name="name">is the object's name</param>
        /// <param name="clazz">is the class the object resolves to</param>
        /// <param name="type">is the object type</param>
        public void AddObject(String @namespace, String name, Type clazz, PluggableObjectType type)
        {
            AddObject(@namespace, name, clazz, type, null);
        }

        /// <summary>
        /// Add a single object to the collection also adding additional configuration.
        /// </summary>
        /// <param name="namespace">is the object's namespace</param>
        /// <param name="name">is the object's name</param>
        /// <param name="clazz">is the class the object resolves to</param>
        /// <param name="type">is the object type</param>
        /// <param name="configuration">The configuration.</param>
        public void AddObject(String @namespace, String name, Type clazz, PluggableObjectType type, Object configuration)
        {
            var namespaceMap = Pluggables.Get(@namespace);
            if (namespaceMap == null)
            {
                namespaceMap = new Dictionary<String, Pair<Type, PluggableObjectEntry>>();
                Pluggables.Put(@namespace, namespaceMap);
            }
            namespaceMap.Put(name, new Pair<Type, PluggableObjectEntry>(clazz, new PluggableObjectEntry(type, configuration)));
        }

        private void InitViews(IEnumerable<ConfigurationPlugInView> configurationPlugInViews, EngineImportService engineImportService)
        {
            if (configurationPlugInViews == null)
            {
                return;
            }

            foreach (var entry in configurationPlugInViews)
            {
                HandleAddPluggableObject(entry.FactoryClassName, entry.Namespace, entry.Name, PluggableObjectType.VIEW, null, engineImportService);
            }
        }

        private void InitVirtualDW(
            IEnumerable<ConfigurationPlugInVirtualDataWindow> configurationPlugInVirtualDataWindows,
            EngineImportService engineImportService)
        {
            if (configurationPlugInVirtualDataWindows == null)
            {
                return;
            }

            foreach (var entry in configurationPlugInVirtualDataWindows)
            {
                HandleAddPluggableObject(entry.FactoryClassName, entry.Namespace, entry.Name, PluggableObjectType.VIRTUALDW, entry.Config, engineImportService);
            }
        }

        private void HandleAddPluggableObject(
            String factoryClassName,
            String @namespace,
            String name,
            PluggableObjectType type,
            Object optionalCustomConfig,
            EngineImportService engineImportService)
        {
            if (factoryClassName == null)
            {
                throw new ConfigurationException("Factory class name has not been supplied for object '" + name + "'");
            }
            if (@namespace == null)
            {
                throw new ConfigurationException("Namespace name has not been supplied for object '" + name + "'");
            }
            if (name == null)
            {
                throw new ConfigurationException("Name has not been supplied for object in namespace '" + @namespace + "'");
            }

            try
            {
                var clazz = engineImportService.GetClassForNameProvider().ClassForName(factoryClassName);

                var namespaceMap = Pluggables.Get(@namespace);
                if (namespaceMap == null)
                {
                    namespaceMap = new Dictionary<String, Pair<Type, PluggableObjectEntry>>();
                    Pluggables.Put(@namespace, namespaceMap);
                }
                namespaceMap.Put(
                    name,
                    new Pair<Type, PluggableObjectEntry>(clazz, new PluggableObjectEntry(type, optionalCustomConfig)));
            }
            catch (TypeLoadException e)
            {
                throw new ConfigurationException("View factory class " + factoryClassName + " could not be loaded", e);
            }
        }

        private void InitPatterns(IEnumerable<ConfigurationPlugInPatternObject> configEntries, EngineImportService engineImportService)
        {
            if (configEntries == null)
            {
                return;
            }

            foreach (var entry in configEntries)
            {
                if (entry.PatternObjectType == null)
                {
                    throw new ConfigurationException("Pattern object type has not been supplied for object '" + entry.Name + "'");
                }

                PluggableObjectType typeEnum;
                if (entry.PatternObjectType == ConfigurationPlugInPatternObject.PatternObjectTypeEnum.GUARD)
                {
                    typeEnum = PluggableObjectType.PATTERN_GUARD;
                }
                else if (entry.PatternObjectType == ConfigurationPlugInPatternObject.PatternObjectTypeEnum.OBSERVER)
                {
                    typeEnum = PluggableObjectType.PATTERN_OBSERVER;
                }
                else
                {
                    throw new ArgumentException("Pattern object type '" + entry.PatternObjectType + "' not known");
                }

                HandleAddPluggableObject(entry.FactoryClassName, entry.Namespace, entry.Name, typeEnum, null, engineImportService);
            }
        }

    }
}
