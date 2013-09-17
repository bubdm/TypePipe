// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Remotion.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe
{
  /// <summary>
  /// Defines the main entry point of the pipeline.
  /// Implementations are used by application developers to create instances for the types generated by the pipeline.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public interface IPipeline
  {
    /// <summary>
    /// Gets the participant configuration ID which describes the participants and their configuration.
    /// <see cref="IPipeline"/> instances with equal <see cref="ParticipantConfigurationID"/>s must generate equivalent types.
    /// </summary>
    string ParticipantConfigurationID { get; }

    /// <summary>
    /// Gets the configuration settings of the pipeline.
    /// </summary>
    PipelineSettings Settings { get; }

    /// <summary>
    /// Gets the participants used by this <see cref="IPipeline"/> instance.
    /// </summary>
    ReadOnlyCollection<IParticipant> Participants { get; }

    /// <summary>
    /// Gets the <see cref="ICodeManager"/> which supports saving and loading of generated code.
    /// </summary>
    ICodeManager CodeManager { get; }

    /// <summary>
    /// Gets the <see cref="IReflectionService"/> which supports retrieving and analyzing assembled types.
    /// </summary>
    IReflectionService ReflectionService { get; }

    T Create<T> (ParamList constructorArguments = null, bool allowNonPublicConstructor = false) where T : class;
    object Create (Type requestedType, ParamList constructorArguments = null, bool allowNonPublicConstructor = false);

    object Create (AssembledTypeID typeID, ParamList constructorArguments = null, bool allowNonPublicConstructor = false);

    /// <summary>
    /// Prepares an externally created instance of an assembled type that was not created by invoking a constructor.
    /// For example, an instance that was created via <see cref="FormatterServices"/>.<see cref="FormatterServices.GetUninitializedObject"/>
    /// during deserialization.
    /// </summary>
    /// <param name="instance">The assembled type instance which should be prepared.</param>
    /// <param name="initializationSemantics">The semantics to apply during initialization.</param>
    void PrepareExternalUninitializedObject (object instance, InitializationSemantics initializationSemantics);
  }
}