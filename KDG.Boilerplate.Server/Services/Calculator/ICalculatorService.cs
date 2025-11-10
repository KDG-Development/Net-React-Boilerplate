using KDG.Boilerplate.Models.DTO;

namespace KDG.Boilerplate.Services;

public interface ICalculatorService
{
    Task<double> AddAsync(double number1, double number2);
    Task<double> DivideAsync(double number1, double number2);
    Task<List<CalculationHistory>> GetCalculationHistoryAsync(string userId);
    double PerformComplexCalculation(ComplexCalculationRequest request);
    Task ClearHistoryAsync(string userId);
}
