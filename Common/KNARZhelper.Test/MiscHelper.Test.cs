using System;

using Xunit;

namespace KNARZhelper.Test
{
    public class MiscHelperTest
    {
        [Theory]
        [InlineData(0, "1970-01-01T01:00:00")]
        [InlineData(1678890851.45678, "2023-03-15T15:34:11.4570000+01:00")]
        [InlineData(1678890851.45638, "2023-03-15T15:34:11.4560000+01:00")]
        public void TestUnixTimeStampToDateTime(double unixTimeStamp, DateTime dateTime) => Assert.Equal(dateTime, MiscHelper.UnixTimeStampToDateTime(unixTimeStamp));
    }
}
