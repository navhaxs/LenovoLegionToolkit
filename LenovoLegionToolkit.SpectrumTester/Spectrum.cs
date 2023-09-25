using LenovoLegionToolkit.Lib.Extensions;
using System.Runtime.InteropServices;
using Windows.Win32;
using WindowsDisplayAPI;

namespace LenovoLegionToolkit.SpectrumBacklightTimeout
{
    internal class Spectrum
    {
        public static void SetBrightnessLevel(SafeHandle device, byte brightnessLevel)
        {
            SetFeature(device, new LENOVO_SPECTRUM_GENERIC_REQUEST(LENOVO_SPECTRUM_OPERATION_TYPE.Brightness, brightnessLevel, 0));
        }


        #region Methods

        void Print(byte[] bytes)
        {
            var length = bytes[2] + 4;
            foreach (var i in bytes.Take(length).Split(16))
                Console.WriteLine(string.Join(" ", i.Select(i => $"{i:X2}")));
        }

        bool HasColor(LENOVO_SPECTRUM_COLOR rgbColor) => rgbColor.Red == 255 && rgbColor.Green == 255 && rgbColor.Blue == 255;

        internal unsafe static void SetFeature<T>(SafeHandle handle, T str) where T : notnull
        {
            var ptr = IntPtr.Zero;
            try
            {
                int size;
                if (str is byte[] bytes)
                {
                    size = bytes.Length;
                    ptr = Marshal.AllocHGlobal(size);
                    Marshal.Copy(bytes, 0, ptr, size);
                }
                else
                {
                    size = Marshal.SizeOf<T>();
                    ptr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(str, ptr, false);
                }

                var result = PInvoke.HidD_SetFeature(handle, ptr.ToPointer(), (uint)size);
                if (!result)
                    PInvokeExtensions.ThrowIfWin32Error(typeof(T).Name);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        internal unsafe static void GetFeature<T>(SafeHandle handle, out T str) where T : struct
        {
            var ptr = IntPtr.Zero;
            try
            {
                var size = Marshal.SizeOf<T>();
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(new byte[] { 7 }, 0, ptr, 1);

                var result = PInvoke.HidD_GetFeature(handle, ptr.ToPointer(), (uint)size);
                if (!result)
                    PInvokeExtensions.ThrowIfWin32Error(typeof(T).Name);

                str = Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        internal struct LENOVO_SPECTRUM_HEADER
        {
            public byte Head = 7;
            public LENOVO_SPECTRUM_OPERATION_TYPE Type;
            public byte Size;
            public byte Tail = 3;

            public LENOVO_SPECTRUM_HEADER(LENOVO_SPECTRUM_OPERATION_TYPE type, int size)
            {
                Type = type;
                Size = (byte)(size % 256);
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 960)]
        internal struct LENOVO_SPECTRUM_GENERIC_REQUEST
        {
            public LENOVO_SPECTRUM_HEADER Header;
            public byte Value;
            public byte Value2;

            public LENOVO_SPECTRUM_GENERIC_REQUEST(LENOVO_SPECTRUM_OPERATION_TYPE operation, byte value, byte value2)
            {
                Header = new LENOVO_SPECTRUM_HEADER(operation, 0xC0);
                Value = value;
                Value2 = value2;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 960)]
        internal readonly struct LENOVO_SPECTRUM_GET_BRIGHTNESS_REQUEST
        {
            private readonly LENOVO_SPECTRUM_HEADER Header;

            public LENOVO_SPECTRUM_GET_BRIGHTNESS_REQUEST()
            {
                Header = new LENOVO_SPECTRUM_HEADER(LENOVO_SPECTRUM_OPERATION_TYPE.GetBrightness, 0xC0);
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 960)]
        internal readonly struct LENOVO_SPECTRUM_GET_BRIGHTNESS_RESPONSE
        {
            private readonly byte ReportId;
            private readonly LENOVO_SPECTRUM_OPERATION_TYPE Type;
            private readonly byte Length;
            private readonly byte Unknown1;
            public readonly byte Brightness;
        }

        [StructLayout(LayoutKind.Sequential, Size = 960)]
        internal struct LENOVO_SPECTRUM_GENERIC_RESPONSE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 960)]
            public byte[] Bytes;
        }

        internal enum LENOVO_SPECTRUM_OPERATION_TYPE : byte
        {
            ProfileSet1 = 0xC8,
            GetProfile = 0xCA,
            EffectChange = 0xCB,
            ProfileSet2 = 0xCC,
            GetBrightness = 0xCD,
            Brightness = 0xCE,
            AuroraSendBitmap = 0xA1,
            State = 0x03,
            Unknown04 = 0x04,
            UnknownC4 = 0xC4,
            UnknownC5 = 0xC5,
            UnknownC6 = 0xC6,
            UnknownC7 = 0xC7,
            UnknownD1 = 0xD1,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LENOVO_SPECTRUM_COLOR
        {
            public byte Red;
            public byte Green;
            public byte Blue;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct LENOVO_SPECTRUM_KEY_STATE
        {
            public ushort Key;
            public LENOVO_SPECTRUM_COLOR Color;
        }

        [StructLayout(LayoutKind.Sequential, Size = 960)]
        internal struct LENOVO_SPECTRUM_STATE
        {
            public byte ReportId;
            public LENOVO_SPECTRUM_OPERATION_TYPE Type;
            public byte Unknown2;
            public byte Unknown3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 191)]
            public LENOVO_SPECTRUM_KEY_STATE[] Data;
            public byte Unknown4;
        }
        #endregion
    }
}
