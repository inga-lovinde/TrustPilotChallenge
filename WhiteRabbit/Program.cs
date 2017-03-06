namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Main class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main()
        {
            var processor = new Processor("poultry outwits ants", 3);
            var results = new List<string>();
            foreach (var phrase in processor.GeneratePhrases(ReadInput()))
            {
                var hash = GetMd5Hash(phrase);
                Console.WriteLine(GetMd5Hash(phrase) + ": " + phrase);
                results.Add(phrase + ": " + hash);
            }

            foreach (var result in results.OrderBy(line => line))
            {
                Console.WriteLine(result);
            }
        }

        private static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(data.Select(b => b.ToString("x2")));
            }
        }

        private static IEnumerable<string> ReadInput()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
