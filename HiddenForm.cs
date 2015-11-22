﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;



namespace HddActivity
{
  
    public partial class HiddenForm : Form
    {
        #region Variables
        private NotifyIcon hddTrayIcon;

        private Icon busyIcon;
        private Icon idleIcon;

        Thread hddActivityThread;
        #endregion

        #region Initialization Methods
        private void setUpIcons()
        {

            //Instantiating icon objects, and joining adequate .ico images.
            busyIcon = new Icon("images\\HDD_Busy.ico");
            idleIcon = new Icon("images\\HDD_Idle.ico");
            hddTrayIcon = new NotifyIcon();


            //Assinging default idleIcon to the tray icon and making it visible.
            hddTrayIcon.Icon = idleIcon;
            hddTrayIcon.Visible = true;
        }

        private void hideForm()
        {
            //Hiding the form-app work visible only in notification tray.

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        /*
        Creates a contextMenu, adds menu items,
        assigns contextMenu to trayIcon.
        */
        private void setUpTrayIconMenu()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem exitItem = new MenuItem("Exit");

            //Assigns an on click event handler to the menu item "Exit
            exitItem.Click += exitItem_Click;     

            contextMenu.MenuItems.Add(exitItem);
            hddTrayIcon.ContextMenu = contextMenu;
        }

        //Assigns thread activity and starts the thread.
        private void setUpThread()
        {
            hddActivityThread = new Thread(new ThreadStart(hddThreadActivity));
            hddActivityThread.Start();
        }
        #endregion

        #region Context Menu Event handlers
        /*
         * Disposes the resources and closes the applicaton
         * when "Exit" button ,in the context menu, is clicked.
        */
        
        private void exitItem_Click(object sender, EventArgs e)
        {
            hddActivityThread.Abort();
            hddTrayIcon.Dispose();
            this.Close();
        }
        #endregion

        #region Activity Threads
        private void hddThreadActivity()
        {
            ManagementClass physicalDriveData = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            try
            {
                //Using WMI to get physical disk activity per second.
                while (true)
                {
                    //Queries WMI at each itteration and gets the new values.
                    //Gets the instances of all physical drives in the system(separately) and a Total instance (all drives on the system combined).
                    ManagementObjectCollection physicalDriveDataCollection = physicalDriveData.GetInstances();

                    //finds total instance
                    foreach (ManagementObject obj in physicalDriveDataCollection)
                    {

                        //Process only the _Total instance, and ignore all the others.
                        if (obj["Name"].ToString() == "_Total")
                        {

                            //Getting the DiskBytesPersec property
                            //unsigned 64bit intenger,return value needs to be converted.

                           if (Convert.ToUInt64(obj["DIskBytesPersec"]) > 0)
                            {
                               //Show busy icon.
                                hddTrayIcon.Icon = busyIcon;
                            }
                            else
                            {
                               //Show idle icon.
                                hddTrayIcon.Icon = idleIcon;
                            }
                        }
                    }
                    //Sleep for 10th of a second
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException tae)  
            {
                //Thread aborted
                physicalDriveData.Dispose();
            }
        }
        #endregion

        #region Main Form
        public HiddenForm()
        {
            setUpIcons();
            setUpTrayIconMenu();
            setUpThread();
            InitializeComponent();
            hideForm();
        }
        #endregion

    }
}
