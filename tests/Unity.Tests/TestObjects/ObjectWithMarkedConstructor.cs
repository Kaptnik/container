﻿
using System;
using Unity.Tests.v5.TestDoubles;

namespace Unity.Tests.v5.TestObjects
{
    internal class ObjectWithMarkedConstructor
    {
        public ObjectWithMarkedConstructor(int notTheInjectionConstructor)
        {
        }

        [InjectionConstructor]
        public ObjectWithMarkedConstructor(string theInjectionConstructor)
        {
            Name = theInjectionConstructor ?? throw new ArgumentNullException(nameof(theInjectionConstructor));
        }

        public string Name { get; }
    }
}
