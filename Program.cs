using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkillBuilding {
    class Program {
        // Http Client to make network calls
        readonly static HttpClient client = new HttpClient();

        private const string baseURL = "https://api.testknock.com/api";
        // /{route}/question/${version}/${subTopicRoute}

        static async Task Main(string[] args) {
            // Expanded Master List based on the Syllabus Analysis
            var targets = new List<(string Topic, string SubTopic)> {
                // --- QUANTITATIVE APTITUDE (Math) ---
                ("QUANTITATIVE_APTITUDE", "NUMBER_SYSTEM"),       // Divisibility, Remainders
                ("QUANTITATIVE_APTITUDE", "PERCENTAGE"),          // Basic % calculation
                ("QUANTITATIVE_APTITUDE", "PROFIT_AND_LOSS"),     // Discounts, Markups
                ("QUANTITATIVE_APTITUDE", "S.I_AND_C.I"),         // Interest problems (Special route handled)
                ("QUANTITATIVE_APTITUDE", "RATIO_AND_PROPORTION"),// Mixtures, Ages
                ("QUANTITATIVE_APTITUDE", "AVERAGES"),            // Weighted averages
                ("QUANTITATIVE_APTITUDE", "TIME_SPEED_AND_DISTANCE"), // Trains, Boats
                ("QUANTITATIVE_APTITUDE", "TIME_AND_WORK"),       // Efficiency problems
                ("QUANTITATIVE_APTITUDE", "PERMUTATION_AND_COMBINATION"), // Arrangements
                ("QUANTITATIVE_APTITUDE", "PROBABILITY"),         // Dice, Cards

                // --- LOGICAL REASONING ---
                ("LOGICAL_REASONING", "SYLLOGISM"),               // Logic (All cats are dogs...)
                ("LOGICAL_REASONING", "BLOOD_RELATIONS"),         // Family tree
                ("LOGICAL_REASONING", "CODING_DECODING"),         // "ROPE" = 6821
                ("LOGICAL_REASONING", "NUMBER_OR_ALPHABET_SERIES"), // Missing terms
                ("LOGICAL_REASONING", "DIRECTION_SENSE"),         // North/South walks
                ("LOGICAL_REASONING", "ARRANGEMENTS"),            // Seating arrangements (Circular/Linear)
                ("LOGICAL_REASONING", "DATA_SUFFICIENCY"),        // Is data enough to answer?

                // --- VERBAL ABILITY ---
                ("VERBAL_ABILITY_AND_READING_COMPREHENSION", "READING_COMPREHENSION"), // Long passages
                ("VERBAL_ABILITY_AND_READING_COMPREHENSION", "SYNONYMS"),              // Vocab
                ("VERBAL_ABILITY_AND_READING_COMPREHENSION", "ANTONYMS"),              // Vocab
                ("VERBAL_ABILITY_AND_READING_COMPREHENSION", "SENTENCE_CORRECTION"),   // Grammar
                ("VERBAL_ABILITY_AND_READING_COMPREHENSION", "IDIOMS_AND_PHRASES"),    // Vocab
                ("VERBAL_ABILITY_AND_READING_COMPREHENSION", "ONE_WORD_SUBSTITUTION"), // Vocab
                ("VERBAL_ABILITY_AND_READING_COMPREHENSION", "PARA_JUMBLES"),          // Reordering sentences
                
                // --- DATA INTERPRETATION ---
                ("DATA_INTERPRETATION", "TABLE_CHARTS"),
                ("DATA_INTERPRETATION", "BAR_GRAPHS"),
                ("DATA_INTERPRETATION", "PIE_CHARTS")
            };

            foreach (var (topic, subTopic) in targets) {
                await FetchQuestions(topic, subTopic);
            }
        }

        private static string ConstructURL(string topic, string subTopic) {
            string route = "";

            switch (topic) {
                case "QUANTITATIVE_APTITUDE": route = "math"; break;
                case "DATA_INTERPRETATION": route = "di"; break;
                case "VERBAL_ABILITY_AND_READING_COMPREHENSION": route = "varc"; break;
                case "LOGICAL_REASONING": route = "lr"; break;
            }

            string subTopicRoute = subTopic;

            if (subTopic == "S.I_AND_C.I")
                subTopicRoute = "simple_interest_and_compound_interest";
            if (subTopic == "NUMBER_OR_ALPHABET_SERIES")
                subTopicRoute = "number_alphabet_series";

            string version = "v2";

            // Case: Reading Comprehension (v1)
            if (topic == "VERBAL_ABILITY_AND_READING_COMPREHENSION" && subTopic == "READING_COMPREHENSION")
            {
                version = "v1";
                subTopicRoute = ""; // Clear it
            }

            // Case: Vocabulary
            var vocabTopics = new HashSet<string> { "IDIOMS_AND_PHRASES", "SYNONYMS", "ANTONYMS", "ONE_WORD_SUBSTITUTION" };
            if (topic == "VERBAL_ABILITY_AND_READING_COMPREHENSION" && vocabTopics.Contains(subTopic))
            {
                subTopicRoute = $"sub/vocabulary/{subTopic.ToLower()}";
            }

            // Case: Data Interpretation
            if (topic == "DATA_INTERPRETATION") version = "v1";

            // Case: Logical Reasoning Misc
            if (topic == "LOGICAL_REASONING" && (subTopic == "MISCELLANEOUS" || subTopic == "ARRANGEMENTS"))
            {
                version = "v1";
            }

            string finalSub = !string.IsNullOrEmpty(subTopicRoute) ? subTopicRoute.ToLower() : "";

            string url = $"{baseURL}/{route}/question/{version}/{finalSub}";

            if (url.EndsWith("/")) url = url.Substring(0, url.Length - 1);

            return url;
        }

        private static async Task FetchQuestions(string topic, string subTopic) {
            string url = ConstructURL(topic, subTopic);

            try {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode) {
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    string fileName = $"{topic}_{subTopic}.json";
                    await File.WriteAllTextAsync(fileName, jsonContent);

                    Console.WriteLine($"    [+] Success! Saved to {fileName}");
                } else {
                    Console.WriteLine($"    [-] Failed with status: {response.StatusCode}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"    [-] Exception occurred: {ex.Message}");
            }
        }
    }
}