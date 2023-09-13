namespace HugeImages.Storage
{
    public interface IHugeImageStorage
    {
        IHugeImageStorageSlot CreateSlot(string name, HugeImageSettingsBase settings);
    }
}
