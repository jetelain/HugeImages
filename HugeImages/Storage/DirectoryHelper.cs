namespace Pmad.HugeImages.Storage
{
    internal static class DirectoryHelper
    {
        public static void CleanupDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    // Method is used by Dispose() of temporary storage implementation, we should not throw any exception
                    // Failure is acceptable, temporary files will be cleaned up by the OS eventually, but log the failure for diagnostics
                    System.Diagnostics.Trace.TraceWarning($"Failed to cleanup directory '{0}': {1}", path, ex.Message);
                }
            }
        }
    }
}
