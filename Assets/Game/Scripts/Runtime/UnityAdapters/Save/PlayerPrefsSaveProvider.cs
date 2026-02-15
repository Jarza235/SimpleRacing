using UnityEngine;

namespace Game.Runtime
{
    // This class is a simple implementation of ISaveProvider using Unity's PlayerPrefs.
    // It's not intended for production use, but rather as a demonstration of how to implement the ISaveProvider interface.
    public sealed class PlayerPrefsSaveProvider : ISaveProvider
    {
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public float GetFloat(string key, float defaultValue) =>
            PlayerPrefs.GetFloat(key, defaultValue);

        public void SetFloat(string key, float value) =>
            PlayerPrefs.SetFloat(key, value);

        public void Save() => PlayerPrefs.Save();
    }
}
