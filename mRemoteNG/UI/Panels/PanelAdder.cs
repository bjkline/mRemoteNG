﻿using mRemoteNG.App;
using mRemoteNG.Messages;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.Window;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using mRemoteNG.Resources.Language;
using System.Runtime.Versioning;

namespace mRemoteNG.UI.Panels
{
    [SupportedOSPlatform("windows")]
    public class PanelAdder
    {
        public ConnectionWindow AddPanel(string title = "")
        {
            try
            {
                ConnectionWindow connectionForm = new(new DockContent());
                BuildConnectionWindowContextMenu(connectionForm);
                SetConnectionWindowTitle(title, connectionForm);
                ShowConnectionWindow(connectionForm);
                PrepareTabSupport(connectionForm);
                return connectionForm;
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Couldn\'t add panel" + Environment.NewLine + ex.Message);
                return null;
            }
        }

        public bool DoesPanelExist(string panelName)
        {
            return Runtime.WindowList?.OfType<ConnectionWindow>().Any(w => w.TabText == panelName)
                ?? false;
        }

        private static void ShowConnectionWindow(ConnectionWindow connectionForm)
        {
            connectionForm.Show(FrmMain.Default.pnlDock, DockState.Document);
        }

        private static void PrepareTabSupport(ConnectionWindow connectionForm)
        {
            Runtime.WindowList.Add(connectionForm);
        }

        private static void SetConnectionWindowTitle(string title, ConnectionWindow connectionForm)
        {
            if (string.IsNullOrEmpty(title))
                title = Language.NewPanel;
            connectionForm.SetFormText(title.Replace("&", "&&"));
        }

        private static void BuildConnectionWindowContextMenu(DockContent pnlcForm)
        {
            ContextMenuStrip cMen = new();
            ToolStripMenuItem cMenRen = CreateRenameMenuItem(pnlcForm);
            ToolStripMenuItem cMenScreens = CreateScreensMenuItem(pnlcForm);
            ToolStripMenuItem cMenClose = CreateCloseMenuItem(pnlcForm);
            cMen.Items.AddRange(new ToolStripItem[] {cMenRen, cMenScreens, cMenClose});
            pnlcForm.TabPageContextMenuStrip = cMen;
        }

        private static ToolStripMenuItem CreateScreensMenuItem(DockContent pnlcForm)
        {
            ToolStripMenuItem cMenScreens = new()
            {
                Text = Language.SendTo,
                Image = Properties.Resources.Monitor_16x,
                Tag = pnlcForm
            };
            cMenScreens.DropDownItems.Add("Dummy");
            cMenScreens.DropDownOpening += cMenConnectionPanelScreens_DropDownOpening;
            return cMenScreens;
        }

        private static ToolStripMenuItem CreateRenameMenuItem(DockContent pnlcForm)
        {
            ToolStripMenuItem cMenRen = new()
            {
                Text = Language.Rename,
                Image = Properties.Resources.Rename_16x,
                Tag = pnlcForm
            };
            cMenRen.Click += cMenConnectionPanelRename_Click;
            return cMenRen;
        }

        private static ToolStripMenuItem CreateCloseMenuItem(DockContent pnlcForm)
        {
            ToolStripMenuItem cMenClose = new()
            {
                Text = Language._Close,
                Image = Properties.Resources.Close_16x,
                Tag = pnlcForm
            };
            cMenClose.Click += cMenConnectionPanelClose_Click;
            return cMenClose;
        }

        private static void cMenConnectionPanelRename_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectionWindow conW = (ConnectionWindow)((ToolStripMenuItem)sender).Tag;

                using (FrmInputBox newTitle = new(Language.NewTitle, Language.NewTitle + ":", ""))
                    if (newTitle.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(newTitle.returnValue))
                        conW.SetFormText(newTitle.returnValue.Replace("&", "&&"));
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("cMenConnectionPanelRename_Click: Caught Exception: ", ex);
            }
        }

        private static void cMenConnectionPanelClose_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectionWindow conW = (ConnectionWindow)((ToolStripMenuItem)sender).Tag;
                conW.Close();
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("cMenConnectionPanelClose_Click: Caught Exception: ", ex);
            }
        }

        private static void cMenConnectionPanelScreens_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem cMenScreens = (ToolStripMenuItem)sender;
                cMenScreens.DropDownItems.Clear();

                for (int i = 0; i <= Screen.AllScreens.Length - 1; i++)
                {
                    ToolStripMenuItem cMenScreen = new(Language.Screen + " " + Convert.ToString(i + 1))
                    {
                        Tag = new ArrayList(),
                        Image = Properties.Resources.Monitor_16x
                    };
                    ((ArrayList)cMenScreen.Tag).Add(Screen.AllScreens[i]);
                    ((ArrayList)cMenScreen.Tag).Add(cMenScreens.Tag);
                    cMenScreen.Click += cMenConnectionPanelScreen_Click;
                    cMenScreens.DropDownItems.Add(cMenScreen);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("cMenConnectionPanelScreens_DropDownOpening: Caught Exception: ", ex);
            }
        }

        private static void cMenConnectionPanelScreen_Click(object sender, EventArgs e)
        {
            Screen screen = null;
            DockContent panel = null;
            try
            {
                IEnumerable tagEnumeration = (IEnumerable)((ToolStripMenuItem)sender).Tag;
                if (tagEnumeration == null) return;
                foreach (object obj in tagEnumeration)
                {
                    if (obj is Screen screen1)
                    {
                        screen = screen1;
                    }
                    else if (obj is DockContent)
                    {
                        panel = (DockContent)obj;
                    }
                }

                Screens.SendPanelToScreen(panel, screen);
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("cMenConnectionPanelScreen_Click: Caught Exception: ", ex);
            }
        }
    }
}