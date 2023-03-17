using static PFXParser.Program;

namespace PFXParser
{
	public class X509 {
		private DERElement inner;
		string sig_alg;
		string serial;
		int version;
		string validity_start;
		string validity_end;
		string issuer_name;
		string subject_name;

		string public_key;
		string key_alg;

		public X509(DERElement certificate) {
			inner = certificate;

			version = inner.sub(0).sub(0).data[0];
			serial = inner.sub(0).sub(1).ToString();

			sig_alg = inner.sub(1).sub(0).print_identifier();
			
			var validity = inner.sub(0).sub(4);
			validity_start = validity.sub(0).ToString(); 
			validity_end = validity.sub(1).ToString();

			var names = inner.sub(0).sub(3).sub(0).children;
			var name_node = names.Find(node => node.sub(0).print_identifier().Equals("2.5.4.3"));

			issuer_name = name_node.sub(1).ToString();

			subject_name = inner.sub(0).sub(5).sub(0).sub(0).sub(1).ToString();
						
			key_alg = inner.sub(0).sub(6).sub(0).sub(0).print_identifier();
			public_key = inner.sub(0).sub(6).sub(1).ToString();

		}

		public static X509[] parse(string filename) {
			var der = DERElement.parse(read("test.p7b")).Item2;
			
			var certificateDERs = der.sub(1).sub(0).sub(3).children;

			int length = certificateDERs.Count;
			var certificates = new X509[length];

			for(int i = 0; i < length; i++) {
				certificates[i] = new X509(certificateDERs[i]);
			}

			return certificates;
		}

		public override string ToString() {
			string output = "";

			output += "Raw DER data: " + inner + "\n\n";

			if (this.inner.children[0].print_identifier().Equals("1.2.840.113549.1.7.2")) {
				output += "P7B Certificate: \n";
			}

			output += "Version: " + version + '\n';
			output += "Signature Algorithm: " + sig_alg + '\n';
			output += "Serial Number: " + serial + '\n';
			output += "Not Before: " + validity_start + '\n';
			output += "Not After: " + validity_end + '\n';
			output += "Issuer Name: " + issuer_name + '\n';
			output += "Subject Name: " + subject_name + '\n';
			output += "Subject Public Key: " + public_key + '\n';
			output += "Public Key Algorithm: " + key_alg + '\n';

			return output;
		}

		static ArraySegment<byte> read(string filename)
		{
			var fs = new FileStream(filename, FileMode.Open);
			var len = (int)fs.Length;
			var bits = new byte[len];

			fs.Read(bits, 0, len);

			return new ArraySegment<byte>(bits);
		}
	} 
}