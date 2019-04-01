/*
 * CREDIT: Apache-Common-Text Version 1.2
 * Apache V2 licensed per https://commons.apache.org/proper/commons-text
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.IO;

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
	/// <summary>
	/// Helper subclass to CharSequenceTranslator to allow for translations that
	/// will replace up to one character at a time.
	/// </summary>
	/// <unknown>@since 1.0</unknown>
	public abstract class CodePointTranslator : CharSequenceTranslator {

	    /// <summary>
	    /// Implementation of translate that maps onto the abstract translate(int, Writer) method.
	    /// {@inheritDoc}
	    /// </summary>
	    public override int Translate(string input, int index, TextWriter @out) {
	        int codepoint = Character.CodePointAt(input, index);
	        bool consumed = Translate(codepoint, @out);
	        return consumed ? 1 : 0;
	    }

	    /// <summary>
	    /// Translate the specified codepoint into another.
	    /// </summary>
	    /// <param name="codepoint">int character input to translate</param>
	    /// <param name="out">Writer to optionally push the translated output to</param>
	    /// <returns>boolean as to whether translation occurred or not</returns>
	    /// <throws>IOException if and only if the Writer produces an IOException</throws>
	    public abstract bool Translate(int codepoint, TextWriter @out) ;

	}
} // end of namespace