namespace SaltyBet
{
    /// <summary>
    /// Determines the gamemode being broadcasted.
    /// </summary>
    public enum GameMode : byte
    {
        /// <summary> The game mode is not know at this time.</summary>
        Unknown,
        /// <summary> Fighters get picked at random for 1 on 1 matches.</summary>
        Matchmaking,
        /// <summary> Preselected fighters fight 1 on 1 in an elimination style tournament until 1 is left.</summary>
        Tournament,
        /// <summary> Fights requested by viewers that can star any fighter they choose.</summary>
        Exhibition
    }
}
