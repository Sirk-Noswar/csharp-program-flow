using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WiredBrainCoffeeSurveys.Reports
{
    class Program
    {
        static void Main(string[] args)
        {
            bool quitApp = false;

            do
            {
                Console.WriteLine("Please specify a report to run (rewards, comments, tasks, quit):");
                var selectedReports = Console.ReadLine();

                Console.WriteLine("PLease specify which quarter of data: (Q1, Q2)");
                var selectedData = Console.ReadLine();

                var surveyResults = JsonConvert.DeserializeObject<SurveyResults>
                    (File.ReadAllText($"data/{selectedData}.json"));

                // Need to drop in an if statement for quit to quit the app.
                switch (selectedReports)
                {
                    case "rewards":
                        GenerateWinnerEmails(surveyResults);
                        break;
                    case "comments":
                        GenerateCommentsReport(surveyResults);
                        break;
                    case "tasks":
                        GenerateTasksReport(surveyResults);
                        break;
                    case "quit":
                        quitApp = true;
                        break;
                    default:
                        Console.WriteLine("Sorry, that's not a valid option.");
                        break;
                }
            } 
            while (!quitApp);
        }

        public static void GenerateWinnerEmails(SurveyResults results)
        {
            var selectedEmails = new List<string>();
            int counter = 0;

            Console.WriteLine(Environment.NewLine + "Selected Winners Output:");
            while (selectedEmails.Count < 2 && counter < results.Responses.Count)
            {
                var currentItem = results.Responses[counter];

                if (currentItem.FavoriteProduct == results.FavoriteProduct)
                {
                    selectedEmails.Add(currentItem.EmailAddress);
                    Console.WriteLine(currentItem.EmailAddress);
                }

                counter++;
            }

            File.WriteAllLines("WinnersReport.csv", selectedEmails);
        }

        public static void GenerateCommentsReport(SurveyResults results)
        {
            var comments = new List<string>();

            Console.WriteLine(Environment.NewLine + "Comments Output:");
            for (var i = 0; i < results.Responses.Count; i++)
            {
                var currentResponse = results.Responses[i];

                if (currentResponse.WouldRecommend < 7.0)
                {
                    Console.WriteLine(currentResponse.Comments);
                    comments.Add(currentResponse.Comments);
                }
            }

            foreach (var response in results.Responses)
            {
                if (response.AreaToImprove == results.AreaToImprove)
                {
                    Console.WriteLine(response.Comments);
                    comments.Add(response.Comments);
                }
            }

            File.WriteAllLines("CommentsReport.csv", comments);
        }

        public static void GenerateTasksReport(SurveyResults results)
        {
            var tasks = new List<string>();

            double responseRate = results.NumberResponded / results.NumberSurveyed;
            double overallScore = (results.ServiceScore + results.CoffeeScore + results.FoodScore + results.PriceScore) / 4;

            if (results.CoffeeScore < results.FoodScore)
            {
                tasks.Add("Investigate coffee recipes and ingredients.");
            }

            if (overallScore > 8.0)
            {
                tasks.Add("Work with leadership to reward staff");
            }
            else
            {
                tasks.Add("Work with employees for improvement ideas.");
            }

            // tasks.Add(overallScore > 8.0 ? "Work with leadership." : "Work with employees for ideas.");
            
            tasks.Add(responseRate switch
            {
                var rate when rate < .33 => "Research options to improve response rate.",
                var rate when rate > .33 && rate < .66 => "Reward participants with free coffee coupon.",
                var rate when rate > .66 => "Reward participants with discount coffee coupon."
            });

            tasks.Add(results.AreaToImprove switch
            {
                "RewardsProgram" => "Revisit the rewards deal",
                "Cleanliness" => "Contact the cleaning vendor",
                "MobileApp" => "Contact the consulting firm about the app.",
                _ => "Investigate individual comments for ideas."
            });


            Console.WriteLine(Environment.NewLine + "Tasks Output:");
            foreach(var task in tasks)
            {
                Console.WriteLine(task);
            }

            File.WriteAllLines("TasksReport.csv", tasks);
        }
    }
}
