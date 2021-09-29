using System.Collections.Concurrent;

namespace CryptoCurrency
{
    internal interface IFakeCurrencyRateRepository
    {
        double GetRate(string currencyName);
        void SetRate(string currencyName, double rate);
    }

    internal class FakeCurrencyRateRepository : IFakeCurrencyRateRepository
    {
        private ConcurrentDictionary<string, double> currencyRates = new ConcurrentDictionary<string, double>();

        internal FakeCurrencyRateRepository()
        {            
        }

        public double GetRate(string currencyName)
        {
            if (currencyRates.ContainsKey(currencyName))
                return currencyRates[currencyName];

            throw new System.ArgumentException($"Ukendt kryptovaluta {currencyName}");
        }

        public void SetRate(string currencyName, double rate)
        {
            currencyRates[currencyName] = rate;
        }
    }
}