namespace ProjectManagement.Domain.Interfaces
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}
