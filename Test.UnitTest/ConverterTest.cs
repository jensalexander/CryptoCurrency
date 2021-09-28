using CryptoCurrency;
using System;
using Xunit;

namespace Test.UnitTest
{
    // Ækvivalensklasser.
    // "Prisklasser": (UGYLDIG:<=0); (GYLDIG:>0)
    // "Priddecimalpræcisionklasser": (UGYLDIG:<0.00000001); (GYLDIG:>=0.00000001)
    // "Valutakodeklasser": (UGYLDIG:færre end 3 tegn), (GYLDIG:præcis 3 tegn); (UGYLDIG:flere end 3 tegn)
    // "Valutakodetegnklasser": (UGYLDIG:<'A' eller >'Z'), (UGYLDIG:>='A' og <='Z')

    // Som det ses er der lavet yderlige specifikationer i forhold til requirements.
    // Det skal selvfølgelig diskuteres med interessenterne. Det er vigtigt at påpege at virkeligheden er en anden tidligt.
    
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
        /// Heldigvis er vi dækket ind af anden test der tester metoden SetPricePerUnit dom anvendes i arrange-delen.
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

        /// <summary>
        /// Uspecificeret hvad der skal ske for negativ pris. 
        /// Almen viden siger mig at vi skal smide en exception - vi vil hellere fejle tidligt.
        /// </summary>
        [Fact]
        public void Der_smides_argument_exception_når_der_tildeles_negativ_pris()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("BTC", -0.01));
        }
        
        /// <summary>
        /// Uspecificeret hvad der skal ske for nul pris. 
        /// Almen viden siger mig at vi skal smide en exception - vi vil hellere fejle tidligt.
        /// </summary>
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

        // Det er oprindeligt uspecificeret omkring decimal præcision.
        // Præcisionen varierer iøvrigt mellem forskellige valutaer i virkeligheden.
        // I løsningen er valgt 8 decimalers nøjagtighed.
        [Fact]
        public void Der_smides_argument_exception_når_der_angives_flere_end_8_decimaler()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("XRP", 0.000000001));
        }

        [Fact]
        public void pris_med_8_eller_færre_decimaler_er_ok()
        {
            var sut = new Converter();

            sut.SetPricePerUnit("XRP", 0.00000001);
        }

        #endregion

        #region Test af valuta navngivning

        /// <summary>
        /// Det er oprindeligt uspecificeret om navngivning omkring valutakoden - skal navgivning følge iso standard ?
        /// Det er lidt svært at validere korrektheden på navngivningen i det hele taget, så for nu antages blot bogstavskombinationer af tre.
        /// </summary>
        [Fact]
        public void Der_smides_argument_exception_når_valutakodens_længde_er_mindre_end_tre()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("BT", 40000));
        }

        [Fact]
        public void Der_smides_argument_exception_når_valutakodens_længde_er_nul()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("", 40000));
        }

        [Fact]
        public void Der_smides_argument_exception_når_valutakodens_længde_er_større_end_tre()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("BTCX", 40000));
        }

        [Fact]
        public void Der_smides_ikke_exception_når_valutakodens_længde_er_præcis_tre()
        {
            var sut = new Converter();

            sut.SetPricePerUnit("XXX", 10000);
        }

        [Fact]
        public void Der_smides_argument_exception_når_valutakoden_ikke_er_alfanumerisk()
        {
            var sut = new Converter();

            Assert.Throws<ArgumentException>(() => sut.SetPricePerUnit("9_ ", 10000));
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
        [InlineData("BTC", 40000, "ETH", 10000, 1, 4)]
        [InlineData("BTC", 40000, "ETH", 10000, 10, 40)]
        [InlineData("ETH", 10000, "BTC", 40000, 1, 0.25)]
        [InlineData("AAA", 0.00000001, "BBB", 1, 5000000, 5000000E-08)]
        [InlineData("AAA", 1.0E+08, "BBB", 1, 1, 1.0E+08)]
        public void Der_konverteres_korrekt(string fraValuta, double prisFraValuta, string tilValuta, double prisTilValuta, double fraAntal, double tilForventetlBeloeb)
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
