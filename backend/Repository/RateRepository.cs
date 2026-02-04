using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Rate;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class RateRepository: IRateRepository
    {
        private readonly ApplicationDBContex _apiclient;
        public RateRepository(ApplicationDBContex apiclient)
        {
             _apiclient = apiclient;
        }

        public async Task<Rate> CreateAsync(Rate currencieModel)
        {
            await _apiclient.Rates.AddAsync(currencieModel);
            await _apiclient.SaveChangesAsync();
            return currencieModel;
        }

        public async Task<Rate?> DeleteAsync(string code)
        {
            var currencieModel = await _apiclient.Rates.FirstOrDefaultAsync(x => x.Code.ToUpper() == code.ToUpper());   

            if (currencieModel == null)
            {
                return null;
            }

            _apiclient.Rates.Remove(currencieModel);
            await _apiclient.SaveChangesAsync();
            return currencieModel;
        }   

        public async Task<List<Rate>> GetAllAsync()
        {
           return await _apiclient.Rates
               .Where(r => r.ResponseItemId != null)
               .ToListAsync();
        }

        public async Task<Rate?> GetByIdAsync(int id)
        {
            return await _apiclient.Rates.FindAsync(id);
        }

        public async Task<Rate?> GetByCodeAsync(string code)
        {
            return await _apiclient.Rates.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Rate?> UpdateAsync(string code, UpdateCurencieReaquestDto rateDto)
        {
            var existingCurrencie = await _apiclient.Rates.FirstOrDefaultAsync(x=>x.Code == code);
            if (existingCurrencie==null)
            {
                return null;
            }
            existingCurrencie.Currency = rateDto.Currency;
            existingCurrencie.Code = rateDto.Code;
            existingCurrencie.Bid = rateDto.Bid;
            existingCurrencie.Ask = rateDto.Ask;

            await _apiclient.SaveChangesAsync();
            return existingCurrencie;
        }

        public async Task UpdateRatesFromNBPAsync(List<ResponseItem> responseItems)
        {
            _apiclient.Rates.RemoveRange(await _apiclient.Rates.ToListAsync());
            _apiclient.ResponseItem.RemoveRange(await _apiclient.ResponseItem.ToListAsync());
            await _apiclient.SaveChangesAsync();

            var newRatesToAddToDb = new List<Rate>();
            var newResponseItemsToDb = new List<ResponseItem>(); 

            foreach (var nbpTableData in responseItems) 
            {
                if (nbpTableData.Rates != null) 
                {
                    foreach (var actualRateFromNbp in nbpTableData.Rates) 
                    {
                        
                        var rateEntityForDb = new Rate
                        {
                            
                            Currency = actualRateFromNbp.Currency,
                            Code = actualRateFromNbp.Code,
                            Bid = actualRateFromNbp.Bid,
                            Ask = actualRateFromNbp.Ask * 1.1
                           
                            
                        };
                        newRatesToAddToDb.Add(rateEntityForDb);
                    }
                }

                
                
                newResponseItemsToDb.Add(nbpTableData); 
            }

            if (newRatesToAddToDb.Any())
            {
                _apiclient.Rates.AddRange(newRatesToAddToDb);
            }

            if (newResponseItemsToDb.Any())
            {
                _apiclient.ResponseItem.AddRange(newResponseItemsToDb);
            }
            
            await _apiclient.SaveChangesAsync();
        }
    }
}