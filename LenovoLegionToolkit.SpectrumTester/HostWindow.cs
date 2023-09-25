using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LenovoLegionToolkit.SpectrumBacklightTimeout
{
    public partial class HostWindow : Form
    {
        KeyboardInput keyboard = new KeyboardInput();
        //ReadyToClamshellWindow wnd;

        public HostWindow()
        {
            InitializeComponent();


            BacklightStepUtils backlightStepUtils = new BacklightStepUtils();
            backlightStepUtils.Run();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            SetVisibility(!Visible);
        }

        void SetVisibility(bool visibility)
        {
            Visible = visibility;
            ShowInTaskbar = visibility;
            Opacity = visibility ? 100 : 0;
            ShowIcon = visibility;
            ShowInTaskbar = visibility;
        }

        private void NotifyIconHost_Load(object sender, EventArgs e)
        {
            SetVisibility(false);
        }

        private void HostWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetVisibility(false);
            e.Cancel = true;
        }
    }
}
