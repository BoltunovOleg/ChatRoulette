using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace ChatRoulette.Utils
{
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32DllName = "kernel32.dll";

        [DllImport(Kernel32DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32DllName)]
        private static extern IntPtr GetConsoleWindow();

        public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;

        public static void Show()
        {
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
        }

        public static void Hide()
        {
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        static void InvalidateOutAndError()
        {
            var type = typeof(Console);

            var _out = type.GetField("_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            var error = type.GetField("_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            var initializeStdOutError = type.GetMethod("InitializeStdOutError",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Debug.Assert(_out != null);
            Debug.Assert(error != null);

            Debug.Assert(initializeStdOutError != null);

            _out.SetValue(null, null);
            error.SetValue(null, null);

            initializeStdOutError.Invoke(null, new object[] {true});
        }

        private static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}