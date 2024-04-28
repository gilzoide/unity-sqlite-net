#if UNITY_EDITOR || !UNITY_WEBGL
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace SQLite
{
    public partial class SQLite3
    {
        [DllImport(LibraryPath, EntryPoint = "sqlite3_config", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Config(int option, IntPtr logFunc, IntPtr userdata);

        public delegate void SQLiteLogCallbackDelegate(IntPtr userdata, Result resultCode, IntPtr messagePtr);

        [MonoPInvokeCallback(typeof(SQLiteLogCallbackDelegate))]
        internal static void SQLiteLogCallback(IntPtr userdata, Result resultCode, IntPtr messagePtr)
        {
            string message = Marshal.PtrToStringUTF8(messagePtr);
            if (resultCode == Result.OK)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogError(message);
            }
        }

        static SQLite3()
        {
            const int SQLITE_CONFIG_LOG = 16;  /* xFunc, void* */
            Config(SQLITE_CONFIG_LOG, Marshal.GetFunctionPointerForDelegate<SQLiteLogCallbackDelegate>(SQLiteLogCallback), IntPtr.Zero);
        }
    }
}
#endif
