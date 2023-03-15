namespace PFXParser
{
	public enum Tag : byte
	{
		EOC                 = 0x00,
		BOOLEAN             = 0x01,
		INTEGER             = 0x02,
		BIT_STRING          = 0x03,
		OCTET_STRING        = 0x04,
		NULL                = 0x05,
		OBJECT_IDENTIFIER   = 0x06,
		UTF8_STRING         = 0x0C,
		SEQUENCE            = 0x30,
		SET                 = 0x31,
        

	}

	public class DERElement
	{
		private Tag tag;
		private List<DERElement> children = new List<DERElement>();
		private ArraySegment<byte> data;

		public static DERElement parse(ArraySegment<byte> raw)
		{
			int offset = 0;

			DERElement result = new DERElement();
			result.tag = (Tag)raw[offset++];

			byte len_byte = raw[offset++];
			int length = 0;

			if ((len_byte & 0xA0) == 0) length = (byte)len_byte;
			else
			{
				byte len_size = (byte)(len_byte & ~0xA0);
				if (len_size > 4)
				{
					//Panic
					throw new Exception("Object size too large");
				}
				else
				{
					for (uint i = 0; i < len_size; i++)
					{
						length <<= 8;
						length = (byte)raw[offset++];
					}
				}
			}

			int start = offset;
			int end = start + length;

			switch (result.tag)
			{
				case Tag.SET:
				case Tag.SEQUENCE:
					{
						while (offset < end)
						{
							DERElement child = parse(raw.Slice(offset, end));
							offset += child.data.Count;
							result.children.Add(child);
						}

					}
					break;

				default:

					break;
			}

			result.data = raw.Slice(start, end);
			return result;

		}

        public override string ToString() {

            string output = this.tag.ToString() + ": ";

            foreach (var child in children) output += "\n " + child.ToString();

            return output;
        }
	}
}
