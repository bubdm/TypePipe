﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Reflection;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Instances of this interface manage the code generated by the pipeline.
  /// </summary>
  /// <remarks>
  /// Implementations of this interface must not use other pipeline APIs themselves as this could cause deadlocks in a multi-threaded environment.
  /// </remarks>
  /// <threadsafety static="true" instance="false"/>
  public interface IGeneratedCodeFlusher
  {
    /// <summary>
    /// Saves all types that have been generated since the last call to this method into a new <see cref="Assembly"/> on disk.
    /// The file name of the assembly is derived from <see cref="PipelineSettings.AssemblyNamePattern"/> plus the file ending <c>.dll</c>.
    /// The assembly is written to the directory defined by <see cref="PipelineSettings.AssemblyDirectory"/>.
    /// If <see cref="PipelineSettings.AssemblyDirectory"/> is <see langword="null"/>, the assembly is saved in the current working directory.
    /// </summary>
    /// <param name="assemblyAttributes">A number of custom <see cref="Attribute"/>s that are attached to the assembly.</param>
    /// <remarks>
    /// If no new types have been generated since the last call to <see cref="FlushCodeToDisk"/>, this method does nothing
    /// and returns <see langword="null"/>.
    /// </remarks>
    /// <returns>The absolute path to the saved assembly file, or <see langword="null"/> if no assembly was saved.</returns>
    string FlushCodeToDisk (IEnumerable<CustomAttributeDeclaration> assemblyAttributes);
  }
}