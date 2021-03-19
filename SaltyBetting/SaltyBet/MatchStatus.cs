namespace SaltyBet
{
    /// <summary>
    /// Determines the status of the match.
    /// </summary>
    public enum MatchStatus : byte
    {
        /// <summary> The match's status cannot be determined.</summary>
        Unknown,
        /// <summary> Player one won the match.</summary>
        TeamRedWon,
        /// <summary> Player two won the match.</summary>
        TeamBlueWon,
        /// <summary> Bets can be placed on the current match.</summary>
        BetsOpen,
        /// <summary> Bets can not be placed on the current match as it has started.</summary>
        BetsClosed,
        /// <summary> The match has ended in a state that has no winners either by two opponents being left with the same HP or a system reset.</summary>
        Draw
    }
}
