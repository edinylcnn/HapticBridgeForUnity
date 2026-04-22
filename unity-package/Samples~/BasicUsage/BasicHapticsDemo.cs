using LogiHaptics;
using UnityEngine;

namespace LogiHaptics.Samples
{
    public class BasicHapticsDemo : MonoBehaviour
    {
        void OnEnable()
        {
            Debug.Log($"[LogiHaptics] Available: {LogiHapticsUnity.IsAvailable}");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) LogiHapticsUnity.Trigger(HapticEvent.Click);
            if (Input.GetKeyDown(KeyCode.Alpha2)) LogiHapticsUnity.Trigger(HapticEvent.Success);
            if (Input.GetKeyDown(KeyCode.Alpha3)) LogiHapticsUnity.Trigger(HapticEvent.Failure);
            if (Input.GetKeyDown(KeyCode.Alpha4)) LogiHapticsUnity.Trigger(HapticEvent.Achievement);
            if (Input.GetKeyDown(KeyCode.Alpha5)) LogiHapticsUnity.Trigger(HapticEvent.ImpactMedium);
        }

        void OnDestroy() => LogiHapticsUnity.Shutdown();
    }
}
