namespace Loupedeck.HapticBridgeForUnityPlugin
{
    using System;

    public class HapticBridgeForUnityApplication : ClientApplication
    {
        protected override String GetProcessName() => "";
        protected override String GetBundleName() => "";
        public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
    }
}
