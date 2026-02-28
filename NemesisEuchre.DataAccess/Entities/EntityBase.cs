namespace NemesisEuchre.DataAccess.Entities;

public abstract class EntityBase
{
    public DateTime CreateDate { get; set; }

    public DateTime ModifyDate { get; set; }
}
