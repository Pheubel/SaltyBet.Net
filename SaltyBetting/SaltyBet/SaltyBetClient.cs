using System;
using System.Globalization;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaltyBet
{
    /// <summary>
    /// A standard implementation of a client used to listen to the feed of saltybet matches.
    /// </summary>
    public class SaltyBetClient : IDisposable, ISaltyBetClient
    {
        private const string SocketAdress = "wss://www.saltybet.com:2096/socket.io/?EIO=3&transport=websocket";
        private const string CurrentStateAdress = "https://www.saltybet.com/state.json";
        private const int PingInterval = 25000;

        readonly static ReadOnlyMemory<byte> _pingMessage = Encoding.UTF8.GetBytes("5");
        readonly static Memory<byte> _messageString = Encoding.UTF8.GetBytes("42[\"message\"]");

        /// <summary>
        /// The HTTP client used to fetch the current state of saltybet.
        /// </summary>
        protected HttpClient HttpClient { get; private set; }

        /// <summary>
        /// The latest match data received from salty bet.
        /// </summary>
        protected Match? CurrentMatch { get; private set; }

        private event NewMatchHandler? OnNewMatch;
        private event MatchStartedHandler? OnMatchStarted;
        private event MatchFinishedHandler? OnMatchFinished;
        private event GameModeStartedHandler? OnGameModeStarted;

        private readonly ClientWebSocket _socketClient;
        private CancellationTokenSource? _cts;
        private ClientState _clientState;
        private Timer? _pingTimer;

        /// <summary> Creates an instance of the <see cref="SaltyBetClient"/> class.</summary>
        /// <param name="httpClient"> The underlying client uused for calling saltybet endpoints.</param>
        /// <param name="options"> The additional options to be applied to the underlying websocket.</param>
        /// <exception cref="ArgumentNullException" />
        public SaltyBetClient(HttpClient httpClient, Action<ClientWebSocketOptions>? options = null)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _socketClient = new ClientWebSocket();

            if (options != null)
                options.Invoke(_socketClient.Options);

            _clientState = ClientState.Disconnected;
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException" />
        /// <exception cref="InvalidOperationException" />
        public virtual async Task StartConnectionAsync()
        {
            if (_clientState == ClientState.Disposed)
                throw new ObjectDisposedException(this.GetType().Name);

            if (_clientState == ClientState.Listening)
                throw new InvalidOperationException("Cannot start connection in current client state.");

            _cts = new CancellationTokenSource();

            await _socketClient.ConnectAsync(new Uri(SocketAdress), CancellationToken.None);
            StartPingInterval();

            await ListenAsync(_cts.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts the internal ping timer to keep the socket connection alive.
        /// </summary>
        private void StartPingInterval()
        {
            _pingTimer = new Timer(async _ =>
            {
                try
                {
                    await _socketClient.SendAsync(_pingMessage, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ran into issues while pinging the console: {ex.Message}");
                }
            }, null, PingInterval, PingInterval);
        }

        /// <inheritdoc/>
        public virtual async Task StopConnectionAsync(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string? description = null)
        {
            CurrentMatch = null;
            _pingTimer?.Dispose();
            await _socketClient.CloseAsync(status, description, CancellationToken.None);
        }

        /// <inheritdoc/>
        public MatchStatus GetCurrentMatchStatus() =>
            CurrentMatch?.Status ?? MatchStatus.Unknown;

        /// <summary>
        /// Starts listening to the websocket feed.
        /// </summary>
        /// <param name="ct"></param>
        private async Task ListenAsync(CancellationToken ct)
        {
            byte[] messageBuffer = new byte[256];

            _clientState = ClientState.Listening;

            do
            {
                var result = await _socketClient.ReceiveAsync(messageBuffer, ct);
                if (_messageString.Span.SequenceEqual(messageBuffer.AsSpan()[..result.Count]))
                    await UpdateStateAsync();
            } while (_clientState == ClientState.Listening && !ct.IsCancellationRequested);
        }

        private async Task UpdateStateAsync()
        {
            var response = await HttpClient.GetStringAsync(CurrentStateAdress);

            using var matchState = System.Text.Json.JsonDocument.Parse(response);

            MatchStatus newStatus = MatchStatus.Unknown;
            string player1 = null!;
            string player2 = null!;
            string remaining = null!;
            string alert = null!;
            ulong player1Bet = 0;
            ulong player2Bet = 0;
            var enumerator = matchState.RootElement.EnumerateObject();
            foreach (var property in enumerator)
            {
                switch (property.Name)
                {
                    case "status":
                        newStatus = property.Value.GetString() switch
                        {
                            "1" => MatchStatus.TeamRedWon,
                            "2" => MatchStatus.TeamBlueWon,
                            "open" => MatchStatus.BetsOpen,
                            "locked" => MatchStatus.BetsClosed,
                            _ => MatchStatus.Draw
                        };
                        break;

                    case "p1name":
                        player1 = property.Value.GetString() ?? string.Empty;
                        break;

                    case "p2name":
                        player2 = property.Value.GetString() ?? string.Empty;
                        break;

                    case "p1total":
                        player1Bet = ulong.Parse(property.Value.GetString()!, NumberStyles.AllowThousands);
                        break;

                    case "p2total":
                        player2Bet = ulong.Parse(property.Value.GetString()!, NumberStyles.AllowThousands);
                        break;

                    case "remaining":
                        remaining = property.Value.GetString() ?? string.Empty;
                        break;

                    case "alert":
                        alert = property.Value.GetString() ?? string.Empty;
                        break;

                    default:
                        break;
                }
            }

            GameMode gameMode = GameMode.Unknown;
            byte matchesUntilNextMode = default;




            // determine the game mode
            if (remaining.EndsWith("next tournament!"))
            {
                matchesUntilNextMode = byte.Parse(remaining.AsSpan(0, remaining.IndexOf(' ')));
                if ((newStatus == MatchStatus.TeamBlueWon || newStatus == MatchStatus.TeamRedWon) && matchesUntilNextMode == 100)
                {
                    // assume the previous mode was exhibition, when no matches are requested before the tournament exhibitions will be skipped 
                    // but it looks unlikely to get info about the edge case from the context of a single message
                    gameMode = GameMode.Exhibition;
                    matchesUntilNextMode = 1;
                }
                else
                {
                    gameMode = GameMode.Matchmaking;
                }
            }
            // determine if final round of matchmaking
            else if (remaining.StartsWith("Tournament mode will be activated"))
            {
                matchesUntilNextMode = (newStatus == MatchStatus.TeamBlueWon || newStatus == MatchStatus.TeamRedWon) ? 2 : 1;
                gameMode = GameMode.Matchmaking;
            }
            else if (remaining.EndsWith("left in the bracket!"))
            {
                matchesUntilNextMode = (byte)(byte.Parse(remaining.AsSpan(0, remaining.IndexOf(' '))) - 1);
                if ((newStatus == MatchStatus.TeamBlueWon || newStatus == MatchStatus.TeamRedWon) && matchesUntilNextMode == 15)
                {
                    matchesUntilNextMode = 1;
                    gameMode = GameMode.Matchmaking;
                }
                else
                {
                    gameMode = GameMode.Tournament;
                }
            }
            // determine if final round of tournament
            else if (remaining.EndsWith("after the tournament!"))
            {
                matchesUntilNextMode = (newStatus == MatchStatus.TeamBlueWon || newStatus == MatchStatus.TeamRedWon) ? 2 : 1;
                gameMode = GameMode.Tournament;
            }
            // determine if exhibition
            else if (remaining.EndsWith("exhibition matches left!"))
            {
                matchesUntilNextMode = byte.Parse(remaining.AsSpan(0, remaining.IndexOf(' ')));
                if ((newStatus == MatchStatus.TeamBlueWon || newStatus == MatchStatus.TeamRedWon) && alert == "Exhibition mode start!")
                {
                    matchesUntilNextMode = 1;
                    gameMode = GameMode.Tournament;
                }
                else
                {
                    gameMode = GameMode.Exhibition;
                }
            }
            // determine if final exhibition match
            else if (remaining == "Matchmaking mode will be activated after the next exhibition match!")
            {
                matchesUntilNextMode = (newStatus == MatchStatus.TeamBlueWon || newStatus == MatchStatus.TeamRedWon) ? 2 : 1;
                gameMode = GameMode.Exhibition;
            }



            Match match = new Match
            {
                Status = newStatus,
                TeamRedName = player1,
                TeamBlueName = player2,
                GameMode = gameMode,
                MatchesUntilNextMode = matchesUntilNextMode
            };

            // make sure match data is new
            if (CurrentMatch != null && CurrentMatch.Status == match.Status && CurrentMatch.IsSameMatch(match))
                return;

            CurrentMatch = match;

            switch (newStatus)
            {
                case MatchStatus.TeamRedWon:
                case MatchStatus.TeamBlueWon:
                    OnMatchFinished?.Invoke(match, player1Bet, player2Bet);
                    break;

                case MatchStatus.BetsOpen:

                    switch (alert)
                    {
                        case "Tournament mode start!":
                        case "Exhibition mode start!":
                            OnGameModeStarted?.Invoke(match.GameMode);
                            break;
                        default:
                            if (matchesUntilNextMode == 100)
                                OnGameModeStarted?.Invoke(GameMode.Matchmaking);
                            break;
                    }

                    OnNewMatch?.Invoke(match);
                    break;

                case MatchStatus.BetsClosed:
                    OnMatchStarted?.Invoke(match, player1Bet, player2Bet);
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_clientState == ClientState.Disposed)
                throw new ObjectDisposedException(this.GetType().Name);

            GC.SuppressFinalize(this);
            _clientState = ClientState.Disposed;

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        /// <inheritdoc/>
        public void AddOnNewMatchHandler(NewMatchHandler newMatchHandler) =>
            OnNewMatch += newMatchHandler;

        /// <inheritdoc/>
        public void AddOnMatchStartedHandler(MatchStartedHandler matchStartedHandler) =>
            OnMatchStarted += matchStartedHandler;

        /// <inheritdoc/>
        public void AddOnMatchFinishedHandler(MatchFinishedHandler matchFinishedHandler) =>
            OnMatchFinished += matchFinishedHandler;

        /// <inheritdoc/>
        public void RemoveOnNewMatchHandler(NewMatchHandler newMatchHandler) =>
            OnNewMatch -= newMatchHandler;

        /// <inheritdoc/>
        public void RemoveOnMatchStartedHandler(MatchStartedHandler matchStartedHandler) =>
            OnMatchStarted -= matchStartedHandler;

        /// <inheritdoc/>
        public void RemoveOnMatchFinishedHandler(MatchFinishedHandler matchFinishedHandler) =>
            OnMatchFinished -= matchFinishedHandler;

        /// <inheritdoc/>
        public void AddOnGameModeStarted(GameModeStartedHandler modeStartedHandler) =>
            OnGameModeStarted += modeStartedHandler;

        /// <inheritdoc/>
        public void RemoveOnGameModeStarted(GameModeStartedHandler modeStartedHandler) =>
            OnGameModeStarted -= modeStartedHandler;

        /// <summary>
        /// Determines the current state the client is in.
        /// </summary>
        private enum ClientState
        {
            Disconnected,
            Listening,
            Disposed
        }
    }
}
