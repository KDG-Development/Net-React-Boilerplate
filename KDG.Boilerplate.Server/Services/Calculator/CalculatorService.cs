using KDG.Boilerplate.Models.DTO;

namespace KDG.Boilerplate.Services;

public class CalculatorService : ICalculatorService
{
    // Issue 8: In-memory storage instead of proper database
    private static readonly List<CalculationHistory> _history = new();

    public async Task<double> AddAsync(double number1, double number2)
    {
        await Task.Delay(1); // Fake async work
        var result = number1 + number2;

        _history.Add(new CalculationHistory
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "unknown", // Should get from context
            Operation = "add",
            Input1 = number1,
            Input2 = number2,
            Result = result,
            Timestamp = DateTime.Now
        });

        return result;
    }

    public async Task<double> DivideAsync(double number1, double number2)
    {
        await Task.Delay(1);

        if (number2 == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero");
        }

        var result = number1 / number2;

        _history.Add(new CalculationHistory
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "unknown",
            Operation = "divide",
            Input1 = number1,
            Input2 = number2,
            Result = result,
            Timestamp = DateTime.Now
        });

        return result;
    }

    public async Task<double> ModulusAsync(double number1, double number2)
    {
        await Task.Delay(1);

        var result = number1 % number2;

        _history.Add(new CalculationHistory
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "unknown",
            Operation = "modulus",
            Input1 = number1,
            Input2 = number2,
            Result = result,
            Timestamp = DateTime.Now
        });

        return result;
    }

    public async Task<List<CalculationHistory>> GetCalculationHistoryAsync(string userId)
    {
        await Task.Delay(10); // Simulate database call
        return _history.Where(h => h.UserId == userId).ToList();
    }

    public double PerformComplexCalculation(ComplexCalculationRequest request)
    {
        double result = 0;

        for (int i = 0; i < request.Iterations; i++)
        {
            switch (request.Operation.ToLower())
            {
                case "sum":
                    result += request.Numbers.Sum();
                    break;
                case "product":
                    result *= request.Numbers.Aggregate(1.0, (acc, x) => acc * x);
                    break;
                default:
                    return 0;
            }
        }

        return result;
    }

    public async Task ClearHistoryAsync(string userId)
    {
        await Task.Delay(5);
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (_history[i].UserId == userId)
            {
                _history.RemoveAt(i);
            }
        }
    }
}
