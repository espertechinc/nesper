///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Text.Json;

namespace com.espertech.esper.common.@internal.util.serde
{
    /// <summary>
    /// Thread-local cache of a single default-options <see cref="Utf8JsonWriter" />,
    /// re-targeted via <see cref="Utf8JsonWriter.Reset(IBufferWriter{byte})" /> instead of
    /// allocating per serialization. Rent clears the slot so reentrant serialization
    /// falls back to a fresh writer rather than corrupting the one in use.
    /// </summary>
    internal static class Utf8JsonWriterPool
    {
        [ThreadStatic]
        private static Utf8JsonWriter _cachedWriter;

        public static Utf8JsonWriter Rent(IBufferWriter<byte> output)
        {
            var writer = _cachedWriter;
            if (writer == null) {
                return new Utf8JsonWriter(output);
            }

            _cachedWriter = null;
            writer.Reset(output);
            return writer;
        }

        public static void Return(Utf8JsonWriter writer)
        {
            _cachedWriter = writer;
        }
    }
}
