using Xunit;

namespace KNARZhelper.Test
{
    public class StringHelperTest
    {
        [Theory]
        [InlineData("This is a test with äÖüß and other chars.", "This is a test with  and other chars")]
        [InlineData("Trüberbrook", "Trberbrook")]
        public void TestRemoveSpecialChars(string stringWithUmlaut, string stringWithoutUmlaut)
        {
            Assert.Equal(stringWithoutUmlaut, stringWithUmlaut.RemoveSpecialChars());
        }

        [Theory]
        [InlineData("This is a test with äÖüß and other chars.", "This is a test with aOuss and other chars.")]
        [InlineData("Trüberbrook", "Truberbrook")]
        public void TestRemoveDiactritics(string stringWithUmlaut, string stringWithoutUmlaut)
        {
            Assert.Equal(stringWithoutUmlaut, stringWithUmlaut.RemoveDiacritics());
        }

        [Theory]
        [InlineData("   This is   a test      .", "This is a test .")]
        [InlineData("Trüber  brook", "Trüber brook")]
        public void TestCollapesWhitespaces(string stringWithUmlaut, string stringWithoutUmlaut)
        {
            Assert.Equal(stringWithoutUmlaut, stringWithUmlaut.CollapseWhitespaces());
        }
    }
}