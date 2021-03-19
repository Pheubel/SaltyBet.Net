using System;
using System.Runtime.CompilerServices;

namespace SaltyBet
{
    public class Match
    {
        public MatchStatus Status { get; init; }
        public GameMode GameMode { get; init; }
        public string TeamRedName { get; init; }
        public string TeamBlueName { get; init; }
        public byte MatchesUntilNextMode { get; init; }

        internal Match()
        {
            TeamRedName = null!;
            TeamBlueName = null!;
        }

        public Match(MatchStatus status, GameMode gameMode, string teamRedName, string teamBlueName, byte matchesUntilNextMode)
        {
            Status = status;
            GameMode = gameMode;
            TeamRedName = teamRedName ?? throw new ArgumentNullException(nameof(teamRedName));
            TeamBlueName = teamBlueName ?? throw new ArgumentNullException(nameof(teamBlueName));
            MatchesUntilNextMode = matchesUntilNextMode;
        }

        public bool IsSameMatch(Match other) =>
            IsSameMatch(this, other);

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
