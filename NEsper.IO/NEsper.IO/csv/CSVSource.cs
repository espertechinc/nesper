using System.IO;

using com.espertech.esper.compat;

namespace com.espertech.esperio.csv
{
	/// <summary>
	/// A wrapper for a Stream or a TextReader.
	/// </summary>

	public class CSVSource
	{
		private readonly AdapterInputSource source; // source of data
		private TextReader reader; // reader from where data comes
		private Stream stream;     // stream from where data comes

	    private int eMarkIndex; // end index
	    private int wMarkIndex; // write index
	    private int rMarkIndex; // read index
	    private int[] markData; // backing store for reading data when marked

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="source">the AdapterInputSource from which to obtain the underlying resource</param>

		public CSVSource(AdapterInputSource source)
		{
            rMarkIndex = -1; // not reading
            wMarkIndex = -1; // not writing
            eMarkIndex = -1;
            markData = new int[2];

			stream = source.GetAsStream() ;
			if ( stream == null )
			{
				reader = source.GetAsReader() ;
			}

			this.source = source;
		}
		
		/// <summary>Close the underlying resource.</summary>
		/// <throws>IOException to indicate an io error</throws>

		public void Close()
		{
			if (stream != null)
			{
				stream.Close();
			}
			else
			{
				reader.Close();
			}
		}

        private int ReadFromBase()
        {
            if (stream != null)
            {
                return stream.ReadByte();
            }
            else
            {
                return reader.Read();
            }
        }

        private void WriteToMark(int value)
        {
            // Do we need to store the value for a future read?
            if (wMarkIndex >= 0)
            {
                if (wMarkIndex < markData.Length)
                {
                    markData[wMarkIndex] = value; // Write the value to buffer
                    wMarkIndex++; // Increment the write mark
                }
            }
        }

        private void IncrementWriteMark()
        {
            // Do we need to store the value for a future read?
            if (wMarkIndex >= 0)
            {
                if (wMarkIndex < markData.Length)
                {
                    wMarkIndex++;
                }
            }
        }

	    /// <summary>Read from the underlying resource.</summary>
		/// <returns>the result of the read</returns>
		/// <throws>IOException for io errors</throws>

        public int Read()
		{
            int value;

            // Check to see if we are supposed to be reading from
            // somewhere within the markData.  If we are, make sure
            // that we return data from the markData buffer.
		    if (rMarkIndex >= 0)
		    {
		        int index = rMarkIndex++;
		        if (eMarkIndex > index)
		        {
		            value = markData[index];
		            IncrementWriteMark();
		        }
                else
		        {
		            value = ReadFromBase();

                    // Can this value be written to the pushback buffer or have
                    // we consumed more data that was specified for the lookahead?

                    if (wMarkIndex < markData.Length)
                    {
                        // More space is available in the pushback buffer
                        WriteToMark(value);
                    }
                    else
                    {
                        // Once we have to read past the end of the marked data, then
                        // the marked data is no longer retrievable because we can not
                        // recreate the character that has been consumed off the wire.

                        eMarkIndex = -1;
                        wMarkIndex = -1;
                        rMarkIndex = -1;
                    }
		        }
            }
            // We are not reading from the mark buffer which means we
            // need to read one value.  If we are marking, then we need
            // to write the data into the markData buffer.
            else
            {
                value = ReadFromBase();
                WriteToMark(value);
            }
            
		    return value;
		}

	    /// <summary>Return true if the underlying resource is resettable.</summary>
		/// <returns>true if resettable, false otherwise</returns>

		public bool IsResettable
		{
			get { return source.IsResettable; }
		}
		
		/// <summary>Reset to the last mark position.</summary>
		/// <throws>IOException for io errors</throws>

		public void ResetToMark()
		{
		    rMarkIndex = 0;
		    eMarkIndex =
		        eMarkIndex > wMarkIndex
		            ? eMarkIndex
		            : wMarkIndex;

		    wMarkIndex = 0;
		}

        /// <summary>Set the mark position.</summary>
		/// <param name="readAheadLimit">is the maximum number of read-ahead events</param>
		/// <throws>IOException when an io error occurs</throws>

		public void Mark(int readAheadLimit)
		{
            // Set a new mark ...
            // - Have we consumed data that is currently in the pushback buffer?

            switch( rMarkIndex )
            {
                case -1:
                case 0:
                    wMarkIndex = 0;
                    break;
                case 1:
                    markData[0] = markData[1];
                    markData[1] = 0;
                    rMarkIndex = 0;
                    eMarkIndex--;
                    wMarkIndex = 0;
                    break;
                case 2:
                    rMarkIndex = -1;
                    wMarkIndex = 0;
                    eMarkIndex = 0;
                    break;
            }
		}
		
		/// <summary>Reset to the beginning of the resource.</summary>

		public void Reset()
		{
			if(!IsResettable)
			{
				throw new UnsupportedOperationException("Reset not supported: underlying source cannot be reset");
			}
			
			if(stream != null)
			{
				stream = source.GetAsStream();
			}
			else
			{
				reader = source.GetAsReader();
			}

		    rMarkIndex = -1;
		    wMarkIndex = -1;
		    eMarkIndex = -1;
		    markData[0] = 0;
		    markData[1] = 0;
		}
	}
}
