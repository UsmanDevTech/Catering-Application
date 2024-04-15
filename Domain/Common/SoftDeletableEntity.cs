
namespace Domain.Common;

public abstract class SoftDeletableEntity
{

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    //public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void RemoveDelete() => IsDeleted = false;

}
