using Bec.Prime.Services;
using System;
using Xunit;

namespace Tests.Bec.Prime.Services
{
  /// <summary>
  /// Pattern 2: TestTargetClassName+Tests
  /// </summary>
  public class PrimeServiceTests
  {
    private readonly PrimeService _primeService;

    public PrimeServiceTests()
    {
      _primeService = new PrimeService(); //这种做法是有争议的，更多的建议，在每个测试中初始化。
    }

    #region IsPrime
    [Fact] //属性指示由测试运行程序运行的测试方法
    public void IsPrime_GivenValueOf1_ReturnFalse()
    {
      // Arrange & Act  -- recommend to have the 3A comments
      var result = _primeService.IsPrime(1);

      // Assert         -- recommend to have the 3A comments
      Assert.False(result, "1 should not be prime");
    }

    #region Theory
    [Theory] //表示执行相同代码，但具有不同输入参数的测试套件(test suites)
    [InlineData(-1)] //属性指定这些输入的值
    [InlineData(0)]
    [InlineData(1)]
    public void IsPrime_GivenValuesLessThan2_ReturnFalse(int value)
    {
      // Arrange & Act
      var result = _primeService.IsPrime(value);

      // Assert
      Assert.False(result, $"{value} should not be prime");
    }


    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void IsPrime_GivenPrimesLessThan10_ReturnTrue(int value)
    {
      // Arrange & Act
      var result = _primeService.IsPrime(value);

      // Assert
      Assert.True(result, $"{value} should be prime");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(9)]
    public void IsPrime_GivenNonPrimesLessThan10_ReturnFalse(int value)
    {
      // Arrange & Act
      var result = _primeService.IsPrime(value);

      // Assert
      Assert.False(result, $"{value} should not be prime");
    }
    #endregion
    #endregion

    #region IsPrimeStrict
    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void IsPrimeStrict_GivenLessThan2_ThrowsException(int value)
    {
      //Arrange 
      var service = new PrimeService();

      // Act
      Action actual= () => service.IsPrimeStrict(value);

      // Assert
      Assert.Throws<Exception>(actual);
    }
    #endregion
  }
}