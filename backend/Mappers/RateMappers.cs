using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Rate;
using api.Models;

namespace api.Mappers
{
    public static class CurrencieMappers
    {
        public static RateDto ToRateDto(this Rate rateModel)
        {
            return new RateDto
            {
                Id = rateModel.Id,
                Currency = rateModel.Currency,
                Code = rateModel.Code,
                Bid = rateModel.Bid,
                Ask = rateModel.Ask
            };
        }

        public static Rate ToRateFromCreateDTO(this CreateCurencieReaquestDto rateDto)
        {
            return new Rate
            {
                Currency = rateDto.Currency,
                Code = rateDto.Code,
                Bid = rateDto.Bid,
                Ask = rateDto.Ask
            };
        }
    }
}