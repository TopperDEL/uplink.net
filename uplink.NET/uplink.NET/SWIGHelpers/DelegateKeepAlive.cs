using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace uplink.NET.SWIGHelpers
{
    /// <summary>
    /// Helper class to prevent SWIG callback delegates from being garbage collected.
    /// On .NET Core/5+ on Linux, the GC is more aggressive than Mono and can collect delegates
    /// that have been passed to native code, causing SEGFAULT when native code tries to call them.
    /// 
    /// This class uses reflection to access the SWIG-generated delegates and pins them with GCHandle
    /// to prevent collection for the lifetime of the application.
    /// </summary>
    internal static class DelegateKeepAlive
    {
        private static GCHandle[] _delegateHandles;
        private static bool _initialized = false;
        private static readonly object _initLock = new object();

        /// <summary>
        /// Initializes the delegate keep-alive mechanism.
        /// This should be called once during application startup, typically from DLLInitializer.Init().
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            lock (_initLock)
            {
                if (_initialized)
                    return;

                try
                {
                    PinSwigDelegates();
                    _initialized = true;
                }
                catch (TypeLoadException)
                {
                    // Type not found - SWIG types may not be loaded yet or don't exist.
                    // This is expected in some configurations.
                }
                catch (SecurityException)
                {
                    // Reflection may be restricted in some security contexts.
                    // Silently fail - the worst case is delegate collection on some platforms.
                }
                catch (Exception)
                {
                    // Other reflection failures - silently ignore.
                    // The worst case is we might get delegate collection on some platforms.
                }
            }
        }

        private static void PinSwigDelegates()
        {
            var handles = new System.Collections.Generic.List<GCHandle>();

            // Get the storj_uplinkPINVOKE class
            var pinvokeType = Type.GetType("uplink.SWIG.storj_uplinkPINVOKE, uplink.NET");
            if (pinvokeType == null)
                return;

            // Pin delegates from SWIGExceptionHelper
            var exceptionHelperType = pinvokeType.GetNestedType("SWIGExceptionHelper", 
                BindingFlags.NonPublic | BindingFlags.Public);
            if (exceptionHelperType != null)
            {
                PinDelegatesFromType(exceptionHelperType, handles);
            }

            // Pin delegates from SWIGStringHelper
            var stringHelperType = pinvokeType.GetNestedType("SWIGStringHelper", 
                BindingFlags.NonPublic | BindingFlags.Public);
            if (stringHelperType != null)
            {
                PinDelegatesFromType(stringHelperType, handles);
            }

            _delegateHandles = handles.ToArray();
        }

        private static void PinDelegatesFromType(Type type, System.Collections.Generic.List<GCHandle> handles)
        {
            // Get all static fields that are delegates
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            
            foreach (var field in fields)
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    try
                    {
                        var delegateInstance = field.GetValue(null);
                        if (delegateInstance != null)
                        {
                            // Allocate a GCHandle to prevent the delegate from being collected.
                            // Using GCHandleType.Normal is sufficient to prevent collection.
                            // GCHandleType.Pinned is not needed since we only need to prevent
                            // collection, not prevent memory movement (delegates are managed objects).
                            var handle = GCHandle.Alloc(delegateInstance, GCHandleType.Normal);
                            handles.Add(handle);
                        }
                    }
                    catch (FieldAccessException)
                    {
                        // Cannot access this field - skip it
                    }
                    catch (TargetException)
                    {
                        // Field access failed - skip it
                    }
                }
            }
        }
    }
}
