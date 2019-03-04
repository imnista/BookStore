using System;

namespace Bec.Prime.Services
{
  public class PrimeService
  {
    public bool IsPrime(int candidate)
    {
      if (candidate < 2)
      {
        return false;
      }

      for (var divisor = 2; divisor <= Math.Sqrt(candidate); divisor++)
      {
        if (candidate % divisor == 0)
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Use strict prime number definition refer to https://en.wikipedia.org/wiki/Prime_number
    /// </summary>
    /// <param name="candidate"></param>
    /// <returns></returns>
    /// <exception cref="Exception">
    /// Ocurrs when the <paramref name="candidate"/> is less than 2.
    /// </exception>
    public bool IsPrimeStrict(int candidate)
    {
      if (candidate < 2)
      {
        throw new Exception($"Invalide input number: {candidate}");
      }

      return IsPrime(candidate);
    }
  }
}