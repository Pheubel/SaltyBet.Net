using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaltyBet
{
    /// <summary>
    /// A standard implementation of a client used to place bets on saltybet matches. Use at your own risk.
    /// </summary>
    public sealed class SaltyBettingClient : SaltyBetClient, IBettingClient
    {
        /// <summary>
        /// Indicates if the client is logged into saltybet and can be used for placign bets.
        /// </summary>
        public bool IsLoggedIn { get; private set; }

        private ulong _balance;
        private ulong _wager;
        private Match? _subscribedMatch;
        private string? _userId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="options"></param>
        public SaltyBettingClient(HttpClient httpClient, Action<ClientWebSocketOptions>? options = null) : base(httpClient, options)
        { }

        /// <inheritdoc/>
        public override async Task StartConnectionAsync()
        {
            await base.StartConnectionAsync();
            AddOnMatchFinishedHandler(OnMatchFinished);
        }

        /// <inheritdoc/>
        public override async Task StopConnectionAsync(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string? description = null)
        {
            RemoveOnMatchFinishedHandler(OnMatchFinished);
            await base.StopConnectionAsync(status, description);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException" />
        public ulong GetBalance()
        {
            if (!IsLoggedIn)
                throw new InvalidOperationException("Can not get the balance while not logged in.");

            return _balance;
        }

        /// <summary> Get the current ammount being wagered.</summary>
        /// <returns> The current ammount being wagered, 0 if no bet is placed.</returns>
        public ulong GetWager() =>
            _wager;

        public async Task LogInAsync(string email, string password)
        {
            const string SignInUrl = "https://www.saltybet.com/authenticate?signin=1";

            if (IsLoggedIn)
                throw new InvalidOperationException("Can not log in while already logged in.");

            using FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string?, string?>[] {
                new("email", email),
                new ("pword", password),
                new ("authenticate", "signin")
            });

            content.Headers.Add("Keep-Alive", "true");

            using var response = await HttpClient.PostAsync(SignInUrl, content);
            response.EnsureSuccessStatusCode();
            // todo: scrape the returned webpage for useful data, like userId and balance 

            _userId = "498838";
            _balance = 1;

            IsLoggedIn = true;
        }

        public async Task LogOutAsync()
        {
            const string SignOutUrl = "https://www.saltybet.com/logout";

            if (!IsLoggedIn)
                throw new InvalidOperationException("Can not get the balance while not logged in.");

            _subscribedMatch = null;

            using var response = await HttpClient.GetAsync(SignOutUrl);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public async Task<bool> PlaceBetAsync(bool betOnRed, ulong amount)
        {
            const string PlaceBetsUrl = "https://www.saltybet.com/ajax_place_bet.php";

            if (!IsLoggedIn)
                throw new InvalidOperationException("Can not place bets while not logged in.");

            if (amount == 0 || amount > _balance)
                throw new ArgumentOutOfRangeException(nameof(amount), "Placed bets must be between 0 and your current balance.");

            // make sure that players can bet at this moment.
            if (GetCurrentMatchStatus() != MatchStatus.BetsOpen)
                return false;

            using FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string?, string?>[] {
                new("selectedplayer",betOnRed ? "player1" : "player2"),
                new ("wager",amount.ToString()),
            });

            using var response = await HttpClient.PostAsync(PlaceBetsUrl, content);
            response.EnsureSuccessStatusCode();

            // check if bet went through in time
            if (!(await response.Content.ReadAsStringAsync()).EndsWith("1"))
                return false;

            _subscribedMatch = CurrentMatch;
            _wager = amount;

            return true;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException" />
        public Task GetRankAsync()
        {
            if (!IsLoggedIn)
                throw new InvalidOperationException("Can not get rank while not logged in.");

            throw new NotImplementedException();
        }

        private async Task OnMatchFinished(Match match, ulong player1Bet, ulong player2Bet)
        {
            const string UserDataUrl = "https://www.saltybet.com/zdata.json";

            if (_subscribedMatch == null)
                return;

            _subscribedMatch = null;
            _wager = 0;

            var responseString = await HttpClient.GetStringAsync(UserDataUrl);
            using JsonDocument doc = JsonDocument.Parse(responseString);

            foreach (var user in doc.RootElement.EnumerateObject())
            {
                if (!user.NameEquals(_userId))
                    continue;

                foreach (var item in user.Value.EnumerateObject())
                {
                    if (item.NameEquals("b"))
                    {
                        _balance = item.Value.GetUInt64();
                        break;
                    }
                }
                break;
            }
        }
    }
}
