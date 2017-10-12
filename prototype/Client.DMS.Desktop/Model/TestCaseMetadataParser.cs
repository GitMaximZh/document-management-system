using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using DMS.Core.Persistence;

namespace Client.DMS.Desktop.Model
{
    class TestCase
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Result { get; set; }
    }

    class TestCaseMetadataParser : IDocumentMetadataParser
    {
        public const string NameKey = "Name";
        public const string DateKey = "Date";
        public const string ResultKey = "Result";

        public string Extension => "json";

        public IDictionary<string, string> Parse(Stream stream)
        {
            IDictionary<string, string> testCase;
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                testCase = new JsonSerializer().Deserialize<Dictionary<string, string>>(jsonReader);
            }
            return new Dictionary<string, string>
            {
                { NameKey, testCase[NameKey] },
                { DateKey, DateTime.Parse(testCase[DateKey]).ToLongTimeString() },
                { ResultKey, testCase[ResultKey] }
            };
        }
    }
}
