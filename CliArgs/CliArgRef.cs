﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CliArgs
{
    // reflection utility
    public static class CliArgRef
    {

        // returning the type of the startup object. StarupObject is specified at application properties
        public static Type GetStartupObjectType()
        {
            Assembly entry = System.Reflection.Assembly.GetEntryAssembly();
            if (entry == null) return null;
            var mainMethod = entry.EntryPoint;
            if (mainMethod == null) return null;
            return mainMethod.DeclaringType;
        }

        // the key method to use the reflection for the command-line parameters
        // use it as following:
        //
        // static void Main(string[] args)
        // {
        //     if (!CliArgRef.ParseArgs(args))
        //     {
        //         PrintHelp();
        //         return;
        //     }
        //
        // The method would use "startup object" to populate the static fields with the values
        //
        public static bool ParseArgs(string[] args)
        {
            var tp = GetStartupObjectType();
            CliArgRefDescr descr = CliArgRefDescr.Alloc(tp);
            // assuming it's a static class anyway!
            CliArgRefApply apply = new CliArgRefApply(descr, null);
            return CliArgUtils.ParseApply(args, descr, apply);
        }
    }
}