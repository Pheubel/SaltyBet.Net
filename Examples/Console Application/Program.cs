//using SaltyBet;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SaltyBetting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HttpClient httpClient = new HttpClient();
            //SaltyBetClient saltyClient = new SaltyBetClient(httpClient);

            //saltyClient.AddOnNewMatchHandler(OnNewMatch);
            //saltyClient.AddOnMatchStartedHandler(OnMatchStarted);
            //saltyClient.AddOnMatchFinishedHandler(OnMatchFinished);
            //saltyClient.AddOnGameModeStarted(OnGamemodeStarted);

            //await saltyClient.StartConnectionAsync();

            JObject obj = new JObject();
            Console.WriteLine(obj);

            await Task.Delay(-1);
        }

        //private static Task OnGamemodeStarted(GameMode gameMode)
        //{
        //    Console.WriteLine($"Started new gamemode: {gameMode}");
        //    return Task.CompletedTask;
        //}

        //private static Task OnMatchFinished(Match match, ulong player1Bet, ulong player2Bet)
        //{
        //    if(match.Status == MatchStatus.Draw)
        //    {
        //        Console.WriteLine($"[{match.GameMode} {match.MatchesUntilNextMode} matches left] A draw has happened, bets have been returned.");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"[{match.GameMode} {match.MatchesUntilNextMode} matches left] The battle is over, {(match.Status == MatchStatus.TeamRedWon ? $"{match.TeamRedName} (red)" : $"{match.TeamBlueName} (blue)")} has won! paying out the ${player1Bet} : ${player2Bet} odds.");
        //    }
            
        //    return Task.CompletedTask;
        //}

        //private static Task OnNewMatch(Match match)
        //{
        //    Console.WriteLine($"[{match.GameMode} {match.MatchesUntilNextMode} matches left] A new battle starts between {match.TeamRedName} and {match.TeamBlueName}, place your bets!");
        //    return Task.CompletedTask;
        //}

        //private static Task OnMatchStarted(Match match, ulong player1Bet, ulong player2Bet)
        //{
        //    Console.WriteLine($"[{match.GameMode} {match.MatchesUntilNextMode} matches left] The battle starts between {match.TeamRedName} (${player1Bet}) and {match.TeamBlueName} (${player2Bet}).");
        //    return Task.CompletedTask;
        //}
    }
}