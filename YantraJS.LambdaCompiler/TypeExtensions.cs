﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace YantraJS
{
    internal static class TypeExtensions
    {

        public static string Quoted(this string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var che in text)
            {
                switch(che)
                {
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        sb.Append(che);
                        break;
                }
            }
            return $"\"{sb.ToString()}\"";
        }

        public static ConstructorInfo GetConstructor(this Type type, params Type[] args)
            => type.GetConstructor(args);


        public static string GetFriendlyName(this Type? type)
        {
            if (type == null)
                return "";
            if(type.IsArray)
            {
                return type.GetElementType().GetFriendlyName() + "[]";
            }
            if(type.IsConstructedGenericType)
            {
                var a = string.Join(", ", type.GetGenericArguments().Select(x => x.GetFriendlyName()));
                return $"{type.Name}<{a}>";
            }
            if(type.IsGenericTypeDefinition)
            {
                return $"{type.Name}<>";
            }
            return type.Name;
        }
    }

}