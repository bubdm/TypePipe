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
using System.Reflection;
using NUnit.Framework;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class FutureTypeTemplateTest
  {
    [Test]
    public void GetBaseType ()
    {
      var baseType = typeof (IDisposable);
      var typeTemplate = FutureTypeTemplateObjectMother.Create (baseType: baseType);

      Assert.That (typeTemplate.GetBaseType(), Is.SameAs (baseType));
    }

    [Test]
    public void GetAttributeFlags ()
    {
      var attributes = TypeAttributes.Sealed;
      var typeTemplate = FutureTypeTemplateObjectMother.Create (attributes: attributes);

      Assert.That (typeTemplate.GetAttributeFlags(), Is.EqualTo (attributes));
    }

    [Test]
    public void GetInterfaces ()
    {
      var interfaces = new Type[1];
      var typeTemplate = FutureTypeTemplateObjectMother.Create (interfaces: interfaces);

      Assert.That (typeTemplate.GetInterfaces(), Is.SameAs (interfaces));
    }

    [Test]
    public void GetFields ()
    {
      var fields = new FieldInfo[1];
      var typeTemplate = FutureTypeTemplateObjectMother.Create (fields: fields);

      Assert.That (typeTemplate.GetFields(BindingFlags.Default), Is.SameAs (fields));
    }

    [Test]
    public void GetConstructors ()
    {
      var constructors = new ConstructorInfo[1];
      var typeTemplate = FutureTypeTemplateObjectMother.Create (constructors: constructors);

      Assert.That (typeTemplate.GetConstructors (BindingFlags.Default), Is.SameAs (constructors));
    }
  }
}