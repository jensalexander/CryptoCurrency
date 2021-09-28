using System;

namespace CryptoCurrency
{
    public class Converter
    {

        private readonly IFakeCurrencyRateRepository currencyRateRepository = new FakeCurrencyRateRepository();

        /// <summary>
        /// Angiver prisen for en enhed af en kryptovaluta. Prisen angives i dollars.
        /// Hvis der tidligere er angivet en værdi for samme kryptovaluta, 
        /// bliver den gamle værdi overskrevet af den nye værdi
        /// </summary>
        /// <param name="currencyName">Navnet på den kryptovaluta der angives</param>
        /// <param name="price">Prisen på en enhed af valutaen målt i dollars. Prisen kan ikke være negativ</param>
        public void SetPricePerUnit(String currencyName, double price)
        {
            var currency = currencyName.ToUpper();
            var afrundetCurrency = RoundAmount(price);


            if (price <= 0)
                throw new ArgumentException($"Ugyldig negativ pris: {price}$");

            if (price != afrundetCurrency)
                throw new ArgumentException($"Ugyldig præcision på pris: {price}$");

            if (currency.Length != 3)
                throw new ArgumentException($"Ugyldig længde på valuta {currencyName}");
            
            foreach(var c in currency)
                if(c < 'A' || c > 'Z')
                    throw new ArgumentException($"Ugyldige tegn i valuta {currencyName}");

            currencyRateRepository.SetRate(currency, afrundetCurrency);
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
            var fromRate = currencyRateRepository.GetRate(fromCurrencyName.ToUpper());
            var toRate = currencyRateRepository.GetRate(toCurrencyName.ToUpper());
            return (fromRate/ toRate) * amount;
        }


        /// <summary>
        /// De forskellig krypto valutaer har forskellig præcision.
        /// Der er intet specificeret om decimal præcision. Her antages 8 decimaler uanset valuta 
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        private double RoundAmount(double price)
        {
            return Math.Round(price, 8, MidpointRounding.ToEven);
        }
    }
}
