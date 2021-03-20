using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaltyBet.Betting
{
    public interface IBettingClient
    {
        Task<bool> PlaceBetAsync(bool betOnRed, ulong amount);
        Task GetRankAsync();
        ulong GetBalance();
    }
}
