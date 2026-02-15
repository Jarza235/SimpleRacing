namespace Game.Runtime
{
    public interface ISaveProvider
    {
        bool HasKey(string key);

        float GetFloat(string key, float defaultValue);
        void SetFloat(string key, float value);

        void Save();
    }
}
