using System;
namespace KDG.Boilerplate.Services
{
    public interface IExampleService
    {
        int Add(int a, int b);
        int Subtract(int a, int b);
    }

    public class ExampleService : IExampleService
    {

        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Subtract(int a, int b)
        {
            // This is intentionally wrong to demonstrate a failing test
            return a + b;
        }
    }
}
