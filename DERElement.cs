namespace PFXParser
{
	public class DERElement
	{
		public Tag tag;
		public List<DERElement> children = new List<DERElement>();
		public ArraySegment<byte> data;

		public DERElement sub(int i) {
			return this.children[i];
		}

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
					var (new_end, child) = parse(raw.Slice(offset, end - offset));
					offset += new_end;
					result.children.Add(child);
				}
			}

			return (end, result);

		}

		public override string ToString()
		{
			return print(0);
		}

		public string print(int depth)
		{

			string output = new string(' ', 2 * depth)
							+ this.tag.ToString()
							+ " (" + this.data.Count + " bytes) : ";

			switch (this.tag)
			{
				case Tag.OBJECT_IDENTIFIER:
					output += print_identifier();
					break;

				case Tag.BOOLEAN:
					output += this.data[0] != 0;
					break;

				case Tag.IA5_STRING:
				case Tag.PRINTABLE_STRING:
					foreach (byte b in this.data) output += (char)b;
					break;

				case Tag.BIT_STRING:
					output += "(bin): ";
					for(int i = 0; i < this.data.Count; i++) {
						output += Convert.ToString(this.data[i], 2);
						if (i > 8) {
							output += "...";
							break;
						}
					}					break;

				case Tag.INTEGER:
					if (this.data.Count <= 4)
					{
						uint num = 0;
						foreach (byte b in this.data)
						{
							num <<= 8;
							num += b;
						}
						output += "(dec):" + num;
					}
					else
						output += "(hex): " + BitConverter.ToString(this.data.ToArray()).Remove('-');
					break;

				case Tag.UTCTime:
					output += "(YY-MM-DD hh:mm:ss) : ";
					for(int i = 0; i < 12; i++) {
						output += (char)this.data[i];
						switch(i) {
							case 1:
							case 3:
								output += '-';
								break;
							case 5:
								output += ' ';
								break;
							case 7:
							case 9:
								output += ':';
								break;
						}
					}
					output += " UTC";
					break;

				case Tag.CONTENT:
				case Tag.SET:
				case Tag.SEQUENCE:
					break;

				default:
					output += "(hex): ";
					for(int i = 0; i < this.data.Count; i++) {
						output += Convert.ToString(this.data[i], 16);
						if (i > 20) {
							output += "...";
							break;
						}
					}
					break;
			}

			foreach (var child in this.children) output += "\n" + child.print(depth + 1);

			return output;
		}

		public string print_identifier()
		{
			string output = this.data[0] / 40 + "." + this.data[0] % 40;

			for (int i = 1; i < this.data.Count; i++)
			{
				output += ".";

				byte next = this.data[i++];
				uint node = 0;
				while ((next & 0x80) != 0)
				{
					node += (uint)(next & ~0x80);
					node <<= 7;
					next = this.data[i++];
				}
				node += next;
				output += node;
				i--;
			}

			return output;
		}
	}

	public enum Tag : byte
	{
		EOC = 0x00,
		BOOLEAN = 0x01,
		INTEGER = 0x02,
		BIT_STRING = 0x03,
		OCTET_STRING = 0x04,
		NULL = 0x05,
		OBJECT_IDENTIFIER = 0x06,
		UTF8_STRING = 0x0C,
		PRINTABLE_STRING = 0x13,
		IA5_STRING = 0x16,
		UTCTime = 0x17,
		BMP_STRING = 0x1E,
		SEQUENCE = 0x30,
		SET = 0x31,
		CONTENT = 0xA0, // P7B Specific
	}
}
