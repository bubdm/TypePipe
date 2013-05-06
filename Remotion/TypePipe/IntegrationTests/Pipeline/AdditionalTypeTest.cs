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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [Ignore ("TODO 5553")]
  [TestFixture]
  public class AdditionalTypeTest : IntegrationTestBase
  {
    [Test]
    public void CachedType_ViaParticipantState ()
    {
      var additionalTypeID = new object();
      var additionalType = ReflectionObjectMother.GetSomeType();

      var participant1 = CreateParticipant (additionalTypeFunc: (id, ctx) =>
          {
            ctx.State["key"] = additionalTypeID;
            return null;
          });
      var participant2 = CreateParticipant (additionalTypeFunc: (id, ctx) =>
          {
            Assert.That (id, Is.SameAs (additionalTypeID));
            return additionalType;
          });
      var participant3 = CreateParticipant (additionalTypeFunc: (id, ctx) =>
          {
            Assert.Fail ("Should not be called.");
            return null;
      });
      var pipeline = CreatePipeline (participant1, participant2, participant3);

      var result = pipeline.ReflectionService.GetAdditionalType (additionalTypeID);

      Assert.That (result, Is.SameAs (additionalType));
    }

    [Test]
    public void NewlyGeneratedType ()
    {
      
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "No participant provided an additional type for the given identifier.")]
    public void NoParticipantProvidesAdditionalType ()
    {
      var pipeline = CreatePipeline(/* no participants */);
      pipeline.ReflectionService.GetAdditionalType (new object());
    }
  }
}