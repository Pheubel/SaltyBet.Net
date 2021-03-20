using System;
using System.Runtime.CompilerServices;

namespace SaltyBet
{
    /// <summary>
    /// A snapshot of the current state of a match on saltybet.
    /// </summary>
    public class Match
    {
        /// <summary> The current status of the match.</summary>
        public MatchStatus Status { get; init; }
        /// <summary> The current game mode of the match.</summary>
        public GameMode GameMode { get; init; }
        /// <summary> The name of the team representing the red team.</summary>
        /// <remarks> During matchmaking and tournaments this is often the character being played.</remarks>
        public string TeamRedName { get; init; }
        /// <summary> The name of the team representing the blue team.</summary>
        /// <remarks> During matchmaking and tournaments this is often the character being played.</remarks>
        public string TeamBlueName { get; init; }
        /// <summary> The ammount of matches until the next game mode gets loaded.</summary>
        public byte MatchesUntilNextMode { get; init; }

        /// <summary>
        /// Creates an instance of the <see cref="Match"/> class.
        /// </summary>
        /// <param name="gameMode"> The current game mode of the match.</param>
        /// <param name="status"> The current status of the match.</param>
        /// <param name="matchesUntilNextMode"> The ammount of matches until the next game mode gets loaded.</param>
        /// <param name="teamRedName"> The name of the team representing the red team.</param>
        /// <param name="teamBlueName"> The name of the team representing the blue team.</param>
        /// <exception cref="ArgumentNullException" />
        public Match(GameMode gameMode, MatchStatus status, byte matchesUntilNextMode, string teamRedName, string teamBlueName)
        {
            Status = status;
            GameMode = gameMode;
            TeamRedName = teamRedName ?? throw new ArgumentNullException(nameof(teamRedName));
            TeamBlueName = teamBlueName ?? throw new ArgumentNullException(nameof(teamBlueName));
            MatchesUntilNextMode = matchesUntilNextMode;
        }

        /// <summary>
        /// Determines if two match snapshots come from the same match.
        /// </summary>
        /// <param name="other"> The other snapshot to compare it to.</param>
        /// <returns> True if both match snapshots come from the same match.</returns>
        public bool IsSameMatch(Match other) =>
            IsSameMatch(this, other);

        /// <summary>
        /// Determines if two match snapshots come from the same match.
        /// </summary>
        /// <param name="left"> A snapshot from a match.</param>
        /// <param name="right"> The other match snapshot to compare it to.</param>
        /// <returns> True if both match snapshots come from the same match.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSameMatch(Match left, Match right)
        {
            return left.GameMode == right.GameMode &&
                ((left.Status == MatchStatus.TeamBlueWon || left.Status == MatchStatus.TeamRedWon) ^ (left.Status == MatchStatus.TeamBlueWon || left.Status == MatchStatus.TeamRedWon)) == false ?
                    left.MatchesUntilNextMode == right.MatchesUntilNextMode :
                    Math.Abs(left.MatchesUntilNextMode - right.MatchesUntilNextMode) == 1 &&
                left.TeamRedName == right.TeamRedName &&
                left.TeamBlueName == right.TeamBlueName;
        }
    }
}
