using Bec;
using Moq;
using Xunit;

namespace Tests.Bec
{
  public class ClassWithDependencyTests
  {
    [Fact]
    public void GetAliasName_GivenAbc_ReturnXyz()
    {
      // Arrange a Mock
      const string name = "Abc";
      const string expected = "Xyz";

      var moq = new Mock<IDependency>();
      moq.Setup(di => di.GetNickName(name))
         .Returns(expected);

      var service = new ClassWithDependency(moq.Object);

      // Act
      var actual = service.GetAliasName(name);

      // Assert
      moq.Verify(di => di.GetNickName(name), Times.Once);
      Assert.Equal(expected, actual);
    }
  }
}
