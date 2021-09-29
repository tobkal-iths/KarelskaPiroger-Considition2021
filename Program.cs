
using System;
using System.Linq;
using System.Text.Json;
using System.Xml;
using DotNet.Responses;

namespace DotNet
{
    public static class Program
    {
        private const string ApiKey = "";  // TODO: Enter your API key
        // The different map names can be found on considition.com/rules
        private const string Map = "training1";     // TODO: Enter your desired map
        private static readonly GameLayer GameLayer = new(ApiKey);
        
        public static void Main(string[] args)
        {
            var gameInformation = GameLayer.NewGame(Map);
            GreedySolver greedySolver = new GreedySolver(gameInformation.Dimensions, gameInformation.Vehicle);
            var solution = greedySolver.Solve();
            var submitSolution = GameLayer.Submit(JsonSerializer.Serialize(solution), Map);
           
            Console.WriteLine("Your GameId is: " + submitSolution.GameId);
            Console.WriteLine("Your score is: " + submitSolution.Score);
            Console.WriteLine("Link to visualisation" + submitSolution.Link);

        }
    }
}
