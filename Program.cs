namespace PFXParser
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var fs = new FileStream("test.p7b", FileMode.Open);
            var len = (int)fs.Length;
            var bits = new byte[len];
            fs.Read(bits, 0, len);
            

            DERElement der = DERElement.parse(new ArraySegment<byte>(bits));

            Console.WriteLine(der.ToString());

        }
    }
}
