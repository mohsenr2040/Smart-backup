using Microsoft.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using ShamsiDateTime;
using System.Threading;
namespace AutoSmartBackup
{
    public partial class Frm_StartBackup : Form
    {
        public Frm_StartBackup()
        {
            InitializeComponent();
        }
        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        Lts_BackupDataContext Lts_Backup = null;
        private void Frm_StartBackup_Load(object sender, EventArgs e)
        {
            Lts_Backup = new Lts_BackupDataContext();
            StreamReader Sr = new StreamReader(Application.StartupPath.ToString() + "\\ConnectionString.txt");
            Lts_Backup.Connection.ConnectionString = "";
            Lts_Backup.Connection.ConnectionString = Sr.ReadLine();

            Lst_Servers = Lts_Backup.Bkp_Tb_Servers.Where(s => s.xServerIsDeleted_ == false).ToList();
            Pgb_Backup.Maximum = Lst_Servers.Count * 2;
            Pgb_Backup.Minimum = 1;
            Pgb_Backup.Step = 1;
            LoadServer(7);
            Btn_Start.Enabled = false;
        }


        private void Btn_Report_Click(object sender, EventArgs e)
        {
            StreamWriter Sw = new StreamWriter("..\\report\\report.txt");
            for (int i = 0; i <= Lstb_Report.Items.Count - 1; i++)
            {
                Sw.WriteLine(Lstb_Report.Items[i].ToString());
            }
            Sw.Close();
            MessageBox.Show("!گزارش گیری انجام شد");
            //System.Diagnostics.Process Prs_Report = new Process();
            //Prs_Report.StartInfo.WorkingDirectory = "..\\report\\report.txt";
            //Prs_Report.StartInfo.FileName = "report.txt";
            //Prs_Report.Start();
        }

