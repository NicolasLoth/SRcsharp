using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;


namespace SRcsharp.Library
{
    public static class SRExtensions
    {
        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }
        public static string[] GetFlagNames(this Enum e)
        {
            return e.ToString().Split(new[] { ", " }, StringSplitOptions.None);
        }


        public static SCNVector3 ToSNCVector3(this Vector3 vector)
        {
            return new SCNVector3(vector);
        }

        //public static T Named<T>(this T value, string name) where T : Enum
        //{
        //    T res = (T)Enum.ToObject(typeof(T), ((byte)0));

        //    if (res == null)
        //        throw new Exception("No default enum value with int-value 0 defined");
        //    //var res = (T)0;
        //    //var res1 = res.ThrowIfNull();

        //    foreach (var n in Enum.GetNames(typeof(T)))
        //    {
        //        if (n == name)
        //            res = (T)Enum.Parse(typeof(T), n);
        //    }

        //    //Enum.TryParse(name, out res);
        //    return res;
        //}

        //public static T ThrowIfNull<T>(
        //    this T? argument,
        //    string? message = default,
        //    [CallerArgumentExpression("argument")] string? paramName = default
        //) where T : notnull
        //{
        //    if (argument is null)
        //    {
        //        throw new ArgumentNullException(paramName, message);
        //    }
        //    else
        //    {
        //        return argument;
        //    }
        //}

        //public static T ThrowIfNull<T>(
        //    this T? argument,
        //    string? message = default,
        //    [CallerArgumentExpression("argument")] string? paramName = default
        //) where T : unmanaged
        //{
        //    if (argument is null)
        //    {
        //        throw new ArgumentNullException(paramName, message);
        //    }
        //    else
        //    {
        //        return (T)argument;
        //    }
        //}


        public static List<string> Keywords(this string str)
        {
            var scanner = new Scanner(str);
            var keywords = new List<string>();
            while (!scanner.IsAtEnd)
            {
                var result = scanner.ScanCharacters("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRTSTUVWXYZ".ToCharArray());
                if (result != null)
                {
                    if (!keywords.Contains(result))
                    {
                        keywords.Add(result);
                    }
                }
                scanner.ScanUpToCharacters("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRTSTUVWXYZ".ToCharArray());
            }
            return keywords;
        }

    }
}
