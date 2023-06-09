using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ArgsParserNS
{
    public class ArgsParser
    {
        public List<Argument> RegisteredArguments = new List<Argument>();

        private Argument GetRegisteredArg(string name)
        {
            foreach (Argument arg in RegisteredArguments)
            {
                if (name == arg.Name)
                    return arg;
                else if (name == arg.ShortName)
                    return arg;
            }
            
            return new Argument() { Name = null, ShortName = null };
        }

        public FilledArgument[] ParseArguments(string[] args)
        {
            List<FilledArgument> parsedArgs = new List<FilledArgument>();
            
            foreach (string arg in args)
            {
                if (arg.Contains("--"))
                {
                    if (arg.Contains("="))
                    {
                        string[] splittedArg = arg.Split('=');
                        string argName = splittedArg[0].Replace("--", string.Empty);
                        string argValue = splittedArg[1];
                        Argument argStruct = GetRegisteredArg(argName);

                        if (argStruct.Name != null)
                        {
                            FilledArgument finalArg = new FilledArgument();
                            finalArg.Arg = argStruct;
                            finalArg.Value = argValue;
                            parsedArgs.Add(finalArg);
                        }
                    }
                    else 
                    {
                        string argName = arg.Replace("--", string.Empty);
                        Argument argStruct = GetRegisteredArg(argName);

                        if (argStruct.Name != null)
                        {
                            FilledArgument finalArg = new FilledArgument();
                            finalArg.Arg = argStruct;
                            finalArg.Value = null;
                            parsedArgs.Add(finalArg);
                        }
                    }
                }
                else if (arg.Contains("-"))
                {
                    if (arg.Contains("="))
                    {
                        string[] splittedArg = arg.Split('=');
                        string argName = splittedArg[0].Replace("-", string.Empty);
                        string argValue = splittedArg[1];
                        Argument argStruct = GetRegisteredArg(argName);

                        if (argStruct.Name != null)
                        {
                            FilledArgument finalArg = new FilledArgument();
                            finalArg.Arg = argStruct;
                            finalArg.Value = argValue;
                            parsedArgs.Add(finalArg);
                        }
                    }
                    else 
                    {
                        string argName = arg.Replace("-", string.Empty);
                        Argument argStruct = GetRegisteredArg(argName);

                        if (argStruct.Name != null)
                        {
                            FilledArgument finalArg = new FilledArgument();
                            finalArg.Arg = argStruct;
                            finalArg.Value = null;
                            parsedArgs.Add(finalArg);
                        }
                    }
                }
                else 
                {
                    FilledArgument finalArg = new FilledArgument();
                    finalArg.Arg = new Argument() { Name = "%DEFAULT%", ShortName = null };
                    finalArg.Value = arg;
                    parsedArgs.Add(finalArg);
                }
            }
            
            return parsedArgs.ToArray();
        }
    }
}