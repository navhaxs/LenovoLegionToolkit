using LenovoLegionToolkit.Lib.Extensions;
using LenovoLegionToolkit.Lib.System;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32.SafeHandles;
using SpectrumBacklightTimeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static LenovoLegionToolkit.SpectrumBacklightTimeout.Spectrum;

namespace LenovoLegionToolkit.SpectrumBacklightTimeout
{
    internal class BacklightStepUtils
    {
        SafeFileHandle? device;
        KeyboardInput keyboard;
        MouseInput mouse;
        DateTime lastActivity;

        // config
        int inactivitySecondsThreshold = 7;
        int brightnessLevel = 10;

        byte[] generateArray(int max)
        {
            var result = new byte[max + 1];
            foreach (int index in Enumerable.Range(0, max + 1))
            {
                result[index] = (byte)index;
            }
            return result;
        }

        public TimeSpan InactivityPeriod
        {
            get
            {
                return DateTime.Now.Subtract(lastActivity);
            }
        }

        async public void Run()
        {
            device = Devices.GetSpectrumRGBKeyboard();

            if (device is null)
            {
                Console.WriteLine("Spectrum not supported");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Running...");


            //var input = new LENOVO_SPECTRUM_GET_BRIGHTNESS_REQUEST();
            //Spectrum.GetFeature(device, out LENOVO_SPECTRUM_GET_BRIGHTNESS_RESPONSE output);
            //brightnessLevel = output.Brightness;

            var acendingBrightnessLevels = generateArray(brightnessLevel);
            var descendingBrightnessLevels = generateArray(brightnessLevel).Reverse();

            //UserActivityMonitor uam = new UserActivityMonitor();
            keyboard = new KeyboardInput();
            mouse = new MouseInput();

            keyboard.KeyBoardKeyPressed += (s, e) =>
            {
                lastActivity = DateTime.Now;
            };
            mouse.MouseMoved += (s, e) =>
            {
                lastActivity = DateTime.Now;
            };

            byte lastActiveBrightnessLevel = 4;
            bool isInactive = false;

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (!isInactive && InactivityPeriod.Seconds > inactivitySecondsThreshold)
                    {
                        isInactive = true;

                        foreach (var level in generateArray(lastActiveBrightnessLevel).Reverse())
                        {
                            Spectrum.SetBrightnessLevel(device, level);
                            await Task.Delay(60);
                        }

                    }
                    else if (isInactive && InactivityPeriod.Seconds < 1)
                    {
                        isInactive = false;
                        foreach (var level in generateArray(lastActiveBrightnessLevel))
                        {
                            Spectrum.SetBrightnessLevel(device, level);
                            await Task.Delay(50);
                        }
                    }

                    await Task.Delay(100);
                }
            });


        }
    }
}
