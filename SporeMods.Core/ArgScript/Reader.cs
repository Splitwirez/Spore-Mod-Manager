using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable enable

namespace SporeMods.Core.ArgScript
{
    public class Reader
    {
        public Dictionary<string, ConfigTweak> Tweaks { get; }

        public Reader()
        {
            Tweaks = new Dictionary<string, ConfigTweak>();
        }
        public void Read(string file)
        {
            using (var reader = new StreamReader(file))
            {
                int lineno = 0;
                int pragmaCount = 0;
                int endPragmaCount = 0;

                bool recording = false;
                int start = 0;
                string currTweakId = "";
                StringBuilder content = new StringBuilder();
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();

                    if (line == null) continue;
                    
                    lineno++;

                    if (recording)
                    {
                        if (line.StartsWith("#endpragma"))
                        {
                            endPragmaCount++;
                            int end = lineno;
                            Tweaks.Add(currTweakId, new ConfigTweak(content.ToString(), start, end));
                            recording = false;
                        }
                        else
                        {
                            content.AppendLine(line);
                        }
                    }
                    else
                    {
                        if (line.StartsWith("#pragma"))
                        {
                            string[] lineSplit = line.Split(' ');
                            if (lineSplit.Length == 1)
                            {
                                throw new InvalidPragmaException(
                                    $"Invalid pragma on line {lineno}, missing tweak identifier.");
                            }
                            currTweakId = lineSplit[1];
                            start = lineno;
                            pragmaCount++;
                            recording = true;
                        }
                    }
                }

                if (pragmaCount != endPragmaCount)
                {
                    Console.WriteLine("Expected #endpragma, got EOF");
                }
            }
        }
        
        public class InvalidPragmaException : Exception
        {
            public override string ToString()
            {
                return Message;
            }

            public override string Message { get; }

            public InvalidPragmaException(string message)
            {
                Message = message;
            }
        }
    }

    public class ConfigTweak
    {
        public int Start { get; }
        public int End { get; }
        
        public string Text { get; }
        public ConfigTweak(string text, int start, int end)
        {
            Start = start;
            End = end;

            Text = text;
        }
    }
}