using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SaltyBet
{
    /// <summary>
    /// Handles when a new match with new characters is created and the bets are open.
    /// </summary>
    /// <param name="match"> The current match being on display.</param>
    public delegate Task NewMatchHandler(Match match);

    /// <summary>
    /// Handles when the new match has started and the bets are closed.
    /// </summary>
    /// <param name="match"> The current match being on display.</param>
    /// <param name="teamRedBet"> The total amount of salt bucks bet on the red team.</param>
    /// <param name="teamBlueBet"> The total amount of salt bucks bet on the blue team.</param>
    public delegate Task MatchStartedHandler(Match match, ulong teamRedBet, ulong teamBlueBet);

    /// <summary>
    /// Handles when the match has been finished and bets are paid out.
    /// </summary>
    /// <param name="match"> The current match being on display.</param>
    /// <param name="teamRedBet"> The total amount of salt bucks bet on the red team.</param>
    /// <param name="teamBlueBet"> The total amount of salt bucks bet on the blue team.</param>
    public delegate Task MatchFinishedHandler(Match match, ulong teamRedBet, ulong teamBlueBet);

    /// <summary>
    /// Handles when a new game mode has started.
    /// </summary>
    /// <param name="gameMode"> The game mode that has started.</param>
    public delegate Task GameModeStartedHandler(GameMode gameMode);

    /// <summary>
    /// A client used to listen to the match feed from saltybet matches.
    /// </summary>
    public interface ISaltyBetClient
    {
        /// <summary>
        /// Starts the client and listen to incoming messages.
        /// </summary>
        Task StartConnectionAsync();

        /// <summary>
        /// Stop listening to incoing messages.
        /// </summary>
        /// <param name="status"> Signals how the websocket has been closed.</param>
        /// <param name="description"> The description of how the websocket has been closed.</param>
        Task StopConnectionAsync(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string? description = null);

        /// <summary>
        /// Gets the Match status from the moment the status was requested.
        /// </summary>
        /// <returns> The Match status from the moment the status was requested.</returns>
        MatchStatus GetCurrentMatchStatus();

        /// <summary>
        /// Adds the handler for when a new match with new characters is created and the bets are open.
        /// </summary>
        /// <param name="newMatchHandler"> The handler for when a new match is created.</param>
        void AddOnNewMatchHandler(NewMatchHandler newMatchHandler);

        /// <summary>
        /// Adds the handler for when the new match has started and the bets are closed.
        /// </summary>
        /// <param name="matchStartedHandler"> The handler for when the new match has started and the bets are closed.</param>
        void AddOnMatchStartedHandler(MatchStartedHandler matchStartedHandler);

        /// <summary>
        /// Adds the handler for when the match has been finished and bets are paid out.
        /// </summary>
        /// <param name="matchFinishedHandler"> The handler for when the match has been finished and bets are paid out.</param>
        void AddOnMatchFinishedHandler(MatchFinishedHandler matchFinishedHandler);

        /// <summary>
        /// Adds the handler for when a new game mode has been loaded in.
        /// </summary>
        /// <param name="modeStartedHandler"> The handler for when a gamemode has been loaded in.</param>
        void AddOnGameModeStarted(GameModeStartedHandler modeStartedHandler);

        /// <summary>
        /// Removes the handler for when a new match with new characters is created and the bets are open.
        /// </summary>
        /// <param name="newMatchHandler"> The handler for when a new match is created.</param>
        void RemoveOnNewMatchHandler(NewMatchHandler newMatchHandler);

        /// <summary>
        /// Removes the handler for when the new match has started and the bets are closed.
        /// </summary>
        /// <param name="matchStartedHandler"> The handler for when the new match has started and the bets are closed.</param>
        void RemoveOnMatchStartedHandler(MatchStartedHandler matchStartedHandler);

        /// <summary>
        /// Removes the handler for when the match has been finished and bets are paid out.
        /// </summary>
        /// <param name="matchFinishedHandler"> The handler for when the match has been finished and bets are paid out.</param>
        void RemoveOnMatchFinishedHandler(MatchFinishedHandler matchFinishedHandler);

        /// <summary>
        /// Adds the handler for when a new game mode has been loaded in.
        /// </summary>
        /// <param name="modeStartedHandler"> The handler for when a gamemode has been loaded in.</param>
        void RemoveOnGameModeStarted(GameModeStartedHandler modeStartedHandler);
    }
}
