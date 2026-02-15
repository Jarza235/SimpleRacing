using UnityEngine;

namespace Game.Runtime
{
    [CreateAssetMenu(
        fileName = "CarConfig",
        menuName = "Game/Configs/Car Config",
        order = 1)]
    public sealed class CarConfig : ScriptableObject
    {
        [Header("Acceleration")]
        [Tooltip("Torque applied to wheels")]
        [Min(0f)]
        public float motorTorque = 20f;

        [Header("Max Wheel Angular Speed")]
        [Min(0f)]
        public float maxWheelAngularSpeed = 15f;
    }
}
