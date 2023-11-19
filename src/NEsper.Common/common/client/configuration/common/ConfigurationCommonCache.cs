///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    /// Marker interface for different cache settings.
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<ConfigurationCommonCache>))]
    public interface ConfigurationCommonCache
    {
    }
} // end of namespace