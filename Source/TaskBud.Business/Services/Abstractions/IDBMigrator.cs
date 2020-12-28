using System.Threading.Tasks;

namespace TaskBud.Business.Services.Abstractions
{
    public interface IDBMigrator
    {
        Task ExecuteAsync();
    }
}