using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.CodeDom.Compiler;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Specialized;

namespace ImageSource.ILINX.DynamicProxyLibrary
{
    internal class DynamicProxyGenerator
    {
        public Assembly GenerateClientProxyAssemblyForService(Uri metadataAddress, out IEnumerable<ServiceEndpoint> endpoints)
        {
            MetadataExchangeClient mexClient = new MetadataExchangeClient(metadataAddress, MetadataExchangeClientMode.HttpGet);
            mexClient.ResolveMetadataReferences = true;
            MetadataSet metaDocs = mexClient.GetMetadata();

            WsdlImporter importer = new WsdlImporter(metaDocs);
            ServiceContractGenerator generator = new ServiceContractGenerator();

            Collection<ContractDescription> contracts = importer.ImportAllContracts();
            endpoints = importer.ImportAllEndpoints();

            foreach (ContractDescription contract in contracts)
            {
                generator.GenerateServiceContractType(contract);
            }

            if (generator.Errors.Count != 0)
            {
                throw new ApplicationException("There were errors during code compilation.");
            }

            // Write the code dom
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("C#");
            
            //IndentedTextWriter textWriter = new IndentedTextWriter(new StreamWriter(outputFile));
            //codeDomProvider.GenerateCodeFromCompileUnit(generator.TargetCompileUnit, textWriter, options);
            //textWriter.Close();

            // compile to assembly
            string proxyCode = string.Empty;
            using (StringWriter writer = new StringWriter())
            {
                codeDomProvider.GenerateCodeFromCompileUnit(
                        generator.TargetCompileUnit, writer, options);
                writer.Flush();
                proxyCode = writer.ToString();
            }

            return CompileProxy(codeDomProvider, proxyCode);

            //DynamicProxyFactory factory = new DynamicProxyFactory();
            //factory.Endpoints = endpoints;
            //factory.ProxyAssembly = proxyAssembly;
            //DynamicProxy proxyClient = factory.CreateProxy("ISimpleCalculator");
            //object result = proxyClient.CallMethod("Add", 100.00D, 20.00D);
            //Console.WriteLine(result);
        }

        private Assembly CompileProxy(CodeDomProvider codeDomProvider, string proxyCode)
        {
            // reference the required assemblies with the correct path.
            CompilerParameters compilerParams = new CompilerParameters();
            //compilerParams.GenerateInMemory = false;
            AddAssemblyReference(
                typeof(System.ServiceModel.ServiceContractAttribute).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(
                typeof(System.Web.Services.Description.ServiceDescription).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(
                typeof(System.Runtime.Serialization.DataContractAttribute).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(System.Xml.XmlElement).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(System.Uri).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(System.Data.DataSet).Assembly,
                compilerParams.ReferencedAssemblies);

            CompilerResults results =
               codeDomProvider.CompileAssemblyFromSource(compilerParams, proxyCode);

            if ((results.Errors != null) && (results.Errors.HasErrors))
            {
                DynamicProxyException exception = new DynamicProxyException("Compilation error");
                exception.CompilationErrors = ToEnumerable(results.Errors);

                throw exception;
            }

            //this.compilerWarnings = ToEnumerable(results.Errors);
            return Assembly.LoadFile(results.PathToAssembly);
        }

        public string GenerateClientProxyForService(Uri metadataAddress, out IEnumerable<ServiceEndpoint> endpoints)
        {
            MetadataExchangeClient mexClient = new MetadataExchangeClient(metadataAddress, MetadataExchangeClientMode.HttpGet);
            mexClient.ResolveMetadataReferences = true;
            MetadataSet metaDocs = mexClient.GetMetadata();

            WsdlImporter importer = new WsdlImporter(metaDocs);
            ServiceContractGenerator generator = new ServiceContractGenerator();

            Collection<ContractDescription> contracts = importer.ImportAllContracts();
            endpoints = importer.ImportAllEndpoints();

            foreach (ContractDescription contract in contracts)
            {
                generator.GenerateServiceContractType(contract);
            }

            if (generator.Errors.Count != 0)
            {
                throw new ApplicationException("There were errors during code compilation.");
            }

            // Write the code dom
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("C#");

            //IndentedTextWriter textWriter = new IndentedTextWriter(new StreamWriter(outputFile));
            //codeDomProvider.GenerateCodeFromCompileUnit(generator.TargetCompileUnit, textWriter, options);
            //textWriter.Close();

            // compile to assembly
            string proxyCode = string.Empty;
            using (StringWriter writer = new StringWriter())
            {
                codeDomProvider.GenerateCodeFromCompileUnit(
                        generator.TargetCompileUnit, writer, options);
                writer.Flush();
                proxyCode = writer.ToString();
            }

            return CompileProxyToFile(codeDomProvider, proxyCode);

            //DynamicProxyFactory factory = new DynamicProxyFactory();
            //factory.Endpoints = endpoints;
            //factory.ProxyAssembly = proxyAssembly;
            //DynamicProxy proxyClient = factory.CreateProxy("ISimpleCalculator");
            //object result = proxyClient.CallMethod("Add", 100.00D, 20.00D);
            //Console.WriteLine(result);
        }


        private string CompileProxyToFile(CodeDomProvider codeDomProvider, string proxyCode)
        {
            // reference the required assemblies with the correct path.
            CompilerParameters compilerParams = new CompilerParameters();
            //compilerParams.GenerateInMemory = false;
            AddAssemblyReference(
                typeof(System.ServiceModel.ServiceContractAttribute).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(
                typeof(System.Web.Services.Description.ServiceDescription).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(
                typeof(System.Runtime.Serialization.DataContractAttribute).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(System.Xml.XmlElement).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(System.Uri).Assembly,
                compilerParams.ReferencedAssemblies);

            AddAssemblyReference(typeof(System.Data.DataSet).Assembly,
                compilerParams.ReferencedAssemblies);

            CompilerResults results =
               codeDomProvider.CompileAssemblyFromSource(compilerParams, proxyCode);

            if ((results.Errors != null) && (results.Errors.HasErrors))
            {
                DynamicProxyException exception = new DynamicProxyException("Compilation error");
                exception.CompilationErrors = ToEnumerable(results.Errors);

                throw exception;
            }

            //this.compilerWarnings = ToEnumerable(results.Errors);
            return results.PathToAssembly;
        }


        private void AddAssemblyReference(Assembly referencedAssembly, StringCollection refAssemblies)
        {
            string path = Path.GetFullPath(referencedAssembly.Location);
            string name = Path.GetFileName(path);
            if (!(refAssemblies.Contains(name) ||
                  refAssemblies.Contains(path)))
            {
                refAssemblies.Add(path);
            }
        }

        private static IEnumerable<CompilerError> ToEnumerable(CompilerErrorCollection collection)
        {
            if (collection == null) return null;

            List<CompilerError> errorList = new List<CompilerError>();
            foreach (CompilerError error in collection)
                errorList.Add(error);

            return errorList;
        }
    }
}
