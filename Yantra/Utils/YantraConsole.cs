﻿using System;
using System.Collections.Generic;
using System.Text;
using WebAtoms.CoreJS.Core;

namespace Yantra.Utils
{
    public class YantraConsole
    {

        public static JSValue Log(in Arguments a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a.TryGetAt(i, out var ai))
                    Console.Write(ai);
            }
            Console.WriteLine();
            return JSUndefined.Value;
        }

    }
}