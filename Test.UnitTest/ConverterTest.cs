using CryptoCurrency;
using System;
using Xunit;

namespace Test.UnitTest
{
    /// <summary>
    /// Testene dækker Ækvivalens partionerne beskrevet i Kryptovalutaer.pdf
    /// </summary>
    public class ConverterTest
    {
        #region Repository relateret test

        [Fact]
        public void Der_smides_argument_exception_for_ukendt_fravaluta()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.Convert("BTC", "ETH", 1));
        }

        /// <summary>
        /// Svaghed er at testen ikke kan garantere om det er fra- eller til-valutaen der forårsager exception.
        /// Det er ikke specificeret at exception fra converter skal melde tilbage om hvilken valuta der forårsager exception.
        /// i min impl fortæller converten hvilken valuta der fejler; men at asserte på indholdet af exception-message vil gøre testen ikke-resistent for refaktorering. 
        /// Heldigvis er vi dækket ind af anden test, der tester SetPricePerUnit som jo anvendes i arrange-delen.
        /// </summary>
        [Fact]
        public void Der_smides_argument_exception_for_ukendt_tilvaluta()
        {
            var sut = new Converter();
            sut.SetPricePerUnit("BTC", 41000);

            Assert.Throws<ArgumentException>(() => sut.Convert("BTC", "ETH", 1));
        }

        #endregion

        #region Test af pris
        
        [Fact]
        public void Der_smides_argument_exception_når_der_tildeles_negativ_pris()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("BTC", -0.01));
        }
        
        [Fact]
        public void Der_smides_argument_exception_når_der_tildeles_prisen_nul()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("BTC", 0));
        }

        [Fact]
        public void Tildeling_af_positiv_pris_er_ok()
        {
            var sut = new Converter();

            sut.SetPricePerUnit("BTC", 0.01);
        }

        [Fact]
        public void Pris_er_ikke_begrænset_af_decimal_præcision_udover_typen()
        {
            var sut = new Converter();

            sut.SetPricePerUnit("XRP", 0.0000000000000000000000000000000000000000000000000000000000000001);
        }

  
        #endregion

        #region Test af valuta navngivning


        [Fact]
        public void Der_smides_argument_exception_når_valutakodens_længde_er_nul()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("", 40000));
        }

        [Fact]
        public void Korte_navne_for_valutakode_er_ok()
        {
            var sut = new Converter();

            sut.SetPricePerUnit("A", 40000);
        }

        [Fact]
        public void Lange_case_insensitive_navne_for_valutakode_er_ok()
        {
            var sut = new Converter();

            sut.SetPricePerUnit("XXXkslklfusfsJHHHHHHHJJJJJJJJJJJJJJJJJJKKKKKKKKKKKKKKKKKKKKKKKKKKK", 10000);
        }

        [Theory]
        [InlineData("9BTC")]
        [InlineData("_BTX")]
        [InlineData(" BTC")]
        public void Der_smides_argument_exception_når_valutakoden_ikke_er_alfanumerisk(string currencyName)
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit(currencyName, 10000));
        }

        #endregion

        #region API test korrekthed

        [Fact]
        public void Pris_der_sættes_igen_overskriver_seneste_pris()
        {
            var sut = new Converter();
            sut.SetPricePerUnit("AAA", 1);
            sut.SetPricePerUnit("BBB", 2000);
            sut.SetPricePerUnit("BBB", 1);

            var actual = sut.Convert("AAA", "BBB", 1);

            Assert.Equal(1, actual);
        }

        [Theory]
        [InlineData("XXX", 100, "YYY", 100, 1, 1)]
        [InlineData("XXX", 100, "XXX", 100, 1007.8, 1007.8)] // identity conversion (man ved aldrig om der er speciel-implmenteret for at konvertere til sig selv)
        [InlineData("BTC", 40000, "ETH", 10000, 1, 4)]
        [InlineData("BTC", 40000, "ETH", 10000, 0.1, 0.4)] // en 1/10 fraktion
        [InlineData("BitCoin", 40000, "etH", 10000, 1, 4)] // case insensitive
        [InlineData("BTC", 40000, "ETH", 10000, 10, 40)]
        [InlineData("ETH", 10000, "BTC", 40000, 1, 0.25)] 
        [InlineData("AAA", 0.00000001, "BBB", 1, 5000000, 5000000E-08)] // small amounts
        [InlineData("AAA", 1.0E+08, "BBB", 1, 1, 1.0E+08)] // large price        
        [InlineData("zzz", 1, "eee", 14, 1, 0.07142857)]  // 8 decimal rundet ned; 0.0714285714285
        [InlineData("zzz", 1, "eee", 66, 1, 0.01515152)]  // 8 decimal rundet op; 0.01515151515....
        [InlineData("zzz", 1.0E-30, "eee", 1.0E+30, 1, 0)]  // afrundet til 0 stk 'eee' (så du ender med at give din 'zzz' væk til ingenting pga afrunding)
        public void Der_konverteres_korrekt(
            string fraValuta, 
            double prisFraValuta, 
            string tilValuta, 
            double prisTilValuta, 
            double fraAntal, 
            double tilForventetlBeloeb)
        {
            var sut = new Converter();
            sut.SetPricePerUnit(fraValuta, prisFraValuta);
            sut.SetPricePerUnit(tilValuta, prisTilValuta);

            var actual = sut.Convert(fraValuta, tilValuta, fraAntal);

            Assert.Equal(tilForventetlBeloeb, actual);
        }

        #endregion
    }
}
