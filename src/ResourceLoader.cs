/*
 * Convenience wrapper for loading binary resources via Assembly.GetManifestResourceStream().
 */
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Globalization;

namespace DragWheel
{
    internal class ResourceLoader
    {
        Assembly _currAssem = null;

        public ResourceLoader( )
        {
            _currAssem = Assembly.GetExecutingAssembly();
        }

        public Stream TryLoadResourceBinary( string name )
        {
            string resPrefix = @"res://";

            if (name.StartsWith(resPrefix))
            {
                // Convert eg. "res://Sounds/throttleExtent.wav" to "DragWheel.Sounds.throttleExtent.wav"
                string defaultNamespace = typeof(ResourceLoader).Namespace;
                string resname = defaultNamespace + "." + name.Substring(resPrefix.Length).Replace('/', '.');

                return _currAssem.GetManifestResourceStream(resname);
            }
            else
            {
                if (File.Exists(name))
                    return File.OpenRead(name);

                return null;
            }
        }

        public string TryLoadResourceUtf8String( string name )
        {
            Stream stream = TryLoadResourceBinary(name);
            if (stream == null) 
                return null;

            using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                string text = reader.ReadToEnd();
                return text;
            }
        }

    }
}