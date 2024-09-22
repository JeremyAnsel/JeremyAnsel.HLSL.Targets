# JeremyAnsel.HLSL.Targets

[![Build status](https://ci.appveyor.com/api/projects/status/hs5p3wsrvaj87kib/branch/master?svg=true)](https://ci.appveyor.com/project/JeremyAnsel/jeremyansel-hlsl-targets/branch/master)
[![NuGet Version](https://img.shields.io/nuget/v/JeremyAnsel.HLSL.Targets)](https://www.nuget.org/packages/JeremyAnsel.HLSL.Targets)
![License](https://img.shields.io/github/license/JeremyAnsel/JeremyAnsel.HLSL.Targets)

JeremyAnsel.HLSL.Targets adds MSBuild support for HLSL compilation.

Description     | Value
----------------|----------------
License         | [The MIT License (MIT)](https://github.com/JeremyAnsel/JeremyAnsel.HLSL.Targets/blob/master/LICENSE.txt)
Source code     | https://github.com/JeremyAnsel/JeremyAnsel.HLSL.Targets
Nuget           | https://www.nuget.org/packages/JeremyAnsel.HLSL.Targets
Build           | https://ci.appveyor.com/project/JeremyAnsel/jeremyansel-hlsl-targets/branch/master

# Usage

To build a hlsl shader file set the "Build Action" property to "HLSL Shader" in the "Advanced" category of the properties window.
Then in the "HLSL" category set the "Shader Profile" property to a shader profile.

# Shader profiles

Supported shader profiles are:
- Compute Shader 5.0
- Domain Shader 5.0
- Geometry Shader 5.0
- Hull Shader 5.0
- Pixel Shader 5.0
- Vertex Shader 5.0
- Compute Shader 4.1
- Geometry Shader 4.1
- Pixel Shader 4.1
- Vertex Shader 4.1
- Compute Shader 4.0
- Geometry Shader 4.0
- Pixel Shader 4.0
- Vertex Shader 4.0
- Pixel Shader 4.0 level 9.1
- Pixel Shader 4.0 level 9.3
- Vertex Shader 4.0 level 9.1
- Vertex Shader 4.0 level 9.3
- Pixel Shader 3.0
- Vertex Shader 3.0
- Pixel Shader 2.0
- Vertex Shader 2.0

# Default shader code

If a shader profile is selected for an empty file then the file will be filled with a default shader code.

# Output filename

On compiling the project the default output filename for a shader file is the filename of the shader file with a ".cso" extension.

# Custom properties

Additional properties are:
- "Object File Output Name" to set the object file output name
- "Inserts Debug Information" to insert debug information into the output code
- "Preprocessor Definitions" to set the preprocessor definitions
- "Entry Point Name" to set the entry point name
- "Treat Warning As Error" to treat warning as error
- "Output Disassembly Shader" to output disassembly shader
- "Output Disassembly Shader in HTML" to output disassembly shader in HTML
