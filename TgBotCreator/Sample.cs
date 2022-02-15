using System;
using System.Collections.Generic;
using System.Text;

namespace TgBotCreator
{
    class Words
    {
        public static Words Create(object first, Words? word, object second)
        {
            return new Words(first.ToString(), word, second.ToString());
        }

        public Tuple<string, Words?, string> data;
        public Words(string first, Words? word, string second)
        {
            data = Tuple.Create(first, word, second);
        }
        public Words(Tuple<string, Words?, string> data)
        {
            this.data = data;
        }

        public string Head
        {
            get { return data.Item1; }
        }
        public string Tail
        {
            get { return data.Item3; }
        }
        public Words Internal
        {
            get {
                if (data.Item2 == null)
                    return Sample.sample[""];
                else
                    return data.Item2; }
        }
        public bool IsInternalNull()
        {
            return Internal == null;
        }

        public string ToString(string inter = "")
        {
            return Head + inter + Tail;
        }

    }

    internal class Sample
    {
        public static Dictionary<string, Words> sample = new Dictionary<string, Words>(){
            { "", Words.Create("", null, "")},
            { "Main.py", Words.Create(Config.Get("mainhead"), Words.Create(Config.Get("maincommandhead"), null, Config.Get("maincommandtail")), Config.Get("maintail")) },
            { "Command.py", Words.Create(Config.Get("commandhead"), Words.Create(Config.Get("commandcontenthead"), null, Config.Get("commandcontenttail")), Config.Get("commandtail")) },
            { ".env", Words.Create("", null, "")},
            { "interact_with_imgur.py", Words.Create(Config.Get("interact_with_imgur"), null, "")},
            { ".gitignore", Words.Create(Config.Get(".gitignore"), null, "")},
            { "updater.py", Words.Create(Config.Get("updater"), null, "")},
            { "function.py", Words.Create(Config.Get("function"), null, "")},
            { "LICENSE", Words.Create(Config.Get("MPL2.0"), null, "")},
            { "run.sh", Words.Create(Config.Get("run"), null, "")},
        };
    }
}