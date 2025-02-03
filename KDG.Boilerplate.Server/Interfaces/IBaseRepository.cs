using System.Data;

namespace KDG.Boilerplate.Server.Services
{
    public interface IBaseRepository
    {
        Task WithConnection(Func<IDbConnection, Task> getData);
        Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData);
        Task<TResult> WithConnection<TRead, TResult>(Func<IDbConnection, Task<TRead>> getData, Func<TRead, Task<TResult>> process);
        Task WithTransaction(Func<IDbTransaction, Task> getData);
        Task<T> WithTransaction<T>(Func<IDbTransaction, Task<T>> getData);
    }
}