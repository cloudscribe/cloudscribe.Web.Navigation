using System;
using System.IO;
using System.Xml;

namespace MvcSitemapConvert
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "Mvc.sitemap";
            var output = "navigation.xml";
            #region preparation
            if (args != null && args.Length > 0)
            {
                input = args[0];
                if (args.Length > 1)
                {
                    output = args[1];
                }
            }
            if (!File.Exists(input))
            {
                Console.WriteLine("File {0} not exist.", input);
                return;
            }
            if (File.Exists(output))
            {
                try
                {
                    File.Delete(output);
                }
                catch
                {
                    Console.WriteLine("Fail to delete file {0}.", output);
                    return;
                }
            }
            #endregion

            var inputXml = new XmlDocument();
            inputXml.Load(input);

            var outputXml = new XmlDocument();
            Convert(inputXml, outputXml);

            using (var fs = new FileStream(output, FileMode.Create))
            {
                var writer = XmlWriter.Create(fs, new XmlWriterSettings { Indent = true });
                outputXml.Save(writer);
                fs.Close();
            }
            Console.WriteLine("Finished.");
        }

        static void Convert(XmlDocument inputXml, XmlDocument outputXml)
        {
            var inputRoot = inputXml.DocumentElement.FirstChild;
            while (inputRoot.NodeType != XmlNodeType.Element)
            {
                inputRoot = inputRoot.NextSibling;
            }
            var outputRoot = ConvertNode(inputRoot, outputXml, null);
            outputXml.AppendChild(outputRoot);
        }

        static XmlNode ConvertNode(XmlNode inputNode, XmlDocument outputXml, XmlNode parentNode)
        {
            var outputNode = outputXml.CreateNode(XmlNodeType.Element, "NavNode", null);

            CopyAttribute(inputNode, "key", outputXml, outputNode, "key");
            CopyAttribute(inputNode, "title", outputXml, outputNode, "title");
            CopyAttribute(inputNode, "area", outputXml, outputNode, "area", parentNode);
            CopyAttribute(inputNode, "controller", outputXml, outputNode, "controller", parentNode);
            CopyAttribute(inputNode, "action", outputXml, outputNode, "action");
            CopyAttribute(inputNode, "preservedRouteParameters", outputXml, outputNode, "preservedRouteParameters");
            CopyAttribute(inputNode, "url", outputXml, outputNode, "url");
            CopyAttribute(inputNode, "route", outputXml, outputNode, "namedRoute");
            CopyAttribute(inputNode, "clickable", outputXml, outputNode, "clickable");
            CopyAttribute(inputNode, "description", outputXml, outputNode, "menuDescription");
            CopyAttribute(inputNode, "url", outputXml, outputNode, "url");
            CopyAttribute(inputNode, "order", outputXml, outputNode, "order");
            CopyAttribute(inputNode, "roles", outputXml, outputNode, "viewRoles");
            CopyAttribute(inputNode, "targetFrame", outputXml, outputNode, "target");
            CopyAttribute(inputNode, "visibility", outputXml, outputNode, "componentVisibility");

            var outputChildren = outputXml.CreateNode(XmlNodeType.Element, "Children", null);
            if (inputNode.ChildNodes.Count > 0)
            {
                foreach(XmlNode inputChild in inputNode.ChildNodes)
                {
                    if (inputChild.NodeType == XmlNodeType.Element)
                    {
                        var outputChild = ConvertNode(inputChild, outputXml, outputNode);
                        outputChildren.AppendChild(outputChild);
                    }
                    else if (inputChild.NodeType == XmlNodeType.Comment)
                    {
                        var outputComment = outputXml.CreateComment(inputChild.InnerText);
                        outputChildren.AppendChild(outputComment);
                    }
                }
            }
            outputNode.AppendChild(outputChildren);

            return outputNode;
        }

        static void CopyAttribute(XmlNode inputNode, string inputAttrName, 
            XmlDocument outputXml, XmlNode outputNode, string outputAttrName, XmlNode parentNode = null)
        {
            var inputAttr = inputNode.Attributes[inputAttrName];
            if (inputAttr != null)
            {
                var outputAttr = outputXml.CreateAttribute(outputAttrName);
                outputAttr.Value = inputAttr.Value;
                outputNode.Attributes.Append(outputAttr);
            }
            else if (parentNode != null && parentNode.Attributes[outputAttrName] != null)
            {
                var outputAttr = outputXml.CreateAttribute(outputAttrName);
                outputAttr.Value = parentNode.Attributes[outputAttrName].Value;
                outputNode.Attributes.Append(outputAttr);
            }
        }

    }
}
