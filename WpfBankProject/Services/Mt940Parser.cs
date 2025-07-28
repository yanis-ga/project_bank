using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using WpfBankProject.Models;

namespace WpfBankProject.Services
{
    public static class Mt940Parser
    {
        public static List<Mt940RawLine> ParseRawLines(string filePath)
        {
            List<Mt940RawLine> lines = new List<Mt940RawLine>();
            string[] rawLines = File.ReadAllLines(filePath);

            Mt940RawLine? currentLine = null;

            foreach (string line in rawLines)
            {
                if (line.StartsWith(":"))
                {
                    int secondColonIndex = line.IndexOf(':', 1);
                    if (secondColonIndex > 0)
                    {
                        string tag = line.Substring(1, secondColonIndex - 1).Trim();
                        string content = line.Substring(secondColonIndex + 1).Trim();

                        // check error if there is a ':' in the content line
                        bool hasColonError = content.Contains(':');

                        // we create a new instance
                        currentLine = new Mt940RawLine
                        {
                            Tag = tag,
                            Content = content,
                            HasFormatError = hasColonError
                        };

                        lines.Add(currentLine); // add into the list

                    }
                }
                else if (currentLine != null)
                {
                    string extraLine = line.Trim();
                    currentLine.Content += "\n" + extraLine;

                    // Check if there is a ":" in a secondary line
                    if (extraLine.Contains(":"))
                    {
                        currentLine.HasFormatError = true;
                    }
                }
            }

            foreach (var line in lines)
            {
                if (line.Tag == "61")
                {
                    line.Details61 = Parse61Details(line.Content);
                }
                else if (line.Tag == "86")
                {
                    line.Details86 = Parse86Details(line.Content);
                }
            }

            return lines;
        }

        // === for parse the line :61 ===
        public static Mt940Line61Details Parse61Details(string content)
        {
            Mt940Line61Details details = new Mt940Line61Details();

            try
            {
                // Use Regex to decode the subfields
                string fullOriginalContent = content;
                content = content.Replace("\n", "").Replace("\r", "");

                var regex = new Regex(
                    @"^(?<valuedate>\d{6})" +                  // Ex: 250506
                    @"(?<entrydate>\d{4})?" +                  // Ex: 0506 (facultatif)
                    @"(?<dc>[DC]{1}R?)" +                      // D, CR, DR, etc.
                    @"(?<amount>\d+,\d{0,2})" +                // Ex: 304315,92
                    @"(?<transactiontype>[A-Z0-9]{3,4})?" +    // S100, NTRF, TAXN, etc.
                    @"(?<reference1>[^/]*)(//(?<reference2>[^\n\r]*))?",
                    RegexOptions.Compiled);

                var match = regex.Match(content);

                if (match.Success)
                {
                    string vd = match.Groups["valuedate"].Value;
                    string ed = match.Groups["entrydate"].Value;
                    string dc = match.Groups["dc"].Value;
                    string amt = match.Groups["amount"].Value;
                    string type = match.Groups["transactiontype"].Value;
                    string reference = match.Groups["reference1"].Value;


                    if (match.Groups["reference2"].Success)
                    {
                        reference += " // " + match.Groups["reference2"].Value;
                    }

                    details.ValueDate = DateTime.ParseExact(vd, "yyMMdd", CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(ed))
                    {
                        // Combine the year of the main date
                        string fullEntryDate = details.ValueDate.Value.Year.ToString() + ed;
                        details.EntryDate = DateTime.ParseExact(fullEntryDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    }

                    details.DebitCredit = dc;
                    details.Amount = decimal.Parse(amt.Replace(',', '.'), CultureInfo.InvariantCulture);
                    details.TransactionType = type;
                    details.Reference = reference;


                    // to avoid errors when there are no lines

                    var originalLines = fullOriginalContent.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries); // delete empty lines
                    if (originalLines.Length > 1)
                    {
                        string extraLine = originalLines[1].Trim();
                        if (!string.IsNullOrWhiteSpace(extraLine))
                        {
                            details.Reference2 = extraLine;
                        }
                    }
                    //test last version

                }
            }
            catch (Exception ex)
            {
                // Tempory : log or breakpoint
                Console.WriteLine("! Error when parsing the line :61:");
                Console.WriteLine(ex.Message);
            }

            return details;
        }

        // === for parse the line :86 ===
        public static Mt940Line86Details Parse86Details(string content)
        {
            var details = new Mt940Line86Details();

            // Split the content into lines
            string[] lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                details.DescriptionLines.Add(line.Trim());
            }

            return details;
        }
    }
}
