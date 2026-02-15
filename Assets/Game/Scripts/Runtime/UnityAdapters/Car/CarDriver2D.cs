using UnityEngine;

namespace Game.Runtime
{
    public sealed class CarDriver2D : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private CarConfig _carConfig;

        [Header("References")]
        [SerializeField] private Rigidbody2D _frontWheel;
        [SerializeField] private Rigidbody2D _rearWheel;

        [SerializeField] private Rigidbody2D _body;
        private float _throttleInput;

        private void Awake()
        {
            if (_carConfig == null)
            {
                Debug.LogError($"{nameof(CarDriver2D)}: CarConfig missing!", this);
                enabled = false;
                return;
            }

            if (_frontWheel == null || _rearWheel == null || _body == null)
            {
                Debug.LogError($"{nameof(CarDriver2D)}: Rigidbody2D reference missing!", this);
                enabled = false;
                return;
            }
        }

        public void SetThrottle(float value)
        {
            _throttleInput = Mathf.Clamp(value, -1f, 1f);
        }

        private void FixedUpdate()
        {
            ApplyAngularVelocityToTyres();
        }

        private void ApplyAngularVelocityToTyres()
        {
            float throttle = _throttleInput;
            if (Mathf.Approximately(throttle, 0f)) {
                return;
            }

            float motorTorque = _carConfig.motorTorque * throttle;
            float maxWheelAngularSpeed = _carConfig.maxWheelAngularSpeed; // degrees/sec

            ApplyTorqueToWheel(_rearWheel, motorTorque, maxWheelAngularSpeed);
            ApplyTorqueToWheel(_frontWheel, motorTorque, maxWheelAngularSpeed);
        }

        private static void ApplyTorqueToWheel(
            Rigidbody2D wheel,
            float torque,
            float maxAngularSpeed)
        {
            float absAngularVelocity = Mathf.Abs(wheel.angularVelocity);

            if (absAngularVelocity < maxAngularSpeed)
            {
                wheel.AddTorque(torque, ForceMode2D.Force);
            }
        }
    }
}
