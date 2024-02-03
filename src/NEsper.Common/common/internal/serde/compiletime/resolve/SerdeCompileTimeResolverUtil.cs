///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class SerdeCompileTimeResolverUtil
    {
        // Order of resolution:
        // (1) any serde providers are asked first, first one taking it counts
        // (2) class implements Externalizable (when allowed)
        // (3) class implements Serializable (when allowed)
        protected internal static SerdeProvision DetermineSerde(
            Type type,
            ICollection<SerdeProvider> serdeProviders,
            bool allowSerializable,
            bool allowExternalizable,
            bool allowSerializationFallback,
            SerdeProviderAdditionalInfo additionalInfo)
        {
            SerdeProvision serde;
            if (!serdeProviders.IsEmpty()) {
                serde = DetermineSerdeFromProviders(type, serdeProviders, additionalInfo);
                if (serde != null) {
                    return serde;
                }

                if (type.IsArray) {
                    Type componentType = type.GetComponentType();
                    var componentSerde = DetermineSerdeFromProviders(componentType, serdeProviders, additionalInfo);
                    if (componentSerde != null) {
                        return new SerdeProvisionArrayOfNonPrimitive(componentType, componentSerde);
                    }
                }
            }

            serde = DetermineSerializable(type, allowExternalizable, allowSerializable, allowSerializationFallback);
            if (serde != null) {
                return serde;
            }

            throw MakeFailedToFindException(
                type,
                allowExternalizable,
                allowSerializable,
                serdeProviders.Count,
                additionalInfo);
        }

        private static SerdeProvision DetermineSerdeFromProviders(
            Type type,
            ICollection<SerdeProvider> serdeProviders,
            SerdeProviderAdditionalInfo additionalInfo)
        {
            if (serdeProviders.IsEmpty()) {
                return null;
            }

            var context = new SerdeProviderContextClass(type, additionalInfo);
            foreach (var provider in serdeProviders) {
                try {
                    var serde = provider.ResolveSerdeForClass(context);
                    if (serde != null) {
                        return serde;
                    }
                }
                catch (DataInputOutputSerdeException) {
                    throw;
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    throw HandleProviderRuntimeException(provider, type, ex);
                }
            }

            return null;
        }

        private static SerdeProvision DetermineSerializable(
            Type type,
            bool allowExternalizable,
            bool allowSerializable,
            bool allowSerializationFallback)
        {
            if (allowSerializationFallback) {
                return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
            }

            if (type.IsSerializable && allowSerializable) {
                return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
            }

            if (type.IsArray && type.GetElementType().IsSerializable && allowSerializable) {
                return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
            }

            return null;
        }

        private static DataInputOutputSerdeException HandleProviderRuntimeException(
            SerdeProvider provider,
            Type type,
            Exception ex)
        {
            return new DataInputOutputSerdeException(
                $"Unexpected exception invoking serde provider '{provider.GetType().Name}' passing '{type}': {ex.Message}",
                ex);
        }

        private static DataInputOutputSerdeException MakeFailedToFindException(
            Type clazz,
            bool allowExternalizable,
            bool allowSerializable,
            int numSerdeProviders,
            SerdeProviderAdditionalInfo additionalInfo)
        {
            return new DataInputOutputSerdeException(
                $"Failed to find serde for class '{compat.TypeExtensions.CleanName(clazz)}' for use with {additionalInfo} (allowExternalizable={allowExternalizable},allowSerializable={allowSerializable},serdeProvider-count={numSerdeProviders})");
        }
    }
} // end of namespace