using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
#if UNITY
using UnityEngine;
#endif

namespace ioSS
{
    public static class Const
    {


        //String Boolean for file reading and writing.
        public const string STR_TRUE = "TRUE";
        public const string STR_FALSE = "FALSE";
        public static bool StringToBool(string _strBool)
        {
            if (IsValidBool(_strBool))
            {
                return _strBool.Equals(STR_TRUE) ? true : false;
            }
            else
            {
                ioDebug.Log("StringToBool: Invalid string '" + _strBool + "' throwing exception...");
                Exception e = new Exception("ValidateStringBool: io StringToBool invalid string boolean: '" + _strBool + "'.");
                throw e;     
            } 
        }
        public static bool IsValidBool(string _strBool)
        {
            return (_strBool.Equals(STR_TRUE) || _strBool.Equals(STR_FALSE)) ? true : false;
        }

        

    }

    /*public static class ioError
    {
        //String Boolean
        
       
    }*/

    public static class ioDebug
    {
        public const bool DEBUG_ACTIVE = true;
        public const bool VERBOSE_DEBUG_ACTIVE = true;

        public static void Log(string _msg, bool _verbose = false, ConsoleColor _fgColor = ConsoleColor.Gray, ConsoleColor _bgColor = ConsoleColor.Black)
        {
#if UNITY
			Debug.Log(_msg);
#else
		
            if(DEBUG_ACTIVE && !_verbose) 
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write(DateTime.Now.ToString("M/d-HH:mm:ff") + "- ");
                System.Console.ForegroundColor = _fgColor;
                System.Console.BackgroundColor = _bgColor;
                System.Console.WriteLine(_msg);
                System.Console.ResetColor();
            }
            else if(VERBOSE_DEBUG_ACTIVE && _verbose)
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.Write(DateTime.Now.ToString("M/d-HH:mm:ff") + "- ");
                System.Console.ForegroundColor = _fgColor;
                System.Console.BackgroundColor = _bgColor;
                System.Console.WriteLine(_msg);
                System.Console.ResetColor();
            }
#endif
        }

    }

    /*public static class ioUtility
    {
        public static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
    */
}
