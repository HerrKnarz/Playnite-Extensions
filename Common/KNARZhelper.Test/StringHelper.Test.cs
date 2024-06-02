using Xunit;

namespace KNARZhelper.Test
{
    public class StringHelperTest
    {
        [Theory]
        [InlineData("   This is   a test      .", "This is a test .")]
        public void TestCollapseWhitespaces(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.CollapseWhitespaces());

        [Theory]
        [InlineData("This is a test with 1 and 6 and 12.", "This is a test with I and VI and III.")]
        public void TestDigitsToRomanNumbers(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.DigitsToRomanNumbers());

        [Theory]
        [InlineData("This is a über test.", "This%20is%20a%20%C3%BCber%20test.")]
        public void TestEscapeDataString(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.EscapeDataString());

        [Theory]
        [InlineData("This is a \"test\".", "This is a \\\"test\\\".")]
        public void TestEscapeQuotes(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.EscapeQuotes());

        [Theory]
        [InlineData("This is a test with äÖüß and other chars.", "This is a test with aOuss and other chars.")]
        public void TestRemoveDiacritics(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.RemoveDiacritics());

        [Theory]
        [InlineData("Doom HD", "Doom")]
        [InlineData("Test: Legendary Edition", "Test")]
        [InlineData("Blubber Remastered", "Blubber")]
        [InlineData("Tester Collection", "Tester")]
        [InlineData("Super Game: Extra Cut", "Super Game")]
        [InlineData("Sonic 06 Game Of The Year Edition", "Sonic 06")]
        [InlineData("Game without edition", "Game")]
        [InlineData("Just a normal game", "Just a normal game")]
        public void TestRemoveEditionSuffix(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.RemoveEditionSuffix());

        [Theory]
        [InlineData("This is a test with äÖüß and other chars.", "This is a test with  and other chars")]
        public void TestRemoveSpecialChars(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.RemoveSpecialChars());

        [Theory]
        [InlineData("This is a (test)", "(", ")", "This is a ")]
        [InlineData("This (is) a (test)", "(", ")", "This  a ")]
        [InlineData("This <testis</test> a (test)</test>", "<test", "</test>", "This  a (test)</test>")]
        public void TestRemoveTextBetween(string stringToCheck, string removeFrom, string removeTo, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.RemoveTextBetween(removeFrom, removeTo));

        [Theory]
        [InlineData("This IS a tesT!", "This Is A Test!")]
        public void TestToTitleCase(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.ToTitleCase());

        [Theory]
        [InlineData("This is a über test.", "This+is+a+%c3%bcber+test.")]
        public void TestUrlEncode(string stringToCheck, string stringResult)
            => Assert.Equal(stringResult, stringToCheck.UrlEncode());
    }
}