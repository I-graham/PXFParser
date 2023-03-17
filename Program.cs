namespace PFXParser
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var certs = X509.parse("test.p7b");

			foreach(var cert in certs) {
				Console.WriteLine(cert);
			}

		}
	}
}
