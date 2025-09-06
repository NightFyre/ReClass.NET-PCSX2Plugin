using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ReClassNET.DataExchange.ReClass;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace PCSX2Plugin
{
	public class PS2NodeSerializer : ICustomNodeSerializer
	{
        /// <summary>
        /// Map node types to their XML identifiers.
        /// </summary>
        private static readonly Dictionary<Type, string> NodeTypeToXml = new Dictionary<Type, string>
        {
            { typeof(PS2PtrNode), "PCSX2::PS2Ptr" },
            { typeof(PS2ScratchPtrNode), "PCSX2::PS2ScratchPtr" }
        };

        /// <summary>
        /// Reverse lookup for XML identifiers -> node factory.
        /// </summary>
        private static readonly Dictionary<string, Func<BaseNode>> XmlToNode = new Dictionary<string, Func<BaseNode>>
        {
            { "PCSX2::PS2Ptr", () => new PS2PtrNode() },
            { "PCSX2::PS2ScratchPtr", () => new PS2ScratchPtrNode() }
        };

        /// <summary>Checks if the node can be handled.</summary>
        public bool CanHandleNode(BaseNode node) => NodeTypeToXml.ContainsKey(node.GetType());

        /// <summary>Checks if the element can be handled.</summary>
        public bool CanHandleElement(XElement element)
        {
            var xmlType = element.Attribute(ReClassNetFile.XmlTypeAttribute)?.Value;
            return !string.IsNullOrEmpty(xmlType) && XmlToNode.ContainsKey(xmlType);
        }

        /// <summary>Creates a node from the xml element.</summary>
        public BaseNode CreateNodeFromElement( XElement element, BaseNode parent, IEnumerable<ClassNode> classes, ILogger logger, CreateNodeFromElementHandler defaultHandler )
        {
            var xmlType = element.Attribute(ReClassNetFile.XmlTypeAttribute)?.Value;

            if (xmlType != null && XmlToNode.TryGetValue(xmlType, out var factory))
            {
                return factory();
            }

            logger.Log(LogLevel.Error, $"Unknown PS2 node type: {xmlType}");
            return null;
        }

        /// <summary>Creates a xml element from the node.</summary>
        public XElement CreateElementFromNode( BaseNode node, ILogger logger, CreateElementFromNodeHandler defaultHandler )
        {
            if (NodeTypeToXml.TryGetValue(node.GetType(), out var xmlType))
            {
                return new XElement(
                    ReClassNetFile.XmlNodeElement,
                    new XAttribute(ReClassNetFile.XmlTypeAttribute, xmlType)
                );
            }

            logger.Log(LogLevel.Error, $"Unhandled PS2 node type: {node.GetType().Name}");
            return null;
        }
    }
}
