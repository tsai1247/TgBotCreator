using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Microsoft.Win32;

namespace TgBotCreator
{
    internal class CMD
    {
        public static void Run(string command)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c " + command;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }
    }
    internal class Config
    {
        private static string filepath = "Data.db";
        private static string tablename = "Config";
        public static void Set(string key, object value)
        {
            Sqlite sql = new Sqlite(filepath, tablename);
            sql.AddWithPair("key", key);
            sql.AddWithPair("value", value);
            sql.condition = "key = $1";
            sql.parameters.Add("$1", key);
            sql.UpdateOrInsert();
            sql.Close();
        }
        public static object Get(string key)
        {
            Sqlite sql = new Sqlite(filepath, tablename);
            sql.fieldnames.Add("value");
            sql.condition = "key = $1";
            sql.parameters.Add("$1", key);
            var result = sql.Read();
            sql.Close();
            if (result.Length > 0)
                return result[0][0];
            else
                return null;
        }
        public static int? GetInt(string key)
        {
            Sqlite sql = new Sqlite(filepath, tablename);
            sql.fieldnames.Add("value");
            sql.condition = "key = $1";
            sql.parameters.Add("$1", key);
            var result = sql.Read();
            sql.Close();
            if (result.Length > 0)
                return int.Parse(result[0][0].ToString());
            else
                return null;
        }

    }
    class FolderPicker
    {
        public virtual string ResultPath { get; protected set; }
        public virtual string ResultName { get; protected set; }
        public virtual string InputPath { get; set; }
        public virtual bool ForceFileSystem { get; set; }
        public virtual string Title { get; set; }
        public virtual string OkButtonLabel { get; set; }
        public virtual string FileNameLabel { get; set; }

        protected virtual int SetOptions(int options)
        {
            if (ForceFileSystem)
            {
                options |= (int)FOS.FOS_FORCEFILESYSTEM;
            }
            return options;
        }

        // for WPF support
        public bool? ShowDialog(Window owner = null, bool throwOnError = false)
        {
            owner ??= Application.Current.MainWindow;
            return ShowDialog(owner != null ? new WindowInteropHelper(owner).Handle : IntPtr.Zero, throwOnError);
        }

        // for all .NET
        public virtual bool? ShowDialog(IntPtr owner, bool throwOnError = false)
        {
            var dialog = (IFileOpenDialog)new FileOpenDialog();
            if (!string.IsNullOrEmpty(InputPath))
            {
                if (CheckHr(SHCreateItemFromParsingName(InputPath, null, typeof(IShellItem).GUID, out var item), throwOnError) != 0)
                    return null;

                dialog.SetFolder(item);
            }

            var options = FOS.FOS_PICKFOLDERS;
            options = (FOS)SetOptions((int)options);
            dialog.SetOptions(options);

            if (Title != null)
            {
                dialog.SetTitle(Title);
            }

            if (OkButtonLabel != null)
            {
                dialog.SetOkButtonLabel(OkButtonLabel);
            }

            if (FileNameLabel != null)
            {
                dialog.SetFileName(FileNameLabel);
            }

            if (owner == IntPtr.Zero)
            {
                owner = Process.GetCurrentProcess().MainWindowHandle;
                if (owner == IntPtr.Zero)
                {
                    owner = GetDesktopWindow();
                }
            }

            var hr = dialog.Show(owner);
            if (hr == ERROR_CANCELLED)
                return null;

            if (CheckHr(hr, throwOnError) != 0)
                return null;

            if (CheckHr(dialog.GetResult(out var result), throwOnError) != 0)
                return null;

            if (CheckHr(result.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out var path), throwOnError) != 0)
                return null;

            ResultPath = path;

            if (CheckHr(result.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out path), false) == 0)
            {
                ResultName = path;
            }
            return true;
        }

        private static int CheckHr(int hr, bool throwOnError)
        {
            if (hr != 0)
            {
                if (throwOnError)
                    Marshal.ThrowExceptionForHR(hr);
            }
            return hr;
        }

        [DllImport("shell32")]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

        [DllImport("user32")]
        private static extern IntPtr GetDesktopWindow();

#pragma warning disable IDE1006 // Naming Styles
        private const int ERROR_CANCELLED = unchecked((int)0x800704C7);
