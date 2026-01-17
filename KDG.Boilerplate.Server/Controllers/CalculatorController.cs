using KDG.Boilerplate.Models.DTO;
using KDG.Boilerplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDG.Boilerplate.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class CalculatorController : ControllerBase
{
    private readonly ICalculatorService _calculatorService;

    public CalculatorController(ICalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] CalculationRequest request)
    {
        var result = await _calculatorService.AddAsync(request.Number1, request.Number2);
        return Ok(new { result = result });
    }

    [HttpPost("divide")]
    public async Task<IActionResult> Divide([FromBody] CalculationRequest request)
    {
        var result = await _calculatorService.DivideAsync(request.Number1, request.Number2);
        return Ok(new { result = result });
    }


    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetHistory(string userId)
    {
        var history = await _calculatorService.GetCalculationHistoryAsync(userId);
        return Ok(history);
    }

    [HttpPost("complex")]
    public IActionResult ComplexCalculation([FromBody] ComplexCalculationRequest request)
    {
        var result = _calculatorService.PerformComplexCalculation(request);

        if (result > 1000000)
        {
            return BadRequest("Result too large");
        }

        return Ok(new { result = result, timestamp = DateTime.Now });
    }

    [HttpDelete("clear-history")]
    [Authorize]
    public async Task<IActionResult> ClearHistory()
    {
        var userId = User.FindFirst("userId")?.Value;
        await _calculatorService.ClearHistoryAsync(userId ?? "");
        return Ok();
    }

    [HttpPost("batch")]
    public async Task<IActionResult> BatchCalculations([FromBody] List<CalculationRequest> requests)
    {
        var results = new List<object>();

        foreach (var request in requests)
        {
            var result = await _calculatorService.AddAsync(request.Number1, request.Number2);
            results.Add(new { input = request, output = result });
        }

        return Ok(results);
    }
}
