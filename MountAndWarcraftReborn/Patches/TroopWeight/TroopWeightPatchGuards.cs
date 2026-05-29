namespace MountAndWarcraftReborn.Patches.TroopWeight
{
    internal static class TroopWeightPatchGuards
    {
        [System.ThreadStatic]
        private static int _globalWeightSuppressionDepth;

        public static bool ShouldApplyGlobalWeight()
        {
            // Native mission deployment/spawn logic is extremely sensitive to raw roster counts.
            // Keep weighted counts on the campaign side, but do not let them bleed into live missions.
            return TaleWorlds.MountAndBlade.Mission.Current == null && _globalWeightSuppressionDepth <= 0;
        }

        public static System.IDisposable SuppressGlobalWeight()
        {
            _globalWeightSuppressionDepth++;
            return new GlobalWeightSuppressionScope();
        }

        private sealed class GlobalWeightSuppressionScope : System.IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _globalWeightSuppressionDepth--;
            }
        }
    }
}
