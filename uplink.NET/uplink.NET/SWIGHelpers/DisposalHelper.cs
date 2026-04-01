using System;
using System.Reflection;

namespace uplink.NET.SWIGHelpers
{
    /// <summary>
    /// Helper class to prevent double-free issues with SWIG-generated wrappers.
    /// When native cleanup functions are called (like uplink_close_project, uplink_close_download, etc.),
    /// we need to tell the SWIG wrapper that it no longer owns the memory, preventing it from
    /// trying to free the same resource again during Dispose().
    /// This is critical for .NET 6+ on Linux where duplicate frees cause SEGFAULTs.
    /// </summary>
    internal static class DisposalHelper
    {
        /// <summary>
        /// Clears ownership of the native memory from a SWIG-generated wrapper object.
        /// This should be called after the native cleanup function has been invoked to prevent double-free.
        /// </summary>
        /// <param name="swigObject">The SWIG-generated object</param>
        public static void ClearOwnership(IDisposable swigObject)
        {
            if (swigObject == null)
                return;

            try
            {
                var type = swigObject.GetType();
                var ownershipField = type.GetField("swigCMemOwn", BindingFlags.Instance | BindingFlags.NonPublic);
                var handleField = type.GetField("swigCPtr", BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (ownershipField != null && ownershipField.FieldType == typeof(bool))
                {
                    ownershipField.SetValue(swigObject, false);
                }

                if (handleField != null && handleField.FieldType == typeof(System.Runtime.InteropServices.HandleRef))
                {
                    handleField.SetValue(swigObject, new System.Runtime.InteropServices.HandleRef(null, IntPtr.Zero));
                }
            }
            catch
            {
                // If reflection fails for any reason, silently ignore
                // The worst case is we might get a double-free attempt,
                // but that's better than crashing here
            }
        }
    }
}
