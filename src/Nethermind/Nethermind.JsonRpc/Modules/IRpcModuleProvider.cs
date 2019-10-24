﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Reflection;
using Utf8Json;

namespace Nethermind.JsonRpc.Modules
{
    public enum ModuleResolution
    {
        Enabled,
        Disabled,
        Unknown
    }
    
    public interface IRpcModuleProvider
    {
        void Register<T>(IRpcModulePool<T> pool) where T : IModule;
        
        IReadOnlyCollection<IJsonFormatter> Formatters { get; }

        IReadOnlyCollection<ModuleType> Enabled { get; }
        
        IReadOnlyCollection<ModuleType> All { get; }

        ModuleResolution Check(string methodName);
        
        MethodInfo Resolve(string methodName);
        
        IModule Rent(string methodName);
        
        void Return(string methodName, IModule module);
    }
}