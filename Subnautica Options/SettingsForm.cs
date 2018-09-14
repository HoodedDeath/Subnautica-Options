using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Subnautica_Options
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            pathTextBox.Text = new Config().Path;
            steamCheckBox.Checked = new Config().IsSteamLaunch;
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (/*pathTextBox.Focused == false && */e.KeyCode == Keys.Escape) { this.DialogResult = DialogResult.Cancel; Close(); }
                //this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e) //Add Saving of settings
        {
            Config cfg = new Config();
            cfg.IsSteamLaunch = steamCheckBox.Checked;
            cfg.Path = pathTextBox.Text.ToString();
            cfg.Save();
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
