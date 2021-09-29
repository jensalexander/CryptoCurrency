using System;

namespace CryptoCurrency
{
    public class Converter
    {
        private const int _decimalPrecisionUnits = 8; // need to be specified and locked for correctness and consistency. 
        
        private readonly IFakeCurrencyRateRepository currencyRateRepository = new FakeCurrencyRateRepository();

        private double RoundAmount(double price) => Math.Round(price, _decimalPrecisionUnits, MidpointRounding.ToEven);

        private string NormalizeName(string currencyName) => currencyName.ToUpper();

        /// <summary>
        /// Angiver prisen for en enhed af en kryptovaluta. Prisen angives i dollars.
        /// Hvis der tidligere er angivet en værdi for samme kryptovaluta, 
        /// bliver den gamle værdi overskrevet af den nye værdi
        /// </summary>
        /// <param name="currencyName">Navnet på den kryptovaluta der angives</param>
        /// <param name="price">Prisen på en enhed af valutaen målt i dollars. Prisen kan ikke være negativ</param>
        public void SetPricePerUnit(String currencyName, double price)
        {
            var currency = NormalizeName( currencyName);

            if (price <= 0)
                throw new ArgumentException($"Ugyldig negativ pris: {price}$");

            if (currency.Length == 0)
                throw new ArgumentException($"Ugyldig længde på valuta {currencyName}");
            
            foreach(var c in currency)
                if(c < 'A' || c > 'Z')
                    throw new ArgumentException($"Ugyldige tegn i valuta {currencyName}");

            currencyRateRepository.SetRate(currency, price);
        }

        /// <summary>
        /// Konverterer fra en kryptovaluta til en anden. 
        /// Hvis en af de angivne valutaer ikke findes, kaster funktionen en ArgumentException
        /// 
        /// </summary>
        /// <param name="fromCurrencyName">Navnet på den valuta, der konverterers fra</param>
        /// <param name="toCurrencyName">Navnet på den valuta, der konverteres til</param>
        /// <param name="amount">Beløbet angivet i valutaen angivet i fromCurrencyName</param>
        /// <returns>Værdien af beløbet i toCurrencyName</returns>
        public double Convert(String fromCurrencyName, String toCurrencyName, double amount) 
        {            
            var fromRate = currencyRateRepository.GetRate(NormalizeName( fromCurrencyName));
            var toRate = currencyRateRepository.GetRate(NormalizeName( toCurrencyName));
            var convertedAmount = (fromRate / toRate) * amount;
            return RoundAmount(convertedAmount);
        }
        
    }
}
