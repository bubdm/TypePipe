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
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using Remotion.Collections;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="IProxySerializationEnabler"/>.
  /// </summary>
  public class ProxySerializationEnabler : IProxySerializationEnabler
  {
    private const string c_serializationKeyPrefix = "<tp>";

    private static readonly MethodInfo s_getValueMethod =
        MemberInfoFromExpressionUtility.GetMethod ((SerializationInfo obj) => obj.GetValue ("", null));
    private static readonly MethodInfo s_onDeserializationMethod =
        MemberInfoFromExpressionUtility.GetMethod ((IDeserializationCallback obj) => obj.OnDeserialization (null));

    public void MakeSerializable (MutableType mutableType, MethodInfo initializationMethod)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);
      // initializationMethod may be null

      var implementsSerializable = mutableType.IsAssignableTo (typeof (ISerializable));
      var implementsDeserializationCallback = mutableType.IsAssignableTo (typeof (IDeserializationCallback));
      var serializedFields = mutableType
          .AddedFields
          .Where (f => !f.IsStatic && !f.GetCustomAttributes (typeof (NonSerializedAttribute), false).Any())
          .ToArray();
      var hasInstanceInitializations = initializationMethod != null;

      if (implementsSerializable && serializedFields.Length != 0)
      {
        var serializedFieldMapping = GetFieldSerializationKeys (serializedFields).ToArray();
        OverrideGetObjectData (mutableType, serializedFieldMapping);
        AdaptDeserializationConstructor (mutableType, serializedFieldMapping);
      }

      if (hasInstanceInitializations)
      {
        if (implementsDeserializationCallback)
          OverrideOnDeserialization (mutableType, initializationMethod);
        else if (mutableType.IsSerializable)
          ExplicitlyImplementOnDeserialization (mutableType, initializationMethod);
      }
    }

    private static void ExplicitlyImplementOnDeserialization (MutableType mutableType, MethodInfo initializationMethod)
    {
      mutableType.AddInterface (typeof (IDeserializationCallback));
      mutableType.AddExplicitOverride (s_onDeserializationMethod, ctx => Expression.Call (ctx.This, initializationMethod));
    }

    private static void OverrideOnDeserialization (MutableType mutableType, MethodInfo initializationMethod)
    {
      var onDeserializationOverride = mutableType.GetOrAddMutableMethod (s_onDeserializationMethod);
      onDeserializationOverride.SetBody (ctx => Expression.Block (typeof (void), ctx.PreviousBody, Expression.Call (ctx.This, initializationMethod)));
    }

    private void OverrideGetObjectData (MutableType mutableType, IEnumerable<Tuple<string, FieldInfo>> serializedFieldMapping)
    {
      var interfaceMethod = MemberInfoFromExpressionUtility.GetMethod ((ISerializable obj) => obj.GetObjectData (null, new StreamingContext()));
      var getObjectDataOverride = mutableType.GetOrAddMutableMethod (interfaceMethod);

      if (!getObjectDataOverride.CanSetBody)
      {
        throw new NotSupportedException (
            "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
            + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.");
      }

      getObjectDataOverride.SetBody (ctx => BuildSerializationBody (ctx.This, ctx.Parameters[0], ctx.PreviousBody, serializedFieldMapping));
    }

    private void AdaptDeserializationConstructor (MutableType mutableType, IEnumerable<Tuple<string, FieldInfo>> serializedFieldMapping)
    {
      var deserializationConstructor = mutableType.GetConstructor (
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
          null,
          new[] { typeof (SerializationInfo), typeof (StreamingContext) },
          null);
      if (deserializationConstructor == null)
        throw new InvalidOperationException ("The underlying type implements 'ISerializable' but does not define a deserialization constructor.");

      var mutableConstructor = mutableType.GetMutableConstructor (deserializationConstructor);
      mutableConstructor.SetBody (ctx => BuildDeserializationBody (ctx.This, ctx.Parameters[0], ctx.PreviousBody, serializedFieldMapping));
    }

    private IEnumerable<Tuple<string, FieldInfo>> GetFieldSerializationKeys (IEnumerable<FieldInfo> serializedFields)
    {
      return serializedFields
          .ToLookup (f => f.Name)
          .SelectMany (
              fieldsByName =>
              {
                var fields = fieldsByName.ToArray();

                var serializationKeyProvider =
                    fields.Length == 1
                        ? (Func<FieldInfo, string>) (f => c_serializationKeyPrefix + f.Name)
                        : (f => string.Format ("{0}{1}@{2}", c_serializationKeyPrefix, f.Name, f.FieldType.FullName));

                return fields.Select (f => Tuple.Create (serializationKeyProvider (f), f));
              });
    }

    private Expression BuildSerializationBody (
        Expression @this, Expression serializationInfo, Expression previousBody, IEnumerable<Tuple<string, FieldInfo>> serializedFieldMapping)
    {
      var expressions = serializedFieldMapping.Select (fm => SerializeField (@this, serializationInfo, fm.Item1, fm.Item2));
      return Expression.Block (typeof (void), new[] { previousBody }.Concat (expressions));
    }

    private Expression BuildDeserializationBody (
        Expression @this, Expression serializationInfo, Expression previousBody, IEnumerable<Tuple<string, FieldInfo>> serializedFieldMapping)
    {
      var expressions = serializedFieldMapping.Select (fm => DeserializeField (@this, serializationInfo, fm.Item1, fm.Item2));
      return Expression.Block (typeof (void), new[] { previousBody }.Concat (expressions));
    }

    private Expression SerializeField (Expression @this, Expression serializationInfo, string serializationKey, FieldInfo field)
    {
      return Expression.Call (serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant (serializationKey), Expression.Field (@this, field));
    }

    private Expression DeserializeField (Expression @this, Expression serializationInfo, string serializationKey, FieldInfo field)
    {
      var type = field.FieldType;
      return Expression.Assign (
          Expression.Field (@this, field),
          Expression.Convert (
              Expression.Call (serializationInfo, s_getValueMethod, Expression.Constant (serializationKey), Expression.Constant (type)), type));
    }
  }
}