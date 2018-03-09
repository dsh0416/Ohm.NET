using Xunit;

namespace Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(2, Add(1, 1));
        }

        public int Add(int a, int b) => a + b;
    }
}