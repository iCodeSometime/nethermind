﻿//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Threading.Tasks;
using Nethermind.Db.Config;
using Nethermind.Db.Databases;
using Nethermind.Logging;
using Nethermind.Store;

namespace Nethermind.Db
{
    public class RocksDbProvider : IDbProvider
    {
        private readonly ILogManager _logManager;

        public RocksDbProvider(ILogManager logManager)
        {
            _logManager = logManager;
        }

        public async Task Init(string basePath, IDbConfig dbConfig, bool useReceiptsDb)
        {
            HashSet<Task> allInitializers = new HashSet<Task>();
            allInitializers.Add(Task.Run(() => BlocksDb = new BlocksRocksDb(basePath, dbConfig, _logManager)));
            allInitializers.Add(Task.Run(() => HeadersDb = new HeadersRocksDb(basePath, dbConfig, _logManager)));
            allInitializers.Add(Task.Run(() => BlockInfosDb = new BlockInfosRocksDb(basePath, dbConfig, _logManager)));
            allInitializers.Add(Task.Run(() => StateDb = new StateDb(new StateRocksDb(basePath, dbConfig, _logManager))));
            allInitializers.Add(Task.Run(() => CodeDb = new StateDb(new CodeRocksDb(basePath, dbConfig, _logManager))));
            allInitializers.Add(Task.Run(() => PendingTxsDb = new PendingTxsRocksDb(basePath, dbConfig, _logManager)));
            allInitializers.Add(Task.Run(() => ConfigsDb = new ConfigsRocksDb(basePath, dbConfig, _logManager)));
            allInitializers.Add(Task.Run(() => EthRequestsDb = new EthRequestsRocksDb(basePath, dbConfig, _logManager)));
            allInitializers.Add(Task.Run(() => BloomDb = new BloomRocksDb(basePath, dbConfig, _logManager)));

            allInitializers.Add(Task.Run(() =>
            {
                if (useReceiptsDb)
                {
                    ReceiptsDb = new ReceiptsRocksDb(basePath, dbConfig, _logManager);
                }
                else
                {
                    ReceiptsDb = new ReadOnlyDb(new MemDb(), false);
                }
            }));

            await Task.WhenAll(allInitializers);
        }

        public ISnapshotableDb StateDb { get; private set; }
        public ISnapshotableDb CodeDb { get; private set; }
        public IDb ReceiptsDb { get; private set; }
        public IDb BlocksDb { get; private set; }
        public IDb HeadersDb { get; private set; }
        public IDb BlockInfosDb { get; private set; }
        public IDb PendingTxsDb { get; private set; }
        public IDb ConfigsDb { get; private set; }
        public IDb EthRequestsDb { get; private set; }
        public IColumnsDb<byte> BloomDb { get; private set; }

        public void Dispose()
        {
            StateDb?.Dispose();
            CodeDb?.Dispose();
            ReceiptsDb?.Dispose();
            BlocksDb?.Dispose();
            HeadersDb?.Dispose();
            BlockInfosDb?.Dispose();
            PendingTxsDb?.Dispose();
            ConfigsDb?.Dispose();
            EthRequestsDb?.Dispose();
            BloomDb?.Dispose();
        }
    }
}