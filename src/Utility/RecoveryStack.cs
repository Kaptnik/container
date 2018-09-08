﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Unity.Build.Policy;
using Unity.Exceptions;

namespace Unity.Utility
{
    /// <summary>
    /// An implementation of <see cref="IRecoveryStack"/>.
    /// </summary>
    public class RecoveryStack : IRecoveryStack
    {
        private readonly Stack<IRequireRecovery> _recoveries = new Stack<IRequireRecovery>();
        private readonly object _lockObj = new object();

        /// <summary>
        /// Add a new <see cref="IRequireRecovery"/> object to this
        /// list.
        /// </summary>
        /// <param name="recovery">Object to add.</param>
        public void Add(IRequireRecovery recovery)
        {
            lock (_lockObj)
            {
                _recoveries.Push(recovery ?? throw new ArgumentNullException(nameof(recovery)));
            }
        }

        /// <summary>
        /// Return the number of recovery objects currently in the stack.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return _recoveries.Count;
                }
            }
        }

        /// <summary>
        /// Execute the <see cref="IRequireRecovery.Recover"/> method
        /// of everything in the recovery list. Recoveries will execute
        /// in the opposite order of add - it's a stack.
        /// </summary>
        public void ExecuteRecovery()
        {
            while (_recoveries.Count > 0)
            {
                _recoveries.Pop().Recover();
            }
        }
    }
}
