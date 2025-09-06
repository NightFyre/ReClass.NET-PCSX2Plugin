using ReClassNET.CodeGenerator;
using ReClassNET.Logger;
using ReClassNET.Nodes;
using System.Collections.Generic;
using System;

namespace PCSX2Plugin
{
    public class PS2CodeGenerator : CustomCppCodeGenerator
    {
        // Map supported node types to how they should be printed
        private static readonly Dictionary<Type, Func<BaseNode, string>> TypePrinters =
            new Dictionary<Type, Func<BaseNode, string>>
            {
            { typeof(PS2PtrNode), node => $"class {((PS2PtrNode)node).InnerNode.Name} *" },
            { typeof(PS2ScratchPtrNode), node => $"class {((PS2ScratchPtrNode)node).InnerNode.Name} *" }
            };

        public override bool CanHandle(BaseNode node)
        {
            return TypePrinters.ContainsKey(node.GetType());
        }

        public override BaseNode TransformNode(BaseNode node)
        {
            // No transformation needed for these custom nodes
            return node;
        }

        public override string GetTypeDefinition(
            BaseNode node,
            GetTypeDefinitionFunc defaultGetTypeDefinitionFunc,
            ResolveWrappedTypeFunc defaultResolveWrappedTypeFunc,
            ILogger logger)
        {
            if (TypePrinters.TryGetValue(node.GetType(), out var printer))
            {
                return printer(node);
            }

            logger.Log(LogLevel.Error, $"No code generator for node type: {node.GetType().Name}");
            return null; // fallback to nothing
        }
    }
}
