using System;
using System.Diagnostics;
using System.Windows.Forms;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using SharpDX.DirectInput;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.Threading;

namespace G27_to_XBOX_Converter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ViGEmClient viGEmClient = new ViGEmClient();
            IXbox360Controller xbox360Controller = viGEmClient.CreateXbox360Controller();
            xbox360Controller.Connect();
            DirectInput directInput = new DirectInput();
            Joystick g27 = null;
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Driving, DeviceEnumerationFlags.AllDevices))
            {
                if (deviceInstance.InstanceName == "Logitech G27 Racing Wheel USB")
                {
                    g27 = new Joystick(new DirectInput(), deviceInstance.InstanceGuid);
                }
            }

            if (g27 == null)
            {
                MessageBox.Show("탐지된 G27이 없습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }


            g27.Properties.BufferSize = 128;
            g27.Acquire();

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    g27.Poll();
                    JoystickUpdate[] datas = g27.GetBufferedData();
                    foreach (JoystickUpdate state in datas)
                    {
                        Console.WriteLine(state);

                        switch (state.Offset)
                        {
                            // 핸들을 좌우로 움직일 때:
                            case JoystickOffset.X:
                                xbox360Controller.SetAxisValue(Xbox360Axis.LeftThumbX, Safety.SafeShort(8 * (state.Value - 32768)));
                                break;

                            // 액셀을 누를 때:
                            case JoystickOffset.Y:
                                xbox360Controller.SetButtonState(g27.GetCurrentState().Buttons[10]?Xbox360Button.B:Xbox360Button.A, state.Value < 65535 - 20);
                                break;

                            case JoystickOffset.Buttons15:
                                xbox360Controller.SetButtonState(Xbox360Button.X, state.Value > 0);
                                break;
                            case JoystickOffset.Buttons16:
                                xbox360Controller.SetButtonState(Xbox360Button.Y, state.Value > 0);
                                break;
                            case JoystickOffset.Buttons17:
                                xbox360Controller.SetButtonState(Xbox360Button.B, state.Value > 0);
                                break;
                            case JoystickOffset.Buttons18:
                                xbox360Controller.SetButtonState(Xbox360Button.A, state.Value > 0);
                                break;


                            case JoystickOffset.Buttons5:
                                xbox360Controller.SetButtonState(Xbox360Button.LeftShoulder, state.Value > 0);
                                break;
                            case JoystickOffset.Buttons4:
                                xbox360Controller.SetButtonState(Xbox360Button.RightShoulder, state.Value > 0);
                                break;

                            case JoystickOffset.Buttons7:
                                xbox360Controller.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(state.Value > 0 ? 127 : 0));
                                break;
                            case JoystickOffset.Buttons6:
                                xbox360Controller.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(state.Value > 0 ? 127 : 0));
                                break;

                            case JoystickOffset.Buttons22:
                                xbox360Controller.SetButtonState(Xbox360Button.Guide, state.Value > 0);
                                break;
                            case JoystickOffset.Buttons21:
                                xbox360Controller.SetButtonState(Xbox360Button.Start, state.Value > 0);
                                break;

                            case JoystickOffset.PointOfViewControllers0:
                                switch(state.Value)
                                {
                                    case -1:
                                        xbox360Controller.SetButtonState(Xbox360Button.Left, false);
                                        xbox360Controller.SetButtonState(Xbox360Button.Right, false);
                                        xbox360Controller.SetButtonState(Xbox360Button.Up, false);
                                        xbox360Controller.SetButtonState(Xbox360Button.Down, false);
                                        break;
                                    case 0:
                                        xbox360Controller.SetButtonState(Xbox360Button.Up, true);
                                        break;
                                    case 18000:
                                        xbox360Controller.SetButtonState(Xbox360Button.Down, true);
                                        break;
                                    case 27000:
                                        xbox360Controller.SetButtonState(Xbox360Button.Left, true);
                                        break;
                                    case 9000:
                                        xbox360Controller.SetButtonState(Xbox360Button.Right, true);
                                        break;
                                }
                                break;
                        }
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();

        }
    }
}
