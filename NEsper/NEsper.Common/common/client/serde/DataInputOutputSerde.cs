///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.client.serde
{
	public interface DataInputOutputSerde
	{
		/// <summary>
		/// Write an object to the stream.
		/// </summary>
		/// <param name="object">to write or null if this is a nullable value</param>
		/// <param name="output">to write to</param>
		/// <param name="unitKey">the page key of the page containing the object, can be null if not relevant or not provided</param>
		/// <param name="writer">the writer for events, can be null if not relevant or not provided</param>
		/// <throws>IOException for io exceptions</throws>
		void Write(
			object @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer);

		/// <summary>
		/// Read an object from the stream.
		/// </summary>
		/// <param name="input">input to read</param>
		/// <param name="unitKey">the identifying key of the reader, can be null if not relevant or not provided</param>
		/// <returns>object read or null if this is a nullable value</returns>
		/// <throws>IOException for io exceptions</throws>
		object Read(
			DataInput input,
			byte[] unitKey);
	}
	
	/// <summary>
	/// Implementations read and write objects from/to the stream.
	/// </summary>
	public interface DataInputOutputSerde<TE> : DataInputOutputSerde
	{
		/// <summary>
		/// Write an object to the stream.
		/// </summary>
		/// <param name="object">to write or null if this is a nullable value</param>
		/// <param name="output">to write to</param>
		/// <param name="unitKey">the page key of the page containing the object, can be null if not relevant or not provided</param>
		/// <param name="writer">the writer for events, can be null if not relevant or not provided</param>
		/// <throws>IOException for io exceptions</throws>
		void Write(
			TE @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer);

		/// <summary>
		/// Read an object from the stream.
		/// </summary>
		/// <param name="input">input to read</param>
		/// <param name="unitKey">the identifying key of the reader, can be null if not relevant or not provided</param>
		/// <returns>object read or null if this is a nullable value</returns>
		/// <throws>IOException for io exceptions</throws>
		TE ReadValue(
			DataInput input,
			byte[] unitKey);
	}
} // end of namespace
