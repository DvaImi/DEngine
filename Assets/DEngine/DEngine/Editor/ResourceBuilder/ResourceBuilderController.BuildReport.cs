using DEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;

namespace DEngine.Editor.ResourceTools
{
    public sealed partial class ResourceBuilderController
    {
        private sealed class BuildReport
        {
            private const string BuildReportName = "BuildReport.xml";
            private const string BuildLogName = "BuildLog.txt";

            private string m_BuildReportName = null;
            private string m_LastBuildReportName = null;
            private string m_BuildLogName = null;
            private string m_ProductName = null;
            private string m_CompanyName = null;
            private string m_GameIdentifier = null;
            private string m_UnityVersion = null;
            private string m_ApplicableGameVersion = null;
            private int m_InternalResourceVersion = 0;
            private Platform m_Platforms = Platform.Undefined;
            private AssetBundleCompressionType m_AssetBundleCompression;
            private string m_CompressionHelperTypeName;
            private bool m_AdditionalCompressionSelected = false;
            private bool m_ForceRebuildAssetBundleSelected = false;
            private string m_BuildEventHandlerTypeName;
            private string m_OutputDirectory;
            private BuildAssetBundleOptions m_BuildAssetBundleOptions = BuildAssetBundleOptions.None;
            private StringBuilder m_LogBuilder = null;
            private SortedDictionary<string, ResourceData> m_ResourceDatas = null;
            private SortedDictionary<string, ResourceData> m_ResourceLastDatas = new SortedDictionary<string, ResourceData>();

            public void Initialize(string buildReportPath, string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion,
                Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, SortedDictionary<string, ResourceData> resourceDatas)
            {
                if (string.IsNullOrEmpty(buildReportPath))
                {
                    throw new DEngineException("Build report path is invalid.");
                }

                m_BuildReportName = Utility.Path.GetRegularPath(Path.Combine(buildReportPath, BuildReportName));
                m_BuildLogName = Utility.Path.GetRegularPath(Path.Combine(buildReportPath, BuildLogName));
                m_ProductName = productName;
                m_CompanyName = companyName;
                m_GameIdentifier = gameIdentifier;
                m_UnityVersion = unityVersion;
                m_ApplicableGameVersion = applicableGameVersion;
                m_InternalResourceVersion = internalResourceVersion;
                m_Platforms = platforms;
                m_AssetBundleCompression = assetBundleCompression;
                m_CompressionHelperTypeName = compressionHelperTypeName;
                m_AdditionalCompressionSelected = additionalCompressionSelected;
                m_ForceRebuildAssetBundleSelected = forceRebuildAssetBundleSelected;
                m_BuildEventHandlerTypeName = buildEventHandlerTypeName;
                m_OutputDirectory = outputDirectory;
                m_BuildAssetBundleOptions = buildAssetBundleOptions;
                m_LogBuilder = new StringBuilder();
                m_ResourceDatas = resourceDatas;
            }

            public void LogInfo(string format, params object[] args)
            {
                LogInternal("INFO", format, args);
            }

            public void LogWarning(string format, params object[] args)
            {
                LogInternal("WARNING", format, args);
            }

            public void LogError(string format, params object[] args)
            {
                LogInternal("ERROR", format, args);
            }

            public void LogFatal(string format, params object[] args)
            {
                LogInternal("FATAL", format, args);
            }

            private void LogInternal(string type, string format, object[] args)
            {
                m_LogBuilder.AppendFormat("[{0:HH:mm:ss.fff}][{1}] ", DateTime.UtcNow.ToLocalTime(), type);
                m_LogBuilder.AppendFormat(format, args);
                m_LogBuilder.AppendLine();
            }

