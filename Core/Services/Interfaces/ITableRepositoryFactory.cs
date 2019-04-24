namespace Core.Services.Interfaces
{
    public interface ITableRepositoryFactory<T> where T: class, new()
    {
        ITableRepository<T> GetInstance(string connectionString, string tableName);
    }
}