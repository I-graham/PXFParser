namespace PFXParser
{
	public class DERElement
	{
		private Tag tag;
		private List<DERElement> children = new List<DERElement>();
		private ArraySegment<byte> data;

		public static (int, DERElement) parse(ArraySegment<byte> raw)
		{
			int offset = 0;

			DERElement result = new DERElement();
			result.tag = (Tag)raw[offset++];

			byte len_byte = raw[offset++];
			int length = 0;

			if ((len_byte & 0x80) == 0) length = (byte)len_byte;
			else
			{
				byte len_size = (byte)(len_byte & ~0x80);
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
						length += raw[offset++];
					}
				}
			}

			int start = offset;
			int end = start + length;
			result.data = raw.Slice(start, length);

			if (result.tag == Tag.SET
				|| result.tag == Tag.SEQUENCE
				|| ((0x20) & (int)(result.tag)) != 0)
			{
				while (offset < end)
				{
					var (new_end, child) = parse(raw.Slice(offset, end-offset));
					offset += new_end;
					result.children.Add(child);
				}
			}
				

			return (end, result);

		}

        public override string ToString() {

            string output = this.tag.ToString() + ": " + this.data;

			switch(this.tag) {
				default:
					foreach (var child in children) output += "\n " + child.ToString();
					break;
			}

            return output;
        }
	}

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
		PRINTABLE_STRING	= 0x13,
		IA5_STRING			= 0x16,
		BMP_STRING			= 0x1E,
		SEQUENCE            = 0x30,
		SET                 = 0x31,
	}
}
