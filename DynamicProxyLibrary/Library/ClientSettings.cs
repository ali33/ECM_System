namespace Microsoft.Tools.TestClient
{
    using System;
    using System.Reflection;

    internal static class ClientSettings
    {
        private static Assembly clientAssembly;

        internal static Type GetType(string typeName)
        {
            Type type = ClientAssembly.GetType(typeName);
            if (type == null)
            {
                type = Type.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
                foreach (AssemblyName name in ClientAssembly.GetReferencedAssemblies())
                {
                    Assembly assembly = Assembly.Load(name);
                    if (assembly != null)
                    {
                        type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                }
            }
            return type;
        }

        internal static Assembly ClientAssembly
        {
            get
            {
                if (clientAssembly == null)
                {
                    string data = (string) AppDomain.CurrentDomain.GetData("clientAssemblyPath");
                    AssemblyName assemblyRef = new AssemblyName {
                        CodeBase = data
                    };
                    clientAssembly = Assembly.Load(assemblyRef);
                }
                return clientAssembly;
            }
        }
    }
}

