/*
 * Copyright (c) Contributors, https://hyperionvirtual.com/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Hyperion Virtual Worlds Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.IO;
using Microsoft.CSharp;
using Microsoft.JScript;
using Microsoft.VisualBasic;
using log4net;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.ScriptEngine.Interfaces;
using OpenMetaverse;

namespace OpenSim.Region.ScriptEngine.Shared.CodeTools
{
    public class Compiler : ICompiler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // * Uses "LSL2Converter" to convert LSL to C# if necessary.
        // * Compiles C#-code into an assembly
        // * Returns assembly name ready for AppDomain load.
        //
        // Assembly is compiled using LSL_BaseClass as base. Look at debug C# code file created when LSL script is compiled for full details.
        //

        internal enum enumCompileType
        {
            lsl = 0,
            cs = 1,
            vb = 2,
            js = 3,
            yp = 4
        }

        /// <summary>
        /// This contains number of lines WE use for header when compiling script. User will get error in line x-LinesToRemoveOnError when error occurs.
        /// </summary>
        public int LinesToRemoveOnError = 3;
        private enumCompileType DefaultCompileLanguage;
        private bool WriteScriptSourceToDebugFile;
        private bool CompileWithDebugInformation;
        private Dictionary<string, bool> AllowedCompilers = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, enumCompileType> LanguageMapping = new Dictionary<string, enumCompileType>(StringComparer.CurrentCultureIgnoreCase);

        private string FilePrefix;
        private string ScriptEnginesPath = "ScriptEngines";
        // mapping between LSL and C# line/column numbers
        private Dictionary<KeyValuePair<int, int>, KeyValuePair<int, int>> m_positionMap; 
        private ICodeConverter LSL_Converter;

        private List<string> m_warnings = new List<string>();

        // private object m_syncy = new object();

        private static CSharpCodeProvider CScodeProvider = new CSharpCodeProvider();
        private static VBCodeProvider VBcodeProvider = new VBCodeProvider();
        private static JScriptCodeProvider JScodeProvider = new JScriptCodeProvider();
        private static CSharpCodeProvider YPcodeProvider = new CSharpCodeProvider(); // YP is translated into CSharp
        private static YP2CSConverter YP_Converter = new YP2CSConverter();

        // private static int instanceID = new Random().Next(0, int.MaxValue);                 // Unique number to use on our compiled files
        private static UInt64 scriptCompileCounter = 0;                                     // And a counter

        public IScriptEngine m_scriptEngine;
        public Compiler(IScriptEngine scriptEngine)
        {
            m_scriptEngine = scriptEngine;
            ReadConfig();
        }

        public bool in_startup = true;
        public void ReadConfig()
        {
            // Get some config
            WriteScriptSourceToDebugFile = m_scriptEngine.Config.GetBoolean("WriteScriptSourceToDebugFile", false);
            CompileWithDebugInformation = m_scriptEngine.Config.GetBoolean("CompileWithDebugInformation", true);

            // Get file prefix from scriptengine name and make it file system safe:
            FilePrefix = "CommonCompiler";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                FilePrefix = FilePrefix.Replace(c, '_');
            }

            // First time we start? Delete old files
            if (in_startup)
            {
                in_startup = false;
                DeleteOldFiles();
            }

            // Map name and enum type of our supported languages
            LanguageMapping.Add(enumCompileType.cs.ToString(), enumCompileType.cs);
            LanguageMapping.Add(enumCompileType.vb.ToString(), enumCompileType.vb);
            LanguageMapping.Add(enumCompileType.lsl.ToString(), enumCompileType.lsl);
            LanguageMapping.Add(enumCompileType.js.ToString(), enumCompileType.js);
            LanguageMapping.Add(enumCompileType.yp.ToString(), enumCompileType.yp);

            // Allowed compilers
            string allowComp = m_scriptEngine.Config.GetString("AllowedCompilers", "lsl");
            AllowedCompilers.Clear();

#if DEBUG
            m_log.Debug("[Compiler]: Allowed languages: " + allowComp);
#endif


            foreach (string strl in allowComp.Split(','))
            {
                string strlan = strl.Trim(" \t".ToCharArray()).ToLower();
                if (!LanguageMapping.ContainsKey(strlan))
                {
                    m_log.Error("[Compiler]: Config error. Compiler is unable to recognize language type \"" + strlan + "\" specified in \"AllowedCompilers\".");
                }
                else
                {
#if DEBUG
                    //m_log.Debug("[Compiler]: Config OK. Compiler recognized language type \"" + strlan + "\" specified in \"AllowedCompilers\".");
#endif
                }
                AllowedCompilers.Add(strlan, true);
            }
            if (AllowedCompilers.Count == 0)
                m_log.Error("[Compiler]: Config error. Compiler could not recognize any language in \"AllowedCompilers\". Scripts will not be executed!");

            // Default language
            string defaultCompileLanguage = m_scriptEngine.Config.GetString("DefaultCompileLanguage", "lsl").ToLower();

            // Is this language recognized at all?
            if (!LanguageMapping.ContainsKey(defaultCompileLanguage))
            {
                m_log.Error("[Compiler]: " +
                                            "Config error. Default language \"" + defaultCompileLanguage + "\" specified in \"DefaultCompileLanguage\" is not recognized as a valid language. Changing default to: \"lsl\".");
                defaultCompileLanguage = "lsl";
            }

            // Is this language in allow-list?
            if (!AllowedCompilers.ContainsKey(defaultCompileLanguage))
            {
                m_log.Error("[Compiler]: " +
                            "Config error. Default language \"" + defaultCompileLanguage + "\"specified in \"DefaultCompileLanguage\" is not in list of \"AllowedCompilers\". Scripts may not be executed!");
            }
            else
            {
#if DEBUG
//                m_log.Debug("[Compiler]: " +
//                                            "Config OK. Default language \"" + defaultCompileLanguage + "\" specified in \"DefaultCompileLanguage\" is recognized as a valid language.");
#endif
                // LANGUAGE IS IN ALLOW-LIST
                DefaultCompileLanguage = LanguageMapping[defaultCompileLanguage];
            }

            // We now have an allow-list, a mapping list, and a default language

        }

        /// <summary>
        /// Delete old script files
        /// </summary>
        private void DeleteOldFiles()
        {
            // CREATE FOLDER IF IT DOESNT EXIST
            if (!Directory.Exists(ScriptEnginesPath))
            {
                try
                {
                    Directory.CreateDirectory(ScriptEnginesPath);
                }
                catch (Exception ex)
                {
                    m_log.Error("[Compiler]: Exception trying to create ScriptEngine directory \"" + ScriptEnginesPath + "\": " + ex.ToString());
                }
            }

            if (!Directory.Exists(Path.Combine(ScriptEnginesPath,
                    m_scriptEngine.World.RegionInfo.RegionID.ToString())))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(ScriptEnginesPath,
                        m_scriptEngine.World.RegionInfo.RegionID.ToString()));
                }
                catch (Exception ex)
                {
                    m_log.Error("[Compiler]: Exception trying to create ScriptEngine directory \"" + Path.Combine(ScriptEnginesPath,
                                            m_scriptEngine.World.RegionInfo.RegionID.ToString())+ "\": " + ex.ToString());
                }
            }

            foreach (string file in Directory.GetFiles(Path.Combine(ScriptEnginesPath,
                     m_scriptEngine.World.RegionInfo.RegionID.ToString()),FilePrefix + "_compiled*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    m_log.Error("[Compiler]: Exception trying delete old script file \"" + file + "\": " + ex.ToString());
                }
            }
            foreach (string file in Directory.GetFiles(Path.Combine(ScriptEnginesPath,
                    m_scriptEngine.World.RegionInfo.RegionID.ToString()), FilePrefix + "_source*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    m_log.Error("[Compiler]: Exception trying delete old script file \"" + file + "\": " + ex.ToString());
                }
            }
        }

        ////private ICodeCompiler icc = codeProvider.CreateCompiler();
        //public string CompileFromFile(string LSOFileName)
        //{
        //    switch (Path.GetExtension(LSOFileName).ToLower())
        //    {
        //        case ".txt":
        //        case ".lsl":
        //            Common.ScriptEngineBase.Shared.SendToDebug("Source code is LSL, converting to CS");
        //            return CompileFromLSLText(File.ReadAllText(LSOFileName));
        //        case ".cs":
        //            Common.ScriptEngineBase.Shared.SendToDebug("Source code is CS");
        //            return CompileFromCSText(File.ReadAllText(LSOFileName));
        //        default:
        //            throw new Exception("Unknown script type.");
        //    }
        //}

        public string GetAssemblyName(UUID assetID)
        {
            return Path.Combine(ScriptEnginesPath, Path.Combine(
                    m_scriptEngine.World.RegionInfo.RegionID.ToString(),
                    FilePrefix + "_compiled_" + assetID + ".dll"));
        }

        /// <summary>
        /// Converts script from LSL to CS and calls CompileFromCSText
        /// </summary>
        /// <param name="Script">LSL script</param>
        /// <returns>Filename to .dll assembly</returns>
        public string PerformScriptCompile(string Script, string asset, UUID ownerUUID)
        {
            m_positionMap = null;
            m_warnings.Clear();
                
            string OutFile = Path.Combine(ScriptEnginesPath, Path.Combine(
                    m_scriptEngine.World.RegionInfo.RegionID.ToString(),
                    FilePrefix + "_compiled_" + asset + ".dll"));
//            string OutFile = Path.Combine(ScriptEnginesPath,
//                    FilePrefix + "_compiled_" + asset + ".dll");

            if (!Directory.Exists(ScriptEnginesPath))
            {
                try
                {
                    Directory.CreateDirectory(ScriptEnginesPath);
                }
                catch (Exception)
                {
                }
            }

            if (!Directory.Exists(Path.Combine(ScriptEnginesPath,
                                               m_scriptEngine.World.RegionInfo.RegionID.ToString())))
            {
                try
                {
                    Directory.CreateDirectory(ScriptEnginesPath);
                }
                catch (Exception)
                {
                }
            }

            if (Script == String.Empty)
            {
                if (File.Exists(OutFile))
                    return OutFile;

                throw new Exception("Cannot find script assembly and no script text present");
            }

            // Don't recompile if we already have it
            //
            if (File.Exists(OutFile) && File.Exists(OutFile+".text") && File.Exists(OutFile+".map"))
            {
                ReadMapFile(OutFile+".map");
                return OutFile;
            }

            enumCompileType l = DefaultCompileLanguage;

            if (Script.StartsWith("//c#", true, CultureInfo.InvariantCulture))
                l = enumCompileType.cs;
            if (Script.StartsWith("//vb", true, CultureInfo.InvariantCulture))
            {
                l = enumCompileType.vb;
                // We need to remove //vb, it won't compile with that

                Script = Script.Substring(4, Script.Length - 4);
            }
            if (Script.StartsWith("//lsl", true, CultureInfo.InvariantCulture))
                l = enumCompileType.lsl;

            if (Script.StartsWith("//js", true, CultureInfo.InvariantCulture))
                l = enumCompileType.js;

            if (Script.StartsWith("//yp", true, CultureInfo.InvariantCulture))
                l = enumCompileType.yp;

            if (!AllowedCompilers.ContainsKey(l.ToString()))
            {
                // Not allowed to compile to this language!
                string errtext = String.Empty;
                errtext += "The compiler for language \"" + l.ToString() + "\" is not in list of allowed compilers. Script will not be executed!";
                throw new Exception(errtext);
            }

            if (m_scriptEngine.World.Permissions.CanCompileScript(ownerUUID, (int)l) == false) {
                // Not allowed to compile to this language!
                string errtext = String.Empty;
                errtext += ownerUUID + " is not in list of allowed users for this scripting language. Script will not be executed!";
                throw new Exception(errtext);
            }
            
            string compileScript = Script;

            if (l == enumCompileType.lsl)
            {
                // Its LSL, convert it to C#
                LSL_Converter = (ICodeConverter)new CSCodeGenerator();
                compileScript = LSL_Converter.Convert(Script);

                // copy converter warnings into our warnings.
                foreach (string warning in LSL_Converter.GetWarnings())
                {
                    AddWarning(warning);
                }

                m_positionMap = ((CSCodeGenerator) LSL_Converter).PositionMap;
            }

            if (l == enumCompileType.yp)
            {
                // Its YP, convert it to C#
                compileScript = YP_Converter.Convert(Script);
            }

            switch (l)
            {
                case enumCompileType.cs:
                case enumCompileType.lsl:
                    compileScript = CreateCSCompilerScript(compileScript);
                    break;
                case enumCompileType.vb:
                    compileScript = CreateVBCompilerScript(compileScript);
                    break;
                case enumCompileType.js:
                    compileScript = CreateJSCompilerScript(compileScript);
                    break;
                case enumCompileType.yp:
                    compileScript = CreateYPCompilerScript(compileScript);
                    break;
            }

            return CompileFromDotNetText(compileScript, l, asset);
        }

        public string[] GetWarnings()
        {
            return m_warnings.ToArray();
        }

        private void AddWarning(string warning)
        {
            if (!m_warnings.Contains(warning))
            {
                m_warnings.Add(warning);
            }
        }

        private static string CreateJSCompilerScript(string compileScript)
        {
            compileScript = String.Empty +
                "import OpenSim.Region.ScriptEngine.Shared; import System.Collections.Generic;\r\n" +
                "package SecondLife {\r\n" +
                "class Script extends OpenSim.Region.ScriptEngine.Shared.ScriptBase.ScriptBaseClass { \r\n" +
                compileScript +
                "} }\r\n";
            return compileScript;
        }

        private static string CreateCSCompilerScript(string compileScript)
        {
            compileScript = String.Empty +
                "using OpenSim.Region.ScriptEngine.Shared; using System.Collections.Generic;\r\n" +
                String.Empty + "namespace SecondLife { " +
                String.Empty + "public class Script : OpenSim.Region.ScriptEngine.Shared.ScriptBase.ScriptBaseClass { \r\n" +
                @"public Script() { } " +
                compileScript +
                "} }\r\n";
            return compileScript;
        }

        private static string CreateYPCompilerScript(string compileScript)
        {
            compileScript = String.Empty +
                       "using OpenSim.Region.ScriptEngine.Shared.YieldProlog; " +
                        "using OpenSim.Region.ScriptEngine.Shared; using System.Collections.Generic;\r\n" +
                        String.Empty + "namespace SecondLife { " +
                        String.Empty + "public class Script : OpenSim.Region.ScriptEngine.Shared.ScriptBase.ScriptBaseClass  { \r\n" +
                        //@"public Script() { } " +
                        @"static OpenSim.Region.ScriptEngine.Shared.YieldProlog.YP YP=null; " +
                        @"public Script() {  YP= new OpenSim.Region.ScriptEngine.Shared.YieldProlog.YP(); } " +

                        compileScript +
                        "} }\r\n";
            return compileScript;
        }

        private static string CreateVBCompilerScript(string compileScript)
        {
            compileScript = String.Empty +
                "Imports OpenSim.Region.ScriptEngine.Shared: Imports System.Collections.Generic: " +
                String.Empty + "NameSpace SecondLife:" +
                String.Empty + "Public Class Script: Inherits OpenSim.Region.ScriptEngine.Shared.ScriptBase.ScriptBaseClass: " +
                "\r\nPublic Sub New()\r\nEnd Sub: " +
                compileScript +
                ":End Class :End Namespace\r\n";
            return compileScript;
        }

        /// <summary>
        /// Compile .NET script to .Net assembly (.dll)
        /// </summary>
        /// <param name="Script">CS script</param>
        /// <returns>Filename to .dll assembly</returns>
        internal string CompileFromDotNetText(string Script, enumCompileType lang, string asset)
        {
            string ext = "." + lang.ToString();

            // Output assembly name
            scriptCompileCounter++;
            string OutFile = Path.Combine(ScriptEnginesPath, Path.Combine(
                    m_scriptEngine.World.RegionInfo.RegionID.ToString(),
                    FilePrefix + "_compiled_" + asset + ".dll"));
            try
            {
                File.Delete(OutFile);
            }
            catch (Exception e) // NOTLEGIT - Should be just FileIOException
            {
                throw new Exception("Unable to delete old existing "+
                        "script-file before writing new. Compile aborted: " +
                        e.ToString());
            }

            // DEBUG - write source to disk
            if (WriteScriptSourceToDebugFile)
            {
                string srcFileName = FilePrefix + "_source_" +
                        Path.GetFileNameWithoutExtension(OutFile) + ext;
                try
                {
                    File.WriteAllText(Path.Combine(Path.Combine(
                        ScriptEnginesPath,
                        m_scriptEngine.World.RegionInfo.RegionID.ToString()),
                        srcFileName), Script);
                }
                catch (Exception ex) //NOTLEGIT - Should be just FileIOException
                {
                    m_log.Error("[Compiler]: Exception while "+
                                "trying to write script source to file \"" +
                                srcFileName + "\": " + ex.ToString());
                }
            }

            // Do actual compile
            CompilerParameters parameters = new CompilerParameters();

            parameters.IncludeDebugInformation = true;

            string rootPath =
                Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            parameters.ReferencedAssemblies.Add(Path.Combine(rootPath,
                    "OpenSim.Region.ScriptEngine.Shared.dll"));
            parameters.ReferencedAssemblies.Add(Path.Combine(rootPath,
                    "OpenSim.Region.ScriptEngine.Shared.Api.Runtime.dll"));

            if (lang == enumCompileType.yp)
            {
                parameters.ReferencedAssemblies.Add(Path.Combine(rootPath,
                        "OpenSim.Region.ScriptEngine.Shared.YieldProlog.dll"));
            }

            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = OutFile;
            parameters.IncludeDebugInformation = CompileWithDebugInformation;
            //parameters.WarningLevel = 1; // Should be 4?
            parameters.TreatWarningsAsErrors = false;

            CompilerResults results;
            switch (lang)
            {
                case enumCompileType.vb:
                    results = VBcodeProvider.CompileAssemblyFromSource(
                            parameters, Script);
                    break;
                case enumCompileType.cs:
                case enumCompileType.lsl:
                    lock (CScodeProvider)
                    {
                        results = CScodeProvider.CompileAssemblyFromSource(
                                parameters, Script);
                    }
                    break;
                case enumCompileType.js:
                    results = JScodeProvider.CompileAssemblyFromSource(
                            parameters, Script);
                    break;
                case enumCompileType.yp:
                    results = YPcodeProvider.CompileAssemblyFromSource(
                            parameters, Script);
                    break;
                default:
                    throw new Exception("Compiler is not able to recongnize "+
                            "language type \"" + lang.ToString() + "\"");
            }

            // Check result
            // Go through errors

            //
            // WARNINGS AND ERRORS
            //
            int display = 5;
            if (results.Errors.Count > 0)
            {
                string errtext = String.Empty;
                foreach (CompilerError CompErr in results.Errors)
                {
                    // Show 5 errors max
                    //
                    if (display <= 0)
                        break;
                    display--;

                    string severity = "Error";
                    if (CompErr.IsWarning)
                    {
                        severity = "Warning";
                    }

                    KeyValuePair<int, int> lslPos;

                    lslPos = FindErrorPosition(CompErr.Line, CompErr.Column);

                    string text = CompErr.ErrorText;

                    // Use LSL type names
                    if (lang == enumCompileType.lsl)
                        text = ReplaceTypes(CompErr.ErrorText);

                    // The Second Life viewer's script editor begins
                    // countingn lines and columns at 0, so we subtract 1.
                    errtext += String.Format("Line ({0},{1}): {4} {2}: {3}\n",
                            lslPos.Key - 1, lslPos.Value - 1,
                            CompErr.ErrorNumber, text, severity);
                }
                
                if (!File.Exists(OutFile))
                {
                    throw new Exception(errtext);
                }
            }

            //
            // NO ERRORS, BUT NO COMPILED FILE
            //
            if (!File.Exists(OutFile))
            {
                string errtext = String.Empty;
                errtext += "No compile error. But not able to locate compiled file.";
                throw new Exception(errtext);
            }
//            m_log.DebugFormat("[Compiler] Compiled new assembly "+
//                    "for {0}", asset);

            // Because windows likes to perform exclusive locks, we simply
            // write out a textual representation of the file here
            //
            // Read the binary file into a buffer
            //
            FileInfo fi = new FileInfo(OutFile);

            if (fi == null)
            {
                string errtext = String.Empty;
                errtext += "No compile error. But not able to stat file.";
                throw new Exception(errtext);
            }

            Byte[] data = new Byte[fi.Length];

            try
            {
                FileStream fs = File.Open(OutFile, FileMode.Open, FileAccess.Read);
                fs.Read(data, 0, data.Length);
                fs.Close();
            }
            catch (Exception)
            {
                string errtext = String.Empty;
                errtext += "No compile error. But not able to open file.";
                throw new Exception(errtext);
            }

            // Convert to base64
            //
            string filetext = System.Convert.ToBase64String(data);

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            Byte[] buf = enc.GetBytes(filetext);

            FileStream sfs = File.Create(OutFile+".text");
            sfs.Write(buf, 0, buf.Length);
            sfs.Close();

            string posmap = String.Empty;
            if (m_positionMap != null)
            {
                foreach (KeyValuePair<KeyValuePair<int, int>, KeyValuePair<int, int>> kvp in m_positionMap)
                {
                    KeyValuePair<int, int> k = kvp.Key;
                    KeyValuePair<int, int> v = kvp.Value;
                    posmap += String.Format("{0},{1},{2},{3}\n",
                            k.Key, k.Value, v.Key, v.Value);
                }
            }

            buf = enc.GetBytes(posmap);

            FileStream mfs = File.Create(OutFile+".map");
            mfs.Write(buf, 0, buf.Length);
            mfs.Close();

            return OutFile;
        }

        public KeyValuePair<int, int> FindErrorPosition(int line, int col)
        {
            return FindErrorPosition(line, col, m_positionMap);
        }

        private class kvpSorter : IComparer<KeyValuePair<int,int>>
        {
            public int Compare(KeyValuePair<int,int> a,
                    KeyValuePair<int,int> b)
            {
                return a.Key.CompareTo(b.Key);
            }
        }

        public static KeyValuePair<int, int> FindErrorPosition(int line,
                int col, Dictionary<KeyValuePair<int, int>,
                KeyValuePair<int, int>> positionMap)
        {
            if (positionMap == null || positionMap.Count == 0)
                return new KeyValuePair<int, int>(line, col);

            KeyValuePair<int, int> ret = new KeyValuePair<int, int>();

            if (positionMap.TryGetValue(new KeyValuePair<int, int>(line, col),
                    out ret))
                return ret;

            List<KeyValuePair<int,int>> sorted =
                    new List<KeyValuePair<int,int>>(positionMap.Keys);

            sorted.Sort(new kvpSorter());

            int l = 1;
            int c = 1;

            foreach (KeyValuePair<int, int> cspos in sorted)
            {
                if (cspos.Key >= line)
                {
                    if (cspos.Key > line)
                        return new KeyValuePair<int, int>(l, c);
                    if (cspos.Value > col)
                        return new KeyValuePair<int, int>(l, c);
                    c = cspos.Value;
                    if (c == 0)
                        c++;
                }
                else
                {
                    l = cspos.Key;
                }
            }
            return new KeyValuePair<int, int>(l, c);
        }

        string ReplaceTypes(string message)
        {
            message = message.Replace(
                    "OpenSim.Region.ScriptEngine.Shared.LSL_Types.LSLString",
                    "string");

            message = message.Replace(
                    "OpenSim.Region.ScriptEngine.Shared.LSL_Types.LSLInteger",
                    "integer");

            message = message.Replace(
                    "OpenSim.Region.ScriptEngine.Shared.LSL_Types.LSLFloat",
                    "float");

            message = message.Replace(
                    "OpenSim.Region.ScriptEngine.Shared.LSL_Types.list",
                    "list");

            return message;
        }

        public Dictionary<KeyValuePair<int, int>, KeyValuePair<int, int>> LineMap()
        {
            if (m_positionMap == null)
                return null;
            
            Dictionary<KeyValuePair<int, int>, KeyValuePair<int, int>> ret =
                new Dictionary<KeyValuePair<int,int>, KeyValuePair<int, int>>();
            
            foreach (KeyValuePair<int, int> kvp in m_positionMap.Keys)
                ret.Add(kvp, m_positionMap[kvp]);
            
            return ret;
        }

        private void ReadMapFile(string filename)
        {
            try
            {
                StreamReader r = File.OpenText(filename);

                m_positionMap = new Dictionary<KeyValuePair<int,int>, KeyValuePair<int, int>>();
                
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    String[] parts = line.Split(new Char[] {','});
                    int kk = System.Convert.ToInt32(parts[0]);
                    int kv = System.Convert.ToInt32(parts[1]);
                    int vk = System.Convert.ToInt32(parts[2]);
                    int vv = System.Convert.ToInt32(parts[3]);

                    KeyValuePair<int, int> k = new KeyValuePair<int, int>(kk, kv);
                    KeyValuePair<int, int> v = new KeyValuePair<int, int>(vk, vv);

                    m_positionMap[k] = v;
                }
            }
            catch
            {
            }
        }
    }
}
