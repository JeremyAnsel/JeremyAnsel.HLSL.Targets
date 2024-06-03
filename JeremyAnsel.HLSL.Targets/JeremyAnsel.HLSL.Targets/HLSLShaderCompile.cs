using JeremyAnsel.DirectX.D3DCompiler;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JeremyAnsel.HLSL.Targets
{
    public sealed class HLSLShaderCompile : Task
    {
        [Required]
        public ITaskItem[] Source { get; set; }

        public string ShaderProfile { get; set; }

        public bool Debug { get; set; }

        public string ObjectFileOutput { get; set; }

        public string[] PreprocessorDefinitions { get; set; }

        public string EntryPointName { get; set; }

        public bool TreatWarningAsError { get; set; }

        public bool OutputDisassembly { get; set; }

        public bool OutputDisassemblyHtml { get; set; }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(ShaderProfile))
            {
                return true;
            }

            foreach (ITaskItem item in Source)
            {
                Log.LogMessage(MessageImportance.Normal, $"{item.ItemSpec} -> {ObjectFileOutput}");

                WriteDefaultText(item);

                if (!CompileItem(item))
                {
                    return false;
                }

                if (OutputDisassembly)
                {
                    OutputDisassemblyItem(item);
                }

                if (OutputDisassemblyHtml)
                {
                    OutputDisassemblyHtmlItem(item);
                }
            }

            return true;
        }

        private void WriteDefaultText(ITaskItem item)
        {
            string fullPath = item.GetMetadata("FullPath");
            string text = File.ReadAllText(fullPath);

            if (!string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            switch (ShaderProfile.Substring(0, 2))
            {
                case "cs":
                    text = @"
[numthreads(1, 1, 1)]
void main( uint3 DTid : SV_DispatchThreadID )
{
}
";
                    break;

                case "ds":
                    text = @"
struct DS_OUTPUT
{
	float4 vPosition  : SV_POSITION;
};

struct HS_CONTROL_POINT_OUTPUT
{
	float3 vPosition : WORLDPOS; 
};

struct HS_CONSTANT_DATA_OUTPUT
{
	float EdgeTessFactor[3]			: SV_TessFactor;
	float InsideTessFactor			: SV_InsideTessFactor;
};

#define NUM_CONTROL_POINTS 3

[domain(""tri"")]
DS_OUTPUT main(
	HS_CONSTANT_DATA_OUTPUT input,
	float3 domain : SV_DomainLocation,
	const OutputPatch<HS_CONTROL_POINT_OUTPUT, NUM_CONTROL_POINTS> patch)
{
	DS_OUTPUT Output;

	Output.vPosition = float4(
		patch[0].vPosition*domain.x+patch[1].vPosition*domain.y+patch[2].vPosition*domain.z,1);

	return Output;
}
";
                    break;

                case "gs":
                    text = @"
struct GSOutput
{
	float4 pos : SV_POSITION;
};

[maxvertexcount(3)]
void main(
	triangle float4 input[3] : SV_POSITION, 
	inout TriangleStream< GSOutput > output
)
{
	for (uint i = 0; i < 3; i++)
	{
		GSOutput element;
		element.pos = input[i];
		output.Append(element);
	}
}
";
                    break;

                case "hs":
                    text = @"
struct VS_CONTROL_POINT_OUTPUT
{
	float3 vPosition : WORLDPOS;
};

struct HS_CONTROL_POINT_OUTPUT
{
	float3 vPosition : WORLDPOS; 
};

struct HS_CONSTANT_DATA_OUTPUT
{
	float EdgeTessFactor[3]			: SV_TessFactor;
	float InsideTessFactor			: SV_InsideTessFactor;
};

#define NUM_CONTROL_POINTS 3

HS_CONSTANT_DATA_OUTPUT CalcHSPatchConstants(
	InputPatch<VS_CONTROL_POINT_OUTPUT, NUM_CONTROL_POINTS> ip,
	uint PatchID : SV_PrimitiveID)
{
	HS_CONSTANT_DATA_OUTPUT Output;

	Output.EdgeTessFactor[0] = 
		Output.EdgeTessFactor[1] = 
		Output.EdgeTessFactor[2] = 
		Output.InsideTessFactor = 15;

	return Output;
}

[domain(""tri"")]
[partitioning(""fractional_odd"")]
[outputtopology(""triangle_cw"")]
[outputcontrolpoints(3)]
[patchconstantfunc(""CalcHSPatchConstants"")]
HS_CONTROL_POINT_OUTPUT main( 
	InputPatch<VS_CONTROL_POINT_OUTPUT, NUM_CONTROL_POINTS> ip, 
	uint i : SV_OutputControlPointID,
	uint PatchID : SV_PrimitiveID )
{
	HS_CONTROL_POINT_OUTPUT Output;

	Output.vPosition = ip[i].vPosition;

	return Output;
}
";
                    break;

                case "ps":
                    text = @"
float4 main() : SV_TARGET
{
	return float4(1.0f, 1.0f, 1.0f, 1.0f);
}
";
                    break;

                case "vs":
                    text = @"
float4 main( float4 pos : POSITION ) : SV_POSITION
{
	return pos;
}
";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text
                    .Replace("\r\n", "\n")
                    .Replace("\n", "\r\n");

                File.WriteAllText(fullPath, text);
            }
        }

        private bool CompileItem(ITaskItem item)
        {
            string fullPath = item.GetMetadata("FullPath");

            D3DShaderMacro[] defines = null;

            if (PreprocessorDefinitions != null)
            {
                defines = new D3DShaderMacro[PreprocessorDefinitions.Length];

                for (int i = 0; i < PreprocessorDefinitions.Length; i++)
                {
                    string definition = PreprocessorDefinitions[i];
                    string[] parts = definition.Split('=');

                    string key = parts[0];

                    if (parts.Length > 1)
                    {
                        defines[i] = new D3DShaderMacro(key, parts[1]);
                    }
                    else
                    {
                        defines[i] = new D3DShaderMacro(key, string.Empty);
                    }
                }
            }

            byte[] code;
            string errorMessages = null;

            try
            {
                D3DCompileOptions options = Debug ? D3DCompileOptions.Debug : D3DCompileOptions.OptimizationLevel3;

                if (TreatWarningAsError)
                {
                    options |= D3DCompileOptions.WarningsAreErrors;
                }

                D3DCompile.CompileFromFile(
                    fullPath,
                    defines,
                    EntryPointName,
                    ShaderProfile,
                    options,
                    out code,
                    out errorMessages);
            }
            catch (Exception ex)
            {
                if (errorMessages == null)
                {
                    Log.LogError($"{item.ItemSpec}: {ex.Message}");
                }
                else
                {
                    Log.LogError($"{item.ItemSpec}: {errorMessages}");
                }

                return false;
            }

            if (errorMessages != null)
            {
                if (TreatWarningAsError)
                {
                    Log.LogError($"{item.ItemSpec}: {errorMessages}");
                    return false;
                }
                else
                {
                    Log.LogWarning($"{item.ItemSpec}: {errorMessages}");
                }
            }

            if (code != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ObjectFileOutput));
                File.WriteAllBytes(ObjectFileOutput, code);
            }

            return true;
        }

        private void OutputDisassemblyItem(ITaskItem item)
        {
            string asmFileName = Path.ChangeExtension(ObjectFileOutput, ".h");

            Log.LogMessage(MessageImportance.Normal, $"{item.ItemSpec} -> {asmFileName}");

            byte[] code = File.ReadAllBytes(ObjectFileOutput);
            string asm = D3DCompile.Disassemble(code);

            File.WriteAllText(asmFileName, asm);
        }

        private void OutputDisassemblyHtmlItem(ITaskItem item)
        {
            string htmlFileName = Path.ChangeExtension(ObjectFileOutput, ".html");

            Log.LogMessage(MessageImportance.Normal, $"{item.ItemSpec} -> {htmlFileName}");

            byte[] code = File.ReadAllBytes(ObjectFileOutput);
            string html = D3DCompile.Disassemble(code, D3DDisassembleOptions.EnableColorCode);

            File.WriteAllText(htmlFileName, html);
        }
    }
}
