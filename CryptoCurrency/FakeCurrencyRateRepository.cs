using System.Collections.Generic;

namespace CryptoCurrency
{
    internal interface IFakeCurrencyRateRepository
    {
        double GetRate(string currencyName);
        void SetRate(string currencyName, double rate);
    }

    internal class FakeCurrencyRateRepository : IFakeCurrencyRateRepository
    {
        private Dictionary<string, double> currencyRates = new Dictionary<string, double>();

        internal FakeCurrencyRateRepository()
        {            
        }

        public double GetRate(string currencyName)
        {
            if (currencyRates.ContainsKey(currencyName))
                return currencyRates[currencyName];

            throw new System.ArgumentException($"Ukendt valuta {currencyName}");
        }

        public void SetRate(string currencyName, double rate)
        {
            currencyRates[currencyName] = rate;
        }
    }
}