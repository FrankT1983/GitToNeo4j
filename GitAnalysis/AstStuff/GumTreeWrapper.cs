using GitAnalysis.AstStuff.InternalGraph;
using GitAnalysis.GraphClasses.Node;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalysis.AstStuff
{

    class GumTreeWrapper
    {
        [DebuggerDisplay("{src} - match -> {dest}")]
        public class GumMatch
        {
            public int src { get; set; }
            public int dest { get; set; }
        }

        [DebuggerDisplay("{action} : {label} ({tree})")]
        public class GumAction
        {
            public string action { get; set; }
            public int tree { get; set; }
            public string label { get; set; }
        }

        public class GumJsonDiffResult
        {
            public GumMatch[] matches { get; set; }
            public GumAction[] actions { get; set; }
        }

        public class GumTreeChange
        {
            internal static List<GumAction> FromConsoleOutput(string result)
            {
                var res = JsonConvert.DeserializeObject<GumJsonDiffResult>(result);
                return res.actions.ToList();
            }
        }

        public static List<GumAction> Compare(string content1, string content2)
        {
            var file1 = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
            var file2 = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
            System.IO.File.WriteAllText(file1, content1);
            System.IO.File.WriteAllText(file2, content2);

            var result = Run("C:\\PlayGround\\Java\\gumtree\\_release\\gumtree\\bin\\gumtree.bat", "jsondiff " + file1 + " " + file2);
            System.IO.File.Delete(file1);
            System.IO.File.Delete(file2);

            List<GumAction> res = GumTreeChange.FromConsoleOutput(result);
            return res;
        }

        public static AstGraph Parse(string content1)
        {
            var file1 = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
            System.IO.File.WriteAllText(file1, content1);

            var gumTreeResult = Run("java", " -jar C:\\PlayGround\\Java\\GumTreeClient.jar " + file1);
            System.IO.File.Delete(file1);
            return ParseGumTreeOutToGraph(gumTreeResult);
        }

        public static List<TransitionEdge> Compare2(string content1, string content2)
        {
            var file1 = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
            var file2 = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
            System.IO.File.WriteAllText(file1, content1);
            System.IO.File.WriteAllText(file2, content2);

            var gumTreeResult = Run("java", " -jar C:\\PlayGround\\Java\\GumTreeClient.jar " + file1 + " " + file2);

            System.IO.File.Delete(file1);
            System.IO.File.Delete(file2);

            return ParseGumTreeOutToTransitionGraph(gumTreeResult);
        }


        // todo: this is realy curde ... rewrite this and do tests
        public static InternalGraph.AstGraph ParseGumTreeOutToGraph(string toParse)
        {
            var result = new InternalGraph.AstGraph();

            var lines = toParse.Split('\n');
            foreach (var l in lines)
            {
                if (String.IsNullOrWhiteSpace(l)) { continue; }
                var parts = l.Trim().Split(' ');
                if (parts[0] != "file")
                {
                    continue;
                }

                if (parts[1].Equals("n"))
                {
                    // node
                    // file n 63 51 - 355 class
                    int id = int.Parse(parts[2]);
                    int start = int.Parse(parts[3]);
                    int end = int.Parse(parts[5]);
                    string type = parts[6];

                    var node = new AstElement() { AstId = id, Start = start, End = end, Type = type };
                    result.AddNode(node);
                    continue;
                }

                if (parts[1].Equals("e"))
                {
                    //file e 61 -> 34
                    int from = int.Parse(parts[2]);
                    int to = int.Parse(parts[4]);

                    var edge = new Edge() { From = from, To = to };
                    result.AddEdge(edge);
                    continue;
                }

                Console.WriteLine("could not pares line " + l);
            }

            return result;
        }

        public static List<TransitionEdge> ParseGumTreeOutToTransitionGraph(string toParse)
        {
            var result = new List<TransitionEdge>();

            var lines = toParse.Split('\n');
            foreach (var l in lines)
            {
                if (String.IsNullOrWhiteSpace(l)) { continue; }
                var parts = l.Trim().Split(' ');
                
                if (String.Compare(parts[0],"Action",true) != 0)
                {   continue;   }

                // Action Keep 45 55
                String mode = parts[1];
                int from = int.Parse(parts[2]);

                int to = -1;
                if (parts.Length > 3)
                {
                    to = int.Parse(parts[3]);
                }

                var edge = new TransitionEdge() { From = from, To = to, Mode =mode };
                result.Add(edge);              
            }

            return result;
        }


        // see https://stackoverflow.com/questions/15360624/wrapper-for-a-command-line-tool-in-c-sharp?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        private static string Run(string exeName, string argsLine, int timeoutSeconds = 0)
        {
            StreamReader outputStream = StreamReader.Null;
            string output = "";
            bool success = false;

            try
            {
                Process newProcess = new Process();
                newProcess.StartInfo.FileName = exeName;
                newProcess.StartInfo.Arguments = argsLine;
                newProcess.StartInfo.UseShellExecute = false;
                newProcess.StartInfo.CreateNoWindow = true; //The command line is supressed to keep the process in the background
                newProcess.StartInfo.RedirectStandardOutput = true;
                newProcess.Start();
                if (0 == timeoutSeconds)
                {
                    outputStream = newProcess.StandardOutput;
                    output = outputStream.ReadToEnd();
                    newProcess.WaitForExit();
                }
                else
                {
                    success = newProcess.WaitForExit(timeoutSeconds * 1000);

                    if (success)
                    {
                        outputStream = newProcess.StandardOutput;
                        output = outputStream.ReadToEnd();
                    }
                    else
                    {
                        output = "Timed out at " + timeoutSeconds + " seconds waiting for " + exeName + " to exit.";
                    }
                }
            }
            catch (Exception e)
            {
                throw (new Exception("An error occurred running " + exeName + ".", e));
            }
            finally
            {
                outputStream.Close();
            }

            return "\t" + output;
        }
    }

}