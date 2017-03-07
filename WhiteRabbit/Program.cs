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
            var processor = new StringsProcessor("poultry outwits ants", 3);
            foreach (var phrase in processor.GeneratePhrases(ReadInput()))
            {
                var hash = GetHash(phrase);
                Console.WriteLine(hash + ": " + phrase);
            }
        }

        private static string GetHash(string input)
        {
            using (MD5 hasher = MD5.Create())
            {
                var data = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
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