            public bool LogLastReport(Platform platform, string buildReportPath)
            {
                m_LastBuildReportName = Utility.Path.GetRegularPath(Path.Combine(buildReportPath, BuildReportName));
                if (!File.Exists(m_LastBuildReportName))
                {
                    return false;
                }
                m_ResourceLastDatas.Clear();
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(m_LastBuildReportName);
                    XmlNode xmlRoot = xmlDocument.SelectSingleNode("DEngine");
                    XmlNode xmlBuildReport = xmlRoot.SelectSingleNode("BuildReport");
                    XmlNode xmlResources = xmlBuildReport.SelectSingleNode("Resources");

                    XmlNodeList xmlNodeList = null;
                    XmlNode xmlNode = null;

                    xmlNodeList = xmlResources.ChildNodes;
                    for (int i = 0; i < xmlNodeList.Count; i++)
                    {
                        xmlNode = xmlNodeList.Item(i);
                        string name = xmlNode.Attributes.GetNamedItem("Name").Value;
                        string variant = xmlNode.Attributes.GetNamedItem("Variant") != null ? xmlNode.Attributes.GetNamedItem("Variant").Value : null;
                        string FullName = variant != null ? Utility.Text.Format("{0}.{1}", name, variant) : name;
                        string fileSystem = xmlNode.Attributes.GetNamedItem("FileSystem") != null ? xmlNode.Attributes.GetNamedItem("FileSystem").Value : null;
                        byte loadType = 0;
                        if (xmlNode.Attributes.GetNamedItem("LoadType") != null)
                        {
                            byte.TryParse(xmlNode.Attributes.GetNamedItem("LoadType").Value, out loadType);
                        }

                        bool packed = false;
                        if (xmlNode.Attributes.GetNamedItem("Packed") != null)
                        {
                            bool.TryParse(xmlNode.Attributes.GetNamedItem("Packed").Value, out packed);
                        }

                        string[] resourceGroups = xmlNode.Attributes.GetNamedItem("ResourceGroups") != null ? xmlNode.Attributes.GetNamedItem("ResourceGroups").Value.Split(',') : new string[0];
                        m_ResourceLastDatas.Add(FullName, new ResourceData(name, variant, fileSystem, (LoadType)loadType, packed, resourceGroups));

                        string strCount = xmlNode.FirstChild.Attributes.GetNamedItem("Count").Value;
                        int count = int.Parse(strCount);
                        for (int j = 0; j < count; j++)
                        {
                            GetAssetData(xmlNode.FirstChild.ChildNodes[j], out string guid, out string sname, out int lenght, out int hashcode, out int metaLength, out int metaHashCode);
                            GetDependencyAssetNames(xmlNode.FirstChild.ChildNodes[j], out string[] dependencyAssetNames);
                            m_ResourceLastDatas[FullName].AddAssetData(guid, sname, lenght, hashcode, dependencyAssetNames, metaLength, metaHashCode);
                        }
                        XmlNode xmlCodes = xmlNode.SelectSingleNode("Codes");
                        for (int k = 0; k < xmlCodes.ChildNodes.Count; k++)
                        {
                            XmlNode item = xmlCodes.ChildNodes.Item(k);
                            int Length = int.Parse(item.Attributes.GetNamedItem("Length").Value);
                            int HashCode = int.Parse(item.Attributes.GetNamedItem("HashCode").Value);
                            int CompressedLength = int.Parse(item.Attributes.GetNamedItem("CompressedLength").Value);
                            int CompressedHashCode = int.Parse(item.Attributes.GetNamedItem("CompressedHashCode").Value);
                            m_ResourceLastDatas[FullName].AddCode(platform, Length, HashCode, CompressedLength, CompressedHashCode);
                        }
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }

            private void GetAssetData(XmlNode xmlNdoe, out string guid, out string name, out int length, out int hashCode, out int metaLength, out int metaHashCode)
            {
                guid = xmlNdoe.Attributes.GetNamedItem("Guid").Value;
                name = xmlNdoe.Attributes.GetNamedItem("Name").Value;
                string strLen = xmlNdoe.Attributes.GetNamedItem("Length").Value;
                int.TryParse(strLen, out length);
                string strCode = xmlNdoe.Attributes.GetNamedItem("HashCode").Value;
                int.TryParse(strCode, out hashCode);
                string metaStrLen = xmlNdoe.Attributes.GetNamedItem("MetaLength") != null ? xmlNdoe.Attributes.GetNamedItem("MetaLength").Value : "0";
                int.TryParse(metaStrLen, out metaLength);
                string metaStrCode = xmlNdoe.Attributes.GetNamedItem("MetaHashCode") != null ? xmlNdoe.Attributes.GetNamedItem("MetaHashCode").Value : "0";
                int.TryParse(metaStrCode, out metaHashCode);
            }

            private void GetDependencyAssetNames(XmlNode xmlNdoe, out string[] names)
            {
                if (xmlNdoe.FirstChild != null)
                {
                    string[] deps = new string[xmlNdoe.FirstChild.ChildNodes.Count];
                    for (int i = 0; i < xmlNdoe.FirstChild.ChildNodes.Count; i++)
                    {
                        deps[0] = xmlNdoe.FirstChild.ChildNodes[i].Attributes.GetNamedItem("Name").Value;
                    }
                    names = deps;
                }
                else
                {
                    names = new string[0];
                }
            }

            internal List<ResourceData> GetDifference(SortedDictionary<string, ResourceData> resourceDatas)
            {
                List<ResourceData> buildResource = new();
                foreach (ResourceData resourceData in resourceDatas.Values)
                {
                    if (CheckResourceUpdate(resourceData))
                    {
                        buildResource.Add(resourceData);
                    }
                }
                return buildResource;
            }

            private bool CheckResourceUpdate(ResourceData resource)
            {
                foreach (ResourceData resourceData in m_ResourceLastDatas.Values)
                {
                    if (resource.Name == resourceData.Name)
                    {
                        if (resource.GetAssetNames().Length != resourceData.GetAssetNames().Length)
                        {
                            return true;
                        }
                        foreach (var curr in resource.GetAssetDatas())
                        {
                            bool existsAsset = false;
                            foreach (var last in resourceData.GetAssetDatas())
                            {
                                if (last.Name == curr.Name)
                                {
                                    existsAsset = true;
                                    if (last.HashCode != curr.HashCode || last.MetaLength != curr.MetaLength || last.MetaHashCode != curr.MetaHashCode)//找到资源并且文件有修改
                                    {
                                        return true;
                                    }
                                }
                            }
                            //未查找到差异资源处理为新增
                            if (!existsAsset)
                            {
                                return true;
                            }
                        }
                        //资源没有变化不需要打包把老的Codes设置进去
                        var codes = resourceData.GetCodes();
                        foreach (var code in codes)
                        {
                            resource.AddCode(code.Platform, code.Length, code.HashCode, code.CompressedLength, code.CompressedHashCode);
                        }
                        return false;
                    }
                }
                return true;
            }

            public void SaveReport()
            {
                XmlElement xmlElement = null;
                XmlAttribute xmlAttribute = null;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

                XmlElement xmlRoot = xmlDocument.CreateElement("DEngine");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlBuildReport = xmlDocument.CreateElement("BuildReport");
                xmlRoot.AppendChild(xmlBuildReport);

                XmlElement xmlSummary = xmlDocument.CreateElement("Summary");
                xmlBuildReport.AppendChild(xmlSummary);

                xmlElement = xmlDocument.CreateElement("ProductName");
                xmlElement.InnerText = m_ProductName;
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("CompanyName");
                xmlElement.InnerText = m_CompanyName;
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("GameIdentifier");
                xmlElement.InnerText = m_GameIdentifier;
                xmlSummary.AppendChild(xmlElement);
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("UnityVersion");
                xmlElement.InnerText = m_UnityVersion;
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("ApplicableGameVersion");
                xmlElement.InnerText = m_ApplicableGameVersion;
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("InternalResourceVersion");
                xmlElement.InnerText = m_InternalResourceVersion.ToString();
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("Platforms");
                xmlElement.InnerText = m_Platforms.ToString();
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("AssetBundleCompression");
                xmlElement.InnerText = m_AssetBundleCompression.ToString();
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("CompressionHelperTypeName");
                xmlElement.InnerText = m_CompressionHelperTypeName;
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("AdditionalCompressionSelected");
                xmlElement.InnerText = m_AdditionalCompressionSelected.ToString();
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("ForceRebuildAssetBundleSelected");
                xmlElement.InnerText = m_ForceRebuildAssetBundleSelected.ToString();
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("BuildEventHandlerTypeName");
                xmlElement.InnerText = m_BuildEventHandlerTypeName;
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("OutputDirectory");
                xmlElement.InnerText = m_OutputDirectory;
                xmlSummary.AppendChild(xmlElement);
                xmlElement = xmlDocument.CreateElement("BuildAssetBundleOptions");
                xmlElement.InnerText = m_BuildAssetBundleOptions.ToString();
                xmlSummary.AppendChild(xmlElement);

                XmlElement xmlResources = xmlDocument.CreateElement("Resources");
                xmlAttribute = xmlDocument.CreateAttribute("Count");
                xmlAttribute.Value = m_ResourceDatas.Count.ToString();
                xmlResources.Attributes.SetNamedItem(xmlAttribute);
                xmlBuildReport.AppendChild(xmlResources);
                foreach (ResourceData resourceData in m_ResourceDatas.Values)
                {
                    XmlElement xmlResource = xmlDocument.CreateElement("Resource");
                    xmlAttribute = xmlDocument.CreateAttribute("Name");
                    xmlAttribute.Value = resourceData.Name;
                    xmlResource.Attributes.SetNamedItem(xmlAttribute);
                    if (resourceData.Variant != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("Variant");
                        xmlAttribute.Value = resourceData.Variant;
                        xmlResource.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAttribute = xmlDocument.CreateAttribute("Extension");
                    xmlAttribute.Value = GetExtension(resourceData);
                    xmlResource.Attributes.SetNamedItem(xmlAttribute);

                    if (resourceData.FileSystem != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("FileSystem");
                        xmlAttribute.Value = resourceData.FileSystem;
                        xmlResource.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAttribute = xmlDocument.CreateAttribute("LoadType");
                    xmlAttribute.Value = ((byte)resourceData.LoadType).ToString();
                    xmlResource.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("Packed");
                    xmlAttribute.Value = resourceData.Packed.ToString();
                    xmlResource.Attributes.SetNamedItem(xmlAttribute);
                    string[] resourceGroups = resourceData.GetResourceGroups();
                    if (resourceGroups.Length > 0)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("ResourceGroups");
                        xmlAttribute.Value = string.Join(",", resourceGroups);
                        xmlResource.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlResources.AppendChild(xmlResource);

                    AssetData[] assetDatas = resourceData.GetAssetDatas();
                    XmlElement xmlAssets = xmlDocument.CreateElement("Assets");
                    xmlAttribute = xmlDocument.CreateAttribute("Count");
                    xmlAttribute.Value = assetDatas.Length.ToString();
                    xmlAssets.Attributes.SetNamedItem(xmlAttribute);
                    xmlResource.AppendChild(xmlAssets);
                    foreach (AssetData assetData in assetDatas)
                    {
                        XmlElement xmlAsset = xmlDocument.CreateElement("Asset");
                        xmlAttribute = xmlDocument.CreateAttribute("Guid");
                        xmlAttribute.Value = assetData.Guid;
                        xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("Name");
                        xmlAttribute.Value = assetData.Name;
                        xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("Length");
                        xmlAttribute.Value = assetData.Length.ToString();
                        xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("HashCode");
                        xmlAttribute.Value = assetData.HashCode.ToString();
                        xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("MetaLength");
                        xmlAttribute.Value = assetData.MetaLength.ToString();
                        xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("MetaHashCode");
                        xmlAttribute.Value = assetData.MetaHashCode.ToString();
                        xmlAsset.Attributes.SetNamedItem(xmlAttribute);
                        xmlAssets.AppendChild(xmlAsset);
                        string[] dependencyAssetNames = assetData.GetDependencyAssetNames();
                        if (dependencyAssetNames.Length > 0)
                        {
                            XmlElement xmlDependencyAssets = xmlDocument.CreateElement("DependencyAssets");
                            xmlAttribute = xmlDocument.CreateAttribute("Count");
                            xmlAttribute.Value = dependencyAssetNames.Length.ToString();
                            xmlDependencyAssets.Attributes.SetNamedItem(xmlAttribute);
                            xmlAsset.AppendChild(xmlDependencyAssets);
                            foreach (string dependencyAssetName in dependencyAssetNames)
                            {
                                XmlElement xmlDependencyAsset = xmlDocument.CreateElement("DependencyAsset");
                                xmlAttribute = xmlDocument.CreateAttribute("Name");
                                xmlAttribute.Value = dependencyAssetName;
                                xmlDependencyAsset.Attributes.SetNamedItem(xmlAttribute);
                                xmlDependencyAssets.AppendChild(xmlDependencyAsset);
                            }
                        }
                    }

                    XmlElement xmlCodes = xmlDocument.CreateElement("Codes");
                    xmlResource.AppendChild(xmlCodes);
                    foreach (ResourceCode resourceCode in resourceData.GetCodes())
                    {
                        XmlElement xmlCode = xmlDocument.CreateElement(resourceCode.Platform.ToString());
                        xmlAttribute = xmlDocument.CreateAttribute("Length");
                        xmlAttribute.Value = resourceCode.Length.ToString();
                        xmlCode.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("HashCode");
                        xmlAttribute.Value = resourceCode.HashCode.ToString();
                        xmlCode.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("CompressedLength");
                        xmlAttribute.Value = resourceCode.CompressedLength.ToString();
                        xmlCode.Attributes.SetNamedItem(xmlAttribute);
                        xmlAttribute = xmlDocument.CreateAttribute("CompressedHashCode");
                        xmlAttribute.Value = resourceCode.CompressedHashCode.ToString();
                        xmlCode.Attributes.SetNamedItem(xmlAttribute);
                        xmlCodes.AppendChild(xmlCode);
                    }
                }

                xmlDocument.Save(m_BuildReportName);
                File.WriteAllText(m_BuildLogName, m_LogBuilder.ToString());
            }
        }
    }
}
