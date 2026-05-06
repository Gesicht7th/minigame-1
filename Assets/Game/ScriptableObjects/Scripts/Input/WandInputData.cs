using UnityEngine;

// Assets/_Game/Scripts/Input/WandInputData.cs

namespace WizardPunk
{
    /// <summary>
    /// Struct data mentah dari ESP32 serial
    /// </summary>
    [System.Serializable]
    public struct WandInputData
    {
        // Accelerometer (raw int16 dari MPU6050)
        public float ax;
        public float ay;
        public float az;

        // Gyroscope (raw int16 dari MPU6050)
        public float gx;
        public float gy;
        public float gz;

        // Apakah data ini valid (berhasil di-parse)
        public bool IsValid;

        public static WandInputData Invalid => new WandInputData { IsValid = false };

        public override string ToString()
        {
            return $"Accel[{ax:F0},{ay:F0},{az:F0}] Gyro[{gx:F0},{gy:F0},{gz:F0}]";
        }
    }
}