using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CpuTemperature
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            this.Opacity = 0;
        }

        int max_temp = 99;
        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + "\\conf.txt"))
            {
                max_temp = Convert.ToInt32(File.ReadAllLines(Application.StartupPath + "\\conf.txt")[0].Trim());
                timer1.Start();
            }
            else
            {
                MessageBox.Show("Нет файла conf.txt", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Hide();
                WindowState = FormWindowState.Minimized;

                if (File.Exists(Application.StartupPath + "\\conf.txt"))
                {
                    max_temp = Convert.ToInt32(File.ReadAllLines(Application.StartupPath + "\\conf.txt")[0].Trim());

                    foreach (ManagementBaseObject tempObject in new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature").Get())
                    {
                        int res = Convert.ToInt32((Convert.ToInt32(tempObject["CurrentTemperature"].ToString()) / 10) - 273.15);
                        using (StreamWriter sw = new StreamWriter("log.txt", true))
                        {
                            sw.WriteLine(res.ToString());
                        }

                        notifyIcon1.Text = "CPU " + res.ToString() + "C";

                        if (res > max_temp)
                        {
                            timer1.Stop();
                            notifyIcon1.ShowBalloonTip(99999, "Сообщение", "Windows переходит в спящий режим!", ToolTipIcon.Info);

                            Thread.Sleep(1000);

                            Application.SetSuspendState(PowerState.Hibernate, false, false);

                            this.Close();
                        }

                        break;
                    }
                }
                else
                {
                    timer1.Stop();
                    MessageBox.Show("Нет файла conf.txt", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                }
            }
            catch
            {
                timer1.Stop();
                MessageBox.Show("Программу нужно запустить от имени администратора!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
        }
    }
}
