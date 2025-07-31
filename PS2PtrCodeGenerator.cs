using ReClassNET.CodeGenerator;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace PCSX2Plugin
{
	public class PS2PtrCodeGenerator : CustomCppCodeGenerator
	{
		public override bool CanHandle(BaseNode node)
		{
			return node is PS2PtrNode;
		}
		
		public override BaseNode TransformNode(BaseNode node)
		{
			return node;
		}
		
		public override string GetTypeDefinition(BaseNode node, GetTypeDefinitionFunc defaultGetTypeDefinitionFunc, ResolveWrappedTypeFunc defaultResolveWrappedTypeFunc, ILogger logger)
		{
			return $"class {((PS2PtrNode)node).InnerNode.Name} *";
		}
	}
}
