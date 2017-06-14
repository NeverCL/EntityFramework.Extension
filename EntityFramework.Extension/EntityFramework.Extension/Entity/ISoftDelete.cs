namespace EntityFramework.Extension.Entity
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
