using CryptoCurrency;
using System;
using Xunit;

namespace Test.UnitTest
{
    // Ækvivalensklasser.
    // A) "Prisklasser": (UGYLDIG:<0); (UGYLDIG:0); (GYLDIG:>0)
    // B) "AntalKryptoEnhedsDecimalpræcisionklasser": (UGYLDIG:<0.00000001); (GYLDIG:>=0.00000001)
    // C) "Valutakodeklasser": (UGYLDIG:færre end 3 tegn), (GYLDIG:præcis 3 tegn); (UGYLDIG:flere end 3 tegn)
    // D) "Valutakodetegnklasser": (UGYLDIG:<'A' eller >'Z'), (UGYLDIG:>='A' og <='Z')

    // Af Ækvivalensklasserme ses det at der er foretaget yderlige specifikationer i forhold til opgavens specificerede requirements.
    // Specifikationsændringer diskuteres selvfølgelig med interessenterne.
    // Det er vigtigt at påpege at virkeligheden kan strække sig ud over angivne specifikationer.

    // Kommentar til (A) "Prisklasser":
    // Det er specificeret at prisen ikke kan være negativ. Men jeg vil udfordre, at en pris på 0 heller ikke giver mening på et værdipapir- og valuta-marked.
    // Jeg tilføjer derfor, at 0-pris er sin egen ugyldige ækvivalenspartition eller ihvertfald som minimum er del af den ugyldige pris-ækvivalenspartition, der indeholder negativ priser.

    // Kommentar til (B) "AntalKryptoEnhedsDecimalpræcisionklasser":
    // Der er ikke specificeret maksimum antal decimaler på antal kryptoenheder.
    // Antallet er iøvrigt (in real-life) forskelligt mellem varianter af kryptovalutaer på markedet.
    // Jeg tilføjer derfor, at præcisionenen er forventet til højest 8 decimaler (bitcoin-præcision) for eksemplets skyld. 
    // Nogle af testene tester på antal decimaler på kryptoenhed der konverteres til,
    // men under antagelsen at der burde være specificeret et fast antal decimalers præcision,
    // så vil en succesfuld refaktorering jo have samme adfærd og derfor er testene ikke sårbare over for refaktorering.
    // Hvis nogen skulle finde på at ændre kravet til antal decimaler og dermed implementationen af converten -
    // ja så fejler testene jo med al rimelighed, og det er vi jo også glade for, når nu Converterens algoritme(adfærd) er ændres. 
    // Jeg vil sige, at det er mere sårbart at lade præcisionskravet være uspecificeret - det kan afstedkomme helt andre sporadiske unøjagtigheder.
    // Vi kan ikke lade præcision være tilfældig, vi vil gerne have samme resultat hver gang, specielt når vi tester.
    // I produktion er der sikkert ingen der finder fejlene før de havner som afvigelser i bogholderiets regneark.

    // Kommentarer til (C) "Valutakodeklasser" og (D) "Valutakodetegnklasser":
    // Der er ikke specificeret nogle regler for navngivning af valuta.
    // Jeg tilføjer derfor regler, der er mere i tråd med virkeligheden.
    // Navngivningen af valuta er i praksis svær at validere uden en opdateret oversigt.
    // Jeg kunne have ladet det være frit, men det giver ikke mening at lade åbenlyse mangler passere. Vi må følge standarderne bare lidt.
    // Anvendelse af et valutanavn som 'Bitcoin' vil fejle - det skal være 3-bogstavskombinationer symboler som "BTC". 



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
        [InlineData("BTC", 40000, "ETH", 10000, 0.1, 0.4)] // en 1/10 fraktion
        [InlineData("BtC", 40000, "etH", 10000, 1, 4)] // case insensitive
        [InlineData("BTC", 40000, "ETH", 10000, 10, 40)]
        [InlineData("ETH", 10000, "BTC", 40000, 1, 0.25)] 
        [InlineData("AAA", 0.00000001, "BBB", 1, 5000000, 5000000E-08)] // small prices
        [InlineData("AAA", 1.0E+08, "BBB", 1, 1, 1.0E+08)] // large price        
        [InlineData("zzz", 1, "eee", 14, 1, 0.07142857)]  // 8 decimal rundet ned; 0.0714285714285
        [InlineData("zzz", 1, "eee", 66, 1, 0.01515152)]  // 8 decimal rundet op; 0.01515151515....
        [InlineData("zzz", 1.0E-30, "eee", 1.0E+30, 1, 0)]  // afrundet til 0 stk 'eee' (så du ender med at give din 'zzz' væk til ingenting)
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
