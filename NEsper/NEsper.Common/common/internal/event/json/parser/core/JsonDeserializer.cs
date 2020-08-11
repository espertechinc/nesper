///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    /// <summary>
    /// Called to deserialize a JsonElement.
    /// </summary>
    /// <param name="element"></param>
    public delegate object JsonDeserializer(JsonElement element);
}