        private void Lstb_Report_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            System.Drawing.Graphics g = e.Graphics;
            g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Red), e.Bounds);
            ListBox lb = (ListBox)sender;
            g.DrawString(lb.Items[e.Index].ToString(), e.Font, new System.Drawing.SolidBrush(System.Drawing.Color.Black), new System.Drawing.PointF(e.Bounds.X, e.Bounds.Y));

            e.DrawFocusRectangle();
        }
        delegate void SetTextCallback(string Str_Text);
        private void ResutltMessage(string Str_Msg)
        {
            if (Lstb_Report.InvokeRequired)
            {
                SetTextCallback Stc_ = new SetTextCallback(ResutltMessage);
                this.Invoke(Stc_, new object[] { Str_Msg });
            }
            else
            {
                Lstb_Report.Items.Add(Str_Msg.Split('|')[0]);
                Lstb_Report.Items.Add(Str_Msg.Split('|')[1]);
                Lstb_Report.Refresh();
                if (Str_Msg.Contains("اتمام عملیات پشتیبان گیری") || Str_Msg.Contains("اتمام عملیات  کپی"))
                {
                    Pgb_Backup.PerformStep();
                }
            }
        }
        List<Bkp_Tb_Server> Lst_Servers = null;
        List<Bkp_Tb_Server> Lst_Servers_sub = null;
        List<Bkp_Tb_Server> Lst_ExecServers = new List<Bkp_Tb_Server>();


        public void LoadServer(int ThreadCount)
        {
            int ThreadCount_ = 0;
            Lst_Servers_sub = new List<Bkp_Tb_Server>();
            foreach (Bkp_Tb_Server item in Lst_Servers)
            {
                if (!Lst_ExecServers.Contains(item) && ThreadCount_ < ThreadCount)
                {
                    Lst_Servers_sub.Add(item);
                    ThreadCount_++;
                }
                if (ThreadCount_ > ThreadCount)
                    break;
            }
            foreach (Bkp_Tb_Server Bkp_Tb_Server1 in Lst_Servers_sub)
            {
                Lst_ExecServers.Add(Bkp_Tb_Server1);
                Thread Thread1 = new Thread(() => Connect(Bkp_Tb_Server1));
                Thread1.Name += Bkp_Tb_Server1.xServerId_pk.ToString();
                Thread1.Start();
                Thread.Sleep(2000);
            }
        }
        List<Bkp_Tb_Server> Lst_ServerError = new List<Bkp_Tb_Server>();


        public void LoadErrorServer(Bkp_Tb_Server Bkp_Tb_Server1)
        {
            Lst_ServerError.Add(Bkp_Tb_Server1);
            ResutltMessage(Bkp_Tb_Server1.xServerIP.ToString() + "|" + "تلاش مجدد ...");
            Thread Thread1 = new Thread(() => Connect(Bkp_Tb_Server1));
            Thread1.Name += Bkp_Tb_Server1.xServerId_pk.ToString();
            Thread1.Start();
            Thread.Sleep(2000);
        }
        private void Btn_Start_Click(object sender, EventArgs e)
        {
            Lst_Servers = Lts_Backup.Bkp_Tb_Servers.Where(s => s.xServerIsDeleted_ == false).ToList();
            Pgb_Backup.Maximum = Lst_Servers.Count * 2;
            Pgb_Backup.Minimum = 1;
            Pgb_Backup.Step = 1;
            LoadServer(10);
            //foreach (Bkp_Tb_Server Bkp_Tb_Server1 in Lst_Servers)
            //{
            //    Thread Thread1 = new Thread(() => Connect(Bkp_Tb_Server1));
            //    Thread1.Name += Bkp_Tb_Server1.xServerId_pk.ToString();
            //    Thread1.Start();
            //    Thread.Sleep(2000);
            //}
            //}
            Btn_Start.Enabled = true;
        }

        public void Connect(Bkp_Tb_Server item)
        {
            IntPtr admin_token = default(IntPtr);
            WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
            WindowsIdentity wid_admin = null;
            WindowsImpersonationContext wic = null;
            ResutltMessage(item.xServerIP.ToString() + "|" + "در حال برقراری ارتباط...");
            string Str_Directory = "";
            string Str_FolderName = "";
            try
            {

                if (LogonUser(item.xServerAdminName, item.xServerDomainName, item.xServerAdminPassword, 9, 0, ref admin_token) != 0)
                {
                    wid_admin = new WindowsIdentity(admin_token);
                    wic = wid_admin.Impersonate();
                    string Str_Domain = item.xServerDomainName.Split('.')[0];
                    string Str_Drive = item.xServerDriveToBackup.Trim();
                    Str_FolderName = Str_Domain + "_" + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(8, 2);
                    Str_Directory = "\\\\" + item.xServerIP + "\\" + Str_Drive + "$\\Backup\\";

                    //Create Directory For Backup
                    try
                    {
                        //if (System.IO.Directory.Exists(Str_Directory + Str_FolderName))
                        //{
                        //    string[] Str_Files = System.IO.Directory.GetFiles(Str_Directory + Str_FolderName);
                        //    foreach (string file in Str_Files)
                        //        System.IO.File.Delete(file);
                        //}
                        //else
                        //{
                        //    System.IO.Directory.CreateDirectory(Str_Directory + Str_FolderName);
                        //}

                        //Create Or Open Txt File  
                        //StreamWriter StreamWriter1 = new StreamWriter(Str_Directory + "AutoBackup_AddressFileToZip.txt");
                        //StreamWriter1.Write(Str_FolderName);
                        //StreamWriter1.Close();

                        //Copy Consol Zip And DLL
                        if (item.xServerDriveToBackup.Trim() == "C")
                        {
                            if (!File.Exists(Str_Directory + "Console_ZipOnC.exe"))
                                System.IO.File.Copy("E:\\Sources\\Console_ZipOnC.exe", Str_Directory + "Console_ZipOnC.exe", true);
                        }
                        else if (item.xServerDriveToBackup.Trim() == "D")
                        {
                            if (!File.Exists(Str_Directory + "Console_ZipOnD.exe"))
                                System.IO.File.Copy("E:\\Sources\\Console_ZipOnD.exe", Str_Directory + "Console_ZipOnD.exe", true);
                        }
                        else if (item.xServerDriveToBackup.Trim() == "E")
                        {
                            if (!File.Exists(Str_Directory + "Console_ZipOnE.exe"))
                                System.IO.File.Copy("E:\\Sources\\Console_ZipOnE.exe", Str_Directory + "Console_ZipOnE.exe", true);
                        }
                        else if (item.xServerDriveToBackup.Trim() == "F")
                        {
                            if (!File.Exists(Str_Directory + "Console_ZipOnF.exe"))
                                System.IO.File.Copy("E:\\Sources\\Console_ZipOnF.exe", Str_Directory + "Console_ZipOnF.exe", true);
                        }
                        if (!File.Exists(Str_Directory + "ICSharpCode.SharpZipLib.dll"))
                            System.IO.File.Copy("E:\\Sources\\ICSharpCode.SharpZipLib.dll", Str_Directory + "ICSharpCode.SharpZipLib.dll", true);
                    }
                    catch (Exception ex)
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا درانتقال فایل : " + ex.ToString());
                    }



                    ////Create Connection And open to backup 
                    string Str_ConnectionString =
                       "Data Source=" + item.xServerIP + (item.xServerSQLInstance != null ? ("\\" + item.xServerSQLInstance) : "") +
                       ";User ID=" + item.xServerSQLAdminName +
                        ";password=" + item.xServerSQLAdminPassword +
                        ";Integrated Security=True;Connect Timeout=120";
                    SqlConnection Sql_conn = new SqlConnection(Str_ConnectionString);
                    SqlCommand Sql_Comm = new SqlCommand();
                    Sql_Comm.Connection = Sql_conn;
                    try
                    {
                        Sql_conn.Open();
                        ResutltMessage(item.xServerIP.ToString() + "|" + "ارتباط موفق با بانک اطلاعاتی");
                    }
                    catch
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "ارتباط ناموفق با بانک اطلاعاتی");
                        if (!Lst_ServerError.Contains(item))
                        {
                            LoadErrorServer(item);
                        }
                        else
                        {
                            LoadServer(1);
                            return;
                        }

                    }

                    //create directory
                    string Str_CreateDirectory =" EXEC master..xp_CMDShell 'mkdir " + item.xServerDriveToBackup.Trim() +
                                   ":\\Backup\\" + Str_FolderName + "' ";

                    Sql_Comm.CommandText = Str_CreateDirectory;
                    Sql_Comm.CommandTimeout = 0;
                    try
                    {
                        Sql_Comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Str_CreateDirectory = "EXEC sp_configure 'show advanced options', 1 " +
                                  " RECONFIGURE EXEC sp_configure 'xp_cmdshell', 1 " +
                                  " RECONFIGURE  " +
                                  " EXEC master..xp_CMDShell 'mkdir " + item.xServerDriveToBackup.Trim() +
                                   ":\\Backup\\" + Str_FolderName + "' ";

                        Sql_Comm.CommandText = Str_CreateDirectory;
                        Sql_Comm.CommandTimeout = 0;
                        Sql_Comm.ExecuteNonQuery();
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                    }

                    //delete all files in directory
                    string Str_DeleteDirectoryFiles = " EXEC master..xp_CMDShell 'del " + item.xServerDriveToBackup.Trim() +
                                  ":\\Backup\\" + Str_FolderName + "\\*.bak' ";

                    Sql_Comm.CommandText = Str_DeleteDirectoryFiles;
                    Sql_Comm.CommandTimeout = 0;
                    try
                    {
                        Sql_Comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                    }

                    Lts_BackupDataContext Lts_Backup = new Lts_BackupDataContext();
                    List<Bkp_Tb_DataBase> Lst_DataBases = Lts_Backup.Bkp_Tb_ServerDataBases.Where(b =>
                                                      b.xServerId_fk == item.xServerId_pk).Select(b => b.Bkp_Tb_DataBase).ToList();
                    string Str_Databases = "";
                    foreach (Bkp_Tb_DataBase DatabaseName in Lst_DataBases)
                    {
                        Str_Databases += "'" + DatabaseName.xDataBaseName + "',";
                    }
                    Str_Databases = Str_Databases.Substring(0, Str_Databases.Length - 1);

                    string Str_BackupQuery = "DECLARE @name VARCHAR(50) " +
                                    "DECLARE @path VARCHAR(256) " +
                                    "DECLARE @fileName VARCHAR(256) " +
                                    "DECLARE @fileDate VARCHAR(20) " +
                                    "SET @path ='" + item.xServerDriveToBackup.Trim() + ":\\" + "Backup\\" + Str_FolderName + "\\'" +
                                     " SELECT @fileDate = CONVERT(VARCHAR(20),GETDATE(),112) " +
                                    "DECLARE db_cursor CURSOR FOR " +
                                    "SELECT name " +
                                    "FROM master.dbo.sysdatabases " +
                                    "WHERE name  IN (" + Str_Databases + ") " +
                                    "OPEN db_cursor " +
                                    "FETCH NEXT FROM db_cursor INTO @name " +
                                    "WHILE @@FETCH_STATUS = 0 " +
                                    "BEGIN " +
                                    "SET @fileName = @path + @name   + '.bak' " +
                                    "BACKUP DATABASE @name TO DISK = @fileName " +
                                    "FETCH NEXT FROM db_cursor INTO @name " +
                                    "END " +
                                    "CLOSE db_cursor " +
                                    "DEALLOCATE db_cursor";

                    Sql_Comm.CommandText = Str_BackupQuery;
                    Sql_Comm.CommandTimeout = 0;

                    ResutltMessage(item.xServerIP.ToString() + "|" + "شروع عملیات پشتیبان گیری در : " + DateTime.Now.ToShortTimeString());

                    try
                    {
                        Sql_Comm.ExecuteNonQuery();
                        ResutltMessage(item.xServerIP.ToString() + "|" + "اتمام عملیات پشتیبان گیری در : " + DateTime.Now.ToShortTimeString());

                    }
                    catch (Exception ex)
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                        if (!Lst_ServerError.Contains(item))
                        {
                            LoadErrorServer(item);
                        }
                        else
                        {
                            LoadServer(1);
                            return;
                        }

                    }


                    //StreamReader StreamReader1 = new StreamReader(Str_Directory + "AutoBackup_AddressFileToZip.txt");
                    //string Str_Msg = StreamReader1.ReadLine();
                    //StreamReader1.Close();
                    //StreamWriter StreamWriter2 = new StreamWriter(Str_Directory + "AutoBackup_AddressFileToZip.txt");
                    //StreamWriter2.Write(Str_Msg + "&" + "Backup Success");
                    //StreamWriter2.Close();  
                    //
                    string Str_WriteFileName = " EXEC master..xp_CMDShell 'echo " + Str_FolderName + "^&" + "Backup Success > " + item.xServerDriveToBackup.Trim() +
                                 ":\\Backup\\AutoBackup_AddressFileToZip.txt' ";
                    Sql_Comm.CommandText = Str_WriteFileName;
                    Sql_Comm.CommandTimeout = 0;
                    try
                    {
                        Sql_Comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());

                    }


                    //
                    ResutltMessage(item.xServerIP.ToString() + "|" + "شروع عملیات فشرده سازی در : " + DateTime.Now.ToShortTimeString());

                    Str_BackupQuery = " EXEC master..xp_CMDShell '" + item.xServerDriveToBackup.Trim() +
                                   ":\\Backup\\Console_ZipOn" + item.xServerDriveToBackup.Trim() + ".exe' ";
                    Sql_Comm.CommandText = Str_BackupQuery;
                    Sql_Comm.CommandTimeout = 0;
                    try
                    {
                        Sql_Comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                    }


                    ResutltMessage(item.xServerIP.ToString() + "|" + "اتمام عملیات فشرده سازی در : " + DateTime.Now.ToShortTimeString());

                    //del files
                    string Str_DeleteDirectoryFiles1 = " EXEC master..xp_CMDShell 'del " + item.xServerDriveToBackup.Trim() +
                                 ":\\Backup\\" + Str_FolderName + "\\*.bak' ";

                    Sql_Comm.CommandText = Str_DeleteDirectoryFiles1;
                    Sql_Comm.CommandTimeout = 0;
                    try
                    {
                        Sql_Comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                        return;
                    }

                    //del dir
                    string Str_DeleteDirectory = " EXEC master..xp_CMDShell 'RMDIR " + item.xServerDriveToBackup.Trim() +
                                   ":\\Backup\\" + Str_FolderName + "' ";

                    Sql_Comm.CommandText = Str_DeleteDirectory;
                    Sql_Comm.CommandTimeout = 0;
                    try
                    {
                        Sql_Comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                    }

                    Sql_conn.Close();


                    //
                    ResutltMessage(item.xServerIP.ToString() + "|" + "شروع عملیات کپی در : " + DateTime.Now.ToShortTimeString());
                    if (!Directory.Exists("G:\\Bkp_" + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(5, 2) + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(8, 2)))
                    {
                        Directory.CreateDirectory("G:\\Bkp_" + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(5, 2) + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(8, 2));
                    }
                    try
                    {
                        File.Copy(Str_Directory + Str_FolderName + ".zip", "G:\\Bkp_" + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(5, 2) + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(8, 2) + "\\" + Str_FolderName + ".zip", true);
                        ResutltMessage(item.xServerIP.ToString() + "|" + "اتمام عملیات  کپی در : " + DateTime.Now.ToShortTimeString());
                        try
                        {
                            File.Delete(Str_Directory + Str_FolderName + ".zip");
                        }
                        catch { }
                    }
                    catch
                    {
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا در عملیات  کپی : ");
                        ResutltMessage(item.xServerIP.ToString() + "|" + "!تلاش مجدد برای عملیات  کپی : ");
                        try
                        {
                            File.Copy(Str_Directory + Str_FolderName + ".zip", "G:\\Bkp_" + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(5, 2) + Class_ShamsiDateTime.MilladiToShamsi(DateTime.Now).ToString().Substring(8, 2) + "\\" + Str_FolderName + ".zip", true);
                            ResutltMessage(item.xServerIP.ToString() + "|" + "اتمام عملیات  کپی در : " + DateTime.Now.ToShortTimeString());
                            try
                            {
                                File.Delete(Str_Directory + Str_FolderName + ".zip");
                            }
                            catch { }
                        }
                        catch
                        {
                            ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا مجدد در عملیات  کپی : ");
                        }
                    }

                    LoadServer(1);
                }
                //}
                //catch (Exception ex)
                //{
                //    ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                //    //LoadServer(1);
                //    //if (Directory.Exists(Str_Directory + Str_FolderName))
                //    //    try
                //    //    {
                //    //        Directory.Delete(Str_Directory + Str_FolderName, true);
                //    //    }
                //    //    catch { }
                //    //if (File.Exists(Str_Directory + Str_FolderName + ".zip"))
                //    //    try
                //    //    {
                //    //        File.Delete(Str_Directory + Str_FolderName + ".zip");
                //    //    }
                //    //    catch { }
                //    if (!Lst_ServerError.Contains(item))
                //    {
                //        LoadErrorServer(item);
                //    }
                //}
            }
            catch (Exception ex)
            {
                ResutltMessage(item.xServerIP.ToString() + "|" + "!خطا : " + ex.ToString());
                LoadServer(1);
            }  
        }
    }
}