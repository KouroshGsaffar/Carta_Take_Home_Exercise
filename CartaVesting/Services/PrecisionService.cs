using System;

namespace CartaVesting.Services
{
    public interface IPrecisionService
    {
        decimal Truncate(decimal value, int precision);
    }

    public class PrecisionService : IPrecisionService
    {
        public decimal Truncate(decimal value, int precision)
        {
            if (precision < 0) throw new ArgumentException("Precision cannot be negative");
            
            // Logic: Multiply by 10^N, floor it, divide by 10^N
            decimal step = (decimal)Math.Pow(10, precision);
            return Math.Floor(value * step) / step;
        }
    }
}