﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSystems.Turtle
{
    public class StringParser
    {
        public bool AllowParameters { get; set; }
               
        public Func<char, object[], object> CharToObject { get; set; }

        public StringParser()
        {
            this.AllowParameters = true;
            this.CharToObject = ((a, b) => null);            
        }

        public List<object> Produce(string text)
        {
            List<object> result = new List<object>();          
            if (AllowParameters)
            {
                for (int i = 0; i < text.Length;)
                {
                    char c = text[i];
                    ++i;

                    object[] parameters = null;

                    if (i < text.Length)
                    {
                        if (text[i] == '(')
                        {
                            // Detected start of parameters list.
                            for (int j = i + 1; j < text.Length; ++j)
                            {
                                if (text[j] == ')')
                                {
                                    // Detected end of parameters list.
                                    string[] parametersString = text.Substring(i + 1, j - i - 1).Split(',');
                                    if (parametersString.Length > 0)
                                    {
                                        List<object> parametersList = new List<object>();
                                        foreach (string s in parametersString)
                                        {
                                            double parameter = 0;
                                            double.TryParse(s, out parameter);
                                            parametersList.Add(parameter);
                                        }
                                        parameters = parametersList.ToArray();
                                    }
                                    i = j + 1;
                                    break;
                                }
                            }
                        }
                    }

                    object item = TransformCharToObject(c, parameters);
                    if (item != null) { result.Add(item); }
                }
            }
            else 
            {
                foreach (char c in text)
                {
                    object item = TransformCharToObject(c, null);
                    if (item != null) { result.Add(item); }
                }
            }
            return result;
        }

        private object TransformCharToObject(char c, object[] parameters)
        {
            return CharToObject(c, parameters) ?? DefaultReplacement(c, parameters);
        }

        static Dictionary<char, Type> stdTypes = new Dictionary<char, Type>()
        {
             { '[', typeof(StartBranchModule) },
             { ']', typeof(EndBranchModule) },
             { '%', typeof(CutModule) },
             { 'F', typeof(F) },
             { 'f', typeof(f) },
             { '+', typeof(TurnLeft) },
             { '-', typeof(TurnRight) },
             { '|', typeof(TurnAround) },
             { '^', typeof(PitchUp) },
             { '&', typeof(PitchDown) },
             { '\\', typeof(RollLeft) },
             { '<', typeof(RollLeft) },
             { '/', typeof(RollRight) },
             { '>', typeof(RollRight) },
             { '{', typeof(SurfaceBegin) },
             { '}', typeof(SurfaceEnd) }
        };

        private object DefaultReplacement(char c, object[] parameters)
        {            
            Type type = null;

            if (!stdTypes.TryGetValue(c, out type))
            {
                return null;
            }

            return Activator.CreateInstance(type, parameters);
        }
    }
}