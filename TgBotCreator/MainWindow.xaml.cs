using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Diagnostics;

namespace TgBotCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<TextBox> functionList = new List<TextBox>();
        List<TextBox> envList = new List<TextBox>();
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            requirementhint.Text = AllRequirementsFilled();
            if ( requirementhint.Text != "")
            {
                return;
            }


            string foldername = outputpath.Text + "\\" + projectname.Text + "\\";
            Folder.Create(foldername);
            // Main.py
            string filename = foldername + "Main.py";

            string cmdInFile = "";

            cmdInFile += Sample.sample["Main.py"].Internal.ToString(string.Format("'{0}', {1}", "start", "Start_Bot")) + "\n";
            foreach (var i in functionList)
            {
                if (i.Text != null && i.Text != "")
                {
                    cmdInFile += Sample.sample["Main.py"].Internal.ToString(string.Format("'{0}', {0}", i.Text)) + "\n";
                }
            }

            File.Write(filename, Sample.sample["Main.py"].ToString(cmdInFile + "\n"));

            // Command.py
            filename = foldername + "Command.py";

            cmdInFile = "";

            cmdInFile += Sample.sample["Command.py"].Internal.ToString("Start_Bot") + "\n";
            foreach (var i in functionList)
            {
                if (i.Text != null && i.Text != "")
                {
                    cmdInFile += Sample.sample["Command.py"].Internal.ToString(i.Text) + "\n";
                }
            }

            File.Write(filename, Sample.sample["Command.py"].ToString(cmdInFile + "\n"));

            // .env
            filename = foldername + ".env";
            cmdInFile = "";

            foreach (var i in envList)
            {
                if (i.Text != null && i.Text != "")
                {
                    cmdInFile += Sample.sample[".env"].Internal.ToString(string.Format("{0}='{1}'", i.Name, i.Text)) + "\n";
                }
            }
            File.Write(filename, Sample.sample[".env"].ToString(cmdInFile));

            // interact_with_imgur.py
            filename = foldername + "interact_with_imgur.py";
            File.Write(filename, Sample.sample["interact_with_imgur.py"].ToString());

            // .gitignore
            filename = foldername + ".gitignore";
            File.Write(filename, Sample.sample[".gitignore"].ToString());

            // updater.py
            filename = foldername + "updater.py";
            File.Write(filename, Sample.sample["updater.py"].ToString());

            // function.py
            filename = foldername + "function.py";
            File.Write(filename, Sample.sample["function.py"].ToString());

            // LICENSE
            filename = foldername + "LICENSE";
            File.Write(filename, Sample.sample["LICENSE"].ToString());

            // run.sh
            filename = foldername + "run.sh";
            File.Write(filename, Sample.sample["run.sh"].ToString());

            // done
            if(MessageBox.Show("創建完成，是否開啟目標資料夾？", "Question", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                if ((showin.SelectedItem as ComboBoxItem).Content.ToString() == "Explorer")
                {
                    Process.Start("explorer", string.Format("/select, {0}", foldername));
                }
                else if ((showin.SelectedItem as ComboBoxItem).Content.ToString() == "Visual Studio Code")
                {
                    CMD.Run("code " + foldername);
                }
            }
            
        }

        private string AllRequirementsFilled()
        {
            if(projectname.Text == "")
            {
                return "Enter project name";
            }
            else if (!Folder.IsExist(outputpath.Text))
            {
                return "Enter the attachable output path";
            }
            else if (TELEGRAM_TOKEN.Text == "")
            {
                return "Enter Telegram token";
            }
            else if (LICENSE.Text == "")
            {
                return "Select a License";
            }
            else
            {
                return "";
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitPath();
            
            functionList.Add(func1);
            envList.Add(TELEGRAM_TOKEN);
            envList.Add(IMGUR_CLIENT_ID);
            LICENSE.SelectedIndex = 0;

            if(Config.Get("showin") == null)
            {
                showin.SelectedIndex = 0;
                Config.Set("showin", 0);
            }
            else
            {
                showin.SelectedIndex = (int)Config.GetInt("showin");
            }
            
        }

        private void InitPath()
        {
            if (Config.Get("outputpath") == null)
            {
                Config.Set("outputpath", Folder.Current);
            }
            outputpath.Text = Config.Get("outputpath").ToString();
        }

        private void setoutputpath_Click(object sender, RoutedEventArgs e)
        {
            string newpath = Folder.Open();
            if (newpath != null)
            {
                outputpath.Text = newpath;
                Config.Set("outputpath", newpath);
            }
        }

        private void Func_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as TextBox).Text == "" && functionList.Count > 1 && !(sender as TextBox).IsFocused && (sender as TextBox) != functionList[functionList.Count - 1])
            {
                functions.Children.Remove(sender as TextBox);
                functionList.Remove(sender as TextBox);
            }
        }

        private void Func_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text != "" && (sender as TextBox) == functionList[functionList.Count - 1])
            {
                TextBox newTextBox = new TextBox();
                newTextBox.Width = functionList[0].Width;
                newTextBox.FontSize = functionList[0].FontSize;
                newTextBox.IsKeyboardFocusedChanged += Func_IsKeyboardFocusedChanged;
                newTextBox.TextChanged += Func_TextChanged;
                functions.Children.Add(newTextBox);
                functionList.Add(newTextBox);
                functions_scrollviewer.ScrollToVerticalOffset(int.MaxValue);
            }
            else if((sender as TextBox).Text == "")
            {
                if (functionList.Count > 1 && (sender as TextBox) == functionList[functionList.Count - 2])
                {
                    functions.Children.Remove(functionList[functionList.Count - 1]);
                    functionList.RemoveAt(functionList.Count - 1);
                    
                }
                else if ((sender as TextBox) != functionList[0])
                {
                    functionList[functionList.IndexOf(sender as TextBox) - 1].Focus();
                    functions.Children.Remove(sender as TextBox);
                    functionList.Remove(sender as TextBox);
                }
                else if (functionList.Count>1)
                {
                    functions.Children.Remove(sender as TextBox);
                    functionList.Remove(sender as TextBox);
                    functionList[0].Focus();
                }
            }
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/tsai1247/TgBotCreator");
        }

        private void showin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Set("showin", showin.SelectedIndex);
        }
    }
}
