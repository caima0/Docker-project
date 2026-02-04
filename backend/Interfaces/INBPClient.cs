using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface INBPClient
    {
        Task<List<ResponseItem>> GetExchangeRatesAsync();
    }
} 