#pragma warning restore IDE1006 // Naming Styles

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")] // CLSID_FileOpenDialog
        private class FileOpenDialog
        {
        }

        [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig] int Show(IntPtr parent); // IModalWindow
            [PreserveSig] int SetFileTypes();  // not fully defined
            [PreserveSig] int SetFileTypeIndex(int iFileType);
            [PreserveSig] int GetFileTypeIndex(out int piFileType);
            [PreserveSig] int Advise(); // not fully defined
            [PreserveSig] int Unadvise();
            [PreserveSig] int SetOptions(FOS fos);
            [PreserveSig] int GetOptions(out FOS pfos);
            [PreserveSig] int SetDefaultFolder(IShellItem psi);
            [PreserveSig] int SetFolder(IShellItem psi);
            [PreserveSig] int GetFolder(out IShellItem ppsi);
            [PreserveSig] int GetCurrentSelection(out IShellItem ppsi);
            [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            [PreserveSig] int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            [PreserveSig] int GetResult(out IShellItem ppsi);
            [PreserveSig] int AddPlace(IShellItem psi, int alignment);
            [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            [PreserveSig] int Close(int hr);
            [PreserveSig] int SetClientGuid();  // not fully defined
            [PreserveSig] int ClearClientData();
            [PreserveSig] int SetFilter([MarshalAs(UnmanagedType.IUnknown)] object pFilter);
            [PreserveSig] int GetResults([MarshalAs(UnmanagedType.IUnknown)] out object ppenum);
            [PreserveSig] int GetSelectedItems([MarshalAs(UnmanagedType.IUnknown)] out object ppsai);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            [PreserveSig] int BindToHandler(); // not fully defined
            [PreserveSig] int GetParent(); // not fully defined
            [PreserveSig] int GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            [PreserveSig] int GetAttributes();  // not fully defined
            [PreserveSig] int Compare();  // not fully defined
        }

#pragma warning disable CA1712 // Do not prefix enum values with type name
        private enum SIGDN : uint
        {
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_NORMALDISPLAY = 0,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_URL = 0x80068000
        }

        [Flags]
        private enum FOS
        {
            FOS_OVERWRITEPROMPT = 0x2,
            FOS_STRICTFILETYPES = 0x4,
            FOS_NOCHANGEDIR = 0x8,
            FOS_PICKFOLDERS = 0x20,
            FOS_FORCEFILESYSTEM = 0x40,
            FOS_ALLNONSTORAGEITEMS = 0x80,
            FOS_NOVALIDATE = 0x100,
            FOS_ALLOWMULTISELECT = 0x200,
            FOS_PATHMUSTEXIST = 0x800,
            FOS_FILEMUSTEXIST = 0x1000,
            FOS_CREATEPROMPT = 0x2000,
            FOS_SHAREAWARE = 0x4000,
            FOS_NOREADONLYRETURN = 0x8000,
            FOS_NOTESTFILECREATE = 0x10000,
            FOS_HIDEMRUPLACES = 0x20000,
            FOS_HIDEPINNEDPLACES = 0x40000,
            FOS_NODEREFERENCELINKS = 0x100000,
            FOS_OKBUTTONNEEDSINTERACTION = 0x200000,
            FOS_DONTADDTORECENT = 0x2000000,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_FORCEPREVIEWPANEON = 0x40000000,
            FOS_SUPPORTSTREAMABLEITEMS = unchecked((int)0x80000000)
        }
#pragma warning restore CA1712 // Do not prefix enum values with type name
    }
    internal class Folder
    {
        internal static bool IsExist(string path)
        {
            return Directory.Exists(path);
        }

        internal static void Create(string path)
        {
            Directory.CreateDirectory(path);
            return;
        }

        internal static string Open(string defaultpath = null)
        {
            var dlg = new FolderPicker();
            if (defaultpath != null)
            {
                dlg.InputPath = defaultpath;
            }

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    if (IsExist(dlg.ResultPath))
                        return dlg.ResultPath;
                    else
                        throw new DirectoryNotFoundException();
                }
                catch (Exception)
                {
                    MessageBox.Show("該路徑無法存取", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        internal static string Current
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
        }
    }
    internal class Parameters
    {
        internal List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
        internal void Add(string key, object value)
        {
            parameters.Add(new KeyValuePair<string, object>(key, value));
            return;
        }

        internal void Append(object value)
        {
            string key = string.Format("$param{0}", parameters.Count);
            Add(key, value);
            return;
        }

        internal int Count
        {
            get
            {
                return parameters.Count;
            }
        }

        internal string LastKey
        {
            get
            {
                if (parameters.Count > 0)
                    return parameters[parameters.Count - 1].Key;
                else
                    return null;
            }
        }

        internal KeyValuePair<string, object> Get(int index)
        {
            return parameters[index];
        }

        internal void Clear()
        {
            parameters.Clear();
        }
    }
    internal class Sqlite
    {
        private SqliteConnection SQL = null;
        private SqliteCommand CMD = null;
        private SqliteDataReader READER = null;

        internal string filename = "";
        internal string tablename = "";
        internal string command = "";
        internal List<string> fieldnames = new List<string>();
        internal List<object> values = new List<object>();
        internal Parameters parameters = new Parameters();
        internal string condition = "";

        #region constructor

        internal Sqlite(string filename, string tablename)
        {
            this.filename = filename;
            this.tablename = tablename;
            Open();
        }

        private void Open()
        {
            SQL = new SqliteConnection("Data Source=" + filename);
            SQL.Open();
            CMD = SQL.CreateCommand();
        }
        internal void Open(string filename, string tablename)
        {
            this.filename = filename;
            this.tablename = tablename;
            Open();
        }

        internal void Close()
        {
            CMD = null;
            if (SQL != null)
            {
                SQL.Close();
            }
        }

        #endregion

        internal void AddWithPair(KeyValuePair<string, object> keyValue)
        {
            AddWithPair(keyValue.Key, keyValue.Value);
        }

        internal void AddWithPair(string fieldname, object value)
        {
            fieldnames.Add(fieldname);
            values.Add(value);
        }

        internal void Clear()
        {
            fieldnames.Clear();
            values.Clear();
            parameters.Clear();
            condition = "";
            command = "";
        }

        internal void Insert()
        {
            // "Insert into tablename (column1, column2, column3) values (a, b, c)"
            command = string.Format("Insert into {0} (", tablename);
            for (int i = 0; i < fieldnames.Count; i++)
            {
                if (i != 0)
                    command += ", ";
                command += fieldnames[i];
            }
            command += ") values (";
            for (int i = 0; i < values.Count; i++)
            {
                parameters.Append(values[i]);
                if (i != 0)
                    command += ", ";
                command += parameters.LastKey;
            }
            command += ")";

            Execute();
            return;
        }
        internal void Update()
        {
            // Update tablename Set column1 = value1, column2 = value2 where condition
            command = string.Format("Update {0} Set ", tablename);
            for (int i = 0; i < fieldnames.Count; i++)
            {
                if (i != 0)
                    command += ", ";
                command += fieldnames[i] + " = ";
                parameters.Append(values[i]);
                command += parameters.LastKey;
            }
            if (condition != "")
            {
                command += string.Format(" where {0}", condition);
            }
            Execute();
            return;
        }
        internal void UpdateOrInsert()
        {
            if (Read().Length != 0)
            {
                Update();
            }
            else
            {
                Insert();
            }
        }
        internal object[][] Read()
        {
            // "Select column1, column2, column3 from tablename where condition"
            command = "Select ";
            if (fieldnames.Count == 0)
            {
                command += "*";
            }
            else
            {
                for (int i = 0; i < fieldnames.Count; i++)
                {
                    if (i != 0)
                        command += ", ";
                    command += fieldnames[i];
                }
            }
            command += string.Format(" from {0}", tablename);
            if (condition != "")
            {
                command += string.Format(" where {0}", condition);
            }
            ExecuteReader();

            List<object[]> result = new List<object[]>();
            List<object> content = new List<object>();
            while (READER.Read())
            {
                content.Clear();
                for (int i = 0; i < READER.FieldCount; i++)
                {
                    content.Add(READER.GetValue(i));
                }
                result.Add(content.ToArray());
            }
            READER.Close();
            READER = null;
            return result.ToArray();
        }
        internal object[][] Select()
        {
            return Read();
        }

        internal void Delete()
        {
            // Delete from tablename where condition
            command = string.Format("Delete from {0}", tablename);
            if (condition != "")
            {
                command += string.Format(" where {0}", condition);
            }
            Execute();
        }


        internal void ExecuteReader()
        {
            CMD.CommandText = command;

            for (int i = 0; i < parameters.Count; i++)
            {
                CMD.Parameters.AddWithValue(parameters.Get(i).Key, parameters.Get(i).Value);
            }
            READER = CMD.ExecuteReader();
            return;
        }
        internal void Execute()
        {
            CMD.CommandText = command;
            CMD.Parameters.Clear();
            for (int i = 0; i < parameters.Count; i++)
            {
                CMD.Parameters.AddWithValue(parameters.Get(i).Key, parameters.Get(i).Value);
            }
            CMD.ExecuteNonQuery();
            return;
        }
    }
    internal class File
    {
        internal static string Open()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == true)
            {
                return openFileDialog1.FileName;
            }
            else
            {
                return null;
            }
        }
        internal static string Read(string path)
        {
            StreamReader reader = new StreamReader(path);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        internal static void Write(string path, string content, bool append = false)
        {
            StreamWriter writer = new StreamWriter(path, append: append);
            writer.Write(content);
            writer.Close();
            return;
        }

        internal static void Append(string path, string content)
        {
            Write(path, content, true);
            return;
        }

        internal static void Create(string path)
        {
            Write(path, "", true);
            return;
        }

        internal static bool IsExist(string path)
        {
            try
            {
                StreamReader reader = new StreamReader(path);
                reader.Close();
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }
        internal static string ToFolder(string path)
        {
            return Path.GetDirectoryName(path);
        }

    }
}
