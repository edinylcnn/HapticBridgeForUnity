using HapticBridge;
using UnityEngine;

namespace HapticBridge.Samples
{
    public class BasicHapticsDemo : MonoBehaviour
    {
        void OnEnable()
        {
            Debug.Log($"[HapticBridge] Available: {HapticsBridge.IsAvailable}");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) HapticsBridge.Trigger(HapticEvent.Click);
            if (Input.GetKeyDown(KeyCode.Alpha2)) HapticsBridge.Trigger(HapticEvent.Success);
            if (Input.GetKeyDown(KeyCode.Alpha3)) HapticsBridge.Trigger(HapticEvent.Failure);
            if (Input.GetKeyDown(KeyCode.Alpha4)) HapticsBridge.Trigger(HapticEvent.Achievement);
            if (Input.GetKeyDown(KeyCode.Alpha5)) HapticsBridge.Trigger(HapticEvent.ImpactMedium);
        }

        void OnDestroy() => HapticsBridge.Shutdown();
    }
}
