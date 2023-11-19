///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Marker interface used for fire-and-forget (on-demand) queries such as "Update...set"
    /// and "delete from..." that can be executed via the API.
    /// </summary>
    [JsonConverter(typeof(JsonConverterAbstract<FireAndForgetClause>))]
    public interface FireAndForgetClause
    {
    }
}