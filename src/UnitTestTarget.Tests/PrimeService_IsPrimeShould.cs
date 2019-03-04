using Bec.Prime.Services;
using Xunit;

namespace Tests.Bec.Prime.Services
{
  /// <summary>
  /// Pattern 1: TestTargetClassName_TestMethodName+Should
  /// </summary>
  public class PrimeService_IsPrimeShould
  {
    private readonly PrimeService _primeService;

    public PrimeService_IsPrimeShould()
    {
      _primeService = new PrimeService();
    }

    [Fact] //属性指示由测试运行程序运行的测试方法
    public void ReturnFalseGivenValueOf1()
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
    public void ReturnFalseGivenValuesLessThan2(int value)
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
    public void ReturnTrueGivenPrimesLessThan10(int value)
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
    public void ReturnFalseGivenNonPrimesLessThan10(int value)
    {
      // Arrange & Act
      var result = _primeService.IsPrime(value);

      // Assert
      Assert.False(result, $"{value} should not be prime");
    }
    #endregion
  }
}