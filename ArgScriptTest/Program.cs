using System;
using SporeMods.Core.ArgScript;

namespace ArgScriptTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new Reader();
            try
            {
                reader.Read("path\\to\\config\\with\\tweaks");
            }
            catch (Reader.InvalidPragmaException ex)
            {
                Console.WriteLine(ex);
                return;
            }
            
            if (reader.Tweaks.Count == 0)
            {
                Console.WriteLine("No tweaks found, config is pure!");
                return;
            }
            
            foreach ((string tweakId, ConfigTweak tweak) in reader.Tweaks)
            {
                Console.WriteLine($"Tweak: {tweakId}");
                Console.WriteLine($"Start: {tweak.Start}, End: {tweak.End}");
                Console.WriteLine($"Content start: {tweak.Start + 1}, Content end: {tweak.End - 1}");
                Console.WriteLine($"Content:\n'''\n{tweak.Text}'''");
            }
        }
    }
}
