///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Repository for pluggable objects of different types that follow a "namespace:name" notation.
    /// </summary>
    public class PluggableObjectCollection
    {
        // Map of namespace, name and class plus type

        /// <summary>
        ///     Ctor.
        /// </summary>
        public PluggableObjectCollection()
        {
            Pluggables = new Dictionary<string, IDictionary<string, Pair<Type, PluggableObjectEntry>>>();
        }

        /// <summary>
        ///     Returns the underlying nested map of namespace keys and name-to-object maps.
        /// </summary>
        /// <value>pluggable object collected</value>
        public IDictionary<string, IDictionary<string, Pair<Type, PluggableObjectEntry>>> Pluggables { get; }

        /// <summary>
        ///     Add a plug-in view.
        /// </summary>
        /// <param name="configurationPlugInViews">is a list of configured plug-in view objects.</param>
        /// <param name="configurationPlugInVirtualDW">virtual data window configs</param>
        /// <param name="importService">imports</param>
        /// <throws>ConfigurationException if the configured views don't resolve</throws>
        public void AddViews(
            IList<ConfigurationCompilerPlugInView> configurationPlugInViews,
            IList<ConfigurationCompilerPlugInVirtualDataWindow> configurationPlugInVirtualDW,
            ImportServiceCompileTime importService)
        {
            InitViews(configurationPlugInViews, importService);
            InitVirtualDW(configurationPlugInVirtualDW, importService);
        }

        /// <summary>
        ///     Add a plug-in pattern object.
        /// </summary>
        /// <param name="configPattern">is a list of configured plug-in pattern objects.</param>
        /// <param name="importService">imports</param>
        /// <throws>ConfigurationException if the configured patterns don't resolve</throws>
        public void AddPatternObjects(
            IList<ConfigurationCompilerPlugInPatternObject> configPattern,
            ImportServiceCompileTime importService)
        {
            InitPatterns(configPattern, importService);
        }

        /// <summary>
        ///     Add the plug-in objects for another collection.
        /// </summary>
        /// <param name="other">is the collection to add</param>
        public void AddObjects(PluggableObjectCollection other)
        {
            foreach (var entry in other.Pluggables) {
                var namespaceMap = Pluggables.Get(entry.Key);
                if (namespaceMap == null) {
                    namespaceMap = new Dictionary<string, Pair<Type, PluggableObjectEntry>>();
                    Pluggables.Put(entry.Key, namespaceMap);
                }

                foreach (string name in entry.Value.Keys) {
                    if (namespaceMap.ContainsKey(name)) {
                        throw new ConfigurationException(
                            "Duplicate object detected in namespace '" +
                            entry.Key +
                            "' by name '" +
                            name +
                            "'");
                    }
                }

                namespaceMap.PutAll(entry.Value);
            }
        }

        /// <summary>
        ///     Add a single object to the collection.
        /// </summary>
        /// <param name="namespace">is the object's namespace</param>
        /// <param name="name">is the object's name</param>
        /// <param name="clazz">is the class the object resolves to</param>
        /// <param name="type">is the object type</param>
        public void AddObject(
            string @namespace,
            string name,
            Type clazz,
            PluggableObjectType type)
        {
            AddObject(@namespace, name, clazz, type, null);
        }

        /// <summary>
        ///     Add a single object to the collection also adding additional configuration.
        /// </summary>
        /// <param name="namespace">is the object's namespace</param>
        /// <param name="name">is the object's name</param>
        /// <param name="clazz">is the class the object resolves to</param>
        /// <param name="type">is the object type</param>
        /// <param name="configuration">config</param>
        public void AddObject(
            string @namespace,
            string name,
            Type clazz,
            PluggableObjectType type,
            object configuration)
        {
            SerializableExtensions.EnsureSerializable(configuration);

            var namespaceMap = Pluggables.Get(@namespace);
            if (namespaceMap == null) {
                namespaceMap = new Dictionary<string, Pair<Type, PluggableObjectEntry>>();
                Pluggables.Put(@namespace, namespaceMap);
            }

            namespaceMap.Put(
                name,
                new Pair<Type, PluggableObjectEntry>(clazz, new PluggableObjectEntry(type, configuration)));
        }

        private void InitViews(
            IList<ConfigurationCompilerPlugInView> configurationPlugInViews,
            ImportServiceCompileTime importService)
        {
            if (configurationPlugInViews == null) {
                return;
            }

            foreach (var entry in configurationPlugInViews) {
                HandleAddPluggableObject(
                    entry.ForgeClassName,
                    entry.Namespace,
                    entry.Name,
                    PluggableObjectType.VIEW,
                    null,
                    importService);
            }
        }

        private void InitVirtualDW(
            IList<ConfigurationCompilerPlugInVirtualDataWindow> configurationPlugInVirtualDataWindows,
            ImportServiceCompileTime importService)
        {
            if (configurationPlugInVirtualDataWindows == null) {
                return;
            }

            foreach (var entry in configurationPlugInVirtualDataWindows) {
                HandleAddPluggableObject(
                    entry.ForgeClassName,
                    entry.Namespace,
                    entry.Name,
                    PluggableObjectType.VIRTUALDW,
                    entry.Config,
                    importService);
            }
        }

        private void HandleAddPluggableObject(
            string factoryClassName,
            string @namespace,
            string name,
            PluggableObjectType type,
            object optionalCustomConfig,
            ImportServiceCompileTime importService)
        {
            SerializableExtensions.EnsureSerializable(optionalCustomConfig);

            if (factoryClassName == null) {
                throw new ConfigurationException("Factory class name has not been supplied for object '" + name + "'");
            }

            if (@namespace == null) {
                throw new ConfigurationException("Namespace name has not been supplied for object '" + name + "'");
            }

            if (name == null) {
                throw new ConfigurationException(
                    "Name has not been supplied for object in namespace '" + @namespace + "'");
            }

            Type clazz;
            try {
                clazz = importService.ClassForNameProvider.ClassForName(factoryClassName);
            }
            catch (TypeLoadException) {
                throw new ConfigurationException("View factory class " + factoryClassName + " could not be loaded");
            }

            var namespaceMap = Pluggables.Get(@namespace);
            if (namespaceMap == null) {
                namespaceMap = new Dictionary<string, Pair<Type, PluggableObjectEntry>>();
                Pluggables.Put(@namespace, namespaceMap);
            }

            namespaceMap.Put(
                name,
                new Pair<Type, PluggableObjectEntry>(clazz, new PluggableObjectEntry(type, optionalCustomConfig)));
        }

        private void InitPatterns(
            IList<ConfigurationCompilerPlugInPatternObject> configEntries,
            ImportServiceCompileTime importService)
        {
            if (configEntries == null) {
                return;
            }

            foreach (var entry in configEntries) {
                if (entry.PatternObjectType == null) {
                    throw new ConfigurationException(
                        "Pattern object type has not been supplied for object '" + entry.Name + "'");
                }

                PluggableObjectType typeEnum;
                if (entry.PatternObjectType == PatternObjectType.GUARD) {
                    typeEnum = PluggableObjectType.PATTERN_GUARD;
                }
                else if (entry.PatternObjectType == PatternObjectType.OBSERVER) {
                    typeEnum = PluggableObjectType.PATTERN_OBSERVER;
                }
                else {
                    throw new ArgumentException("Pattern object type '" + entry.PatternObjectType + "' not known");
                }

                HandleAddPluggableObject(
                    entry.ForgeClassName,
                    entry.Namespace,
                    entry.Name,
                    typeEnum,
                    null,
                    importService);
            }
        }
    }
} // end of namespace