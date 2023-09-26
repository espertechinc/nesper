// <summary>
// Copyright 2010 The Apache Software Foundation
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// <summary>
// Original file: org/apache/hadoop/hbase/util/MurmurHash.java
// </summary>

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// This is a very fast, non-cryptographic hash suitable for general hash-based
    /// lookup.  See http://murmurhash.googlepages.com/ for more details.
    /// <para />The C version of MurmurHash 2.0 found at that site was ported
    /// </summary>
    public class MurmurHash
    {
        public static int Hash(
            byte[] data,
            int offset,
            int length,
            int seed)
        {
            const int r = 24;

            uint m = 0x5bd1e995;
            var h = (uint)(seed ^ length);

            var len_4 = length >> 2;

            for (var i = 0; i < len_4; i++) {
                var i_4 = (uint)((i << 2) + offset);
                uint k = data[i_4 + 3];
                k = k << 8;
                k = k | (uint)(data[i_4 + 2] & 0xff);
                k = k << 8;
                k = k | (uint)(data[i_4 + 1] & 0xff);
                k = k << 8;
                //noinspection PointlessArithmeticExpression
                k = k | (uint)(data[i_4 + 0] & 0xff);
                k *= m;
                k ^= k >> r;
                k *= m;
                h *= m;
                h ^= k;
            }

            // avoid calculating modulo
            var len_m = len_4 << 2;
            var left = length - len_m;
            var i_m = len_m + offset;

            if (left != 0) {
                if (left >= 3) {
                    h ^= (uint)data[i_m + 2] << 16;
                }

                if (left >= 2) {
                    h ^= (uint)data[i_m + 1] << 8;
                }

                if (left >= 1) {
                    h ^= data[i_m];
                }

                h *= m;
            }

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return (int)h;
        }
    }
}