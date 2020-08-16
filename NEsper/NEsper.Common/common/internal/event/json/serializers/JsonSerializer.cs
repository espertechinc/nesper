///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json;

namespace com.espertech.esper.common.@internal.@event.json.serializers
{
    /// <summary>
    /// JsonSerializer is a delegate that given a writer and an object will
    /// serialize that object into that writer.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="und"></param>
    public delegate void JsonSerializer(
        Utf8JsonWriter writer,
        object und);
}