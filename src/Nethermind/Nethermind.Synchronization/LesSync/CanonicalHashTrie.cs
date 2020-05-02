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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Db;
using Nethermind.Dirichlet.Numerics;
using Nethermind.Serialization.Rlp;
using Nethermind.State.Proofs;
using Nethermind.Trie;

namespace Nethermind.Synchronization.LesSync
{
    public class CanonicalHashTrie: PatriciaTree
    {
        private static readonly ChtDecoder _decoder = new ChtDecoder();
        public static readonly int SectionSize = 32768; // 2**15

        private static readonly byte[] MaxSectionKey = Encoding.ASCII.GetBytes("MaxSection");

        public CanonicalHashTrie(IKeyValueStore db) : base(db, getMaxRootHash(db), true, true)
        {
        }

        public CanonicalHashTrie(IKeyValueStore db, Keccak rootHash) : base(db, rootHash, true, true)
        {
        }

        public void Commit(long sectionIndex)
        {
            StoreRootHash(sectionIndex);
            Commit();
        }

        public CanonicalHashTrie GetSubTrie(long sectionIndex)
        {
            var trie = new CanonicalHashTrie(_keyValueStore, getRootHash(_keyValueStore, sectionIndex));
            var temp = trie.RootHash;
            trie.UpdateRootHash();
            return trie;
        }

        public long GetMaxSectionIndex()
        {
            return getMaxSectionIndex(_keyValueStore);
        }

        public static long GetSectionFromBlockNo(long blockNo) => (blockNo / SectionSize) - 1;

        public byte[][] BuildProof(long blockNo)
        {
            return BuildProof(GetKey(blockNo));
        }

        public byte[][] BuildProof(byte[] key)
        {
            ProofCollector proofCollector = new ProofCollector(key);
            Accept(proofCollector, RootHash, false);
            return proofCollector.BuildResult();
        }

        private void StoreRootHash(long sectionIndex)
        {
            UpdateRootHash();
            _keyValueStore[GetRootHashKey(sectionIndex)] = RootHash.Bytes;
            if (getMaxSectionIndex(_keyValueStore) < sectionIndex)
            {
                setMaxSectionIndex(sectionIndex);
            }
        }

        private static long getMaxSectionIndex(IKeyValueStore db)
        {
            byte[] storeValue = null;
            try
            {
                storeValue = db[MaxSectionKey];
            }
            catch (KeyNotFoundException e) { }
            return storeValue == null ? 0L : storeValue.ToLongFromBigEndianByteArrayWithoutLeadingZeros();
        }

        private void setMaxSectionIndex(long sectionIndex)
        {
            _keyValueStore[MaxSectionKey] = sectionIndex.ToBigEndianByteArrayWithoutLeadingZeros();
        }

        private static Keccak getRootHash(IKeyValueStore db, long sectionIndex)
        {
            byte[] hash = db[GetRootHashKey(sectionIndex)];
            return hash == null ? EmptyTreeHash : new Keccak(hash);
        }

        private static Keccak getMaxRootHash(IKeyValueStore db)
        {
            long maxSection = getMaxSectionIndex(db);
            return maxSection == 0 ? EmptyTreeHash : getRootHash(db, maxSection);
        }

        public void Set(BlockHeader header)
        {
            Set(GetKey(header), GetValue(header));
        }

        public (Keccak, UInt256) Get(long key)
        {
            return Get(GetKey(key));
        }
        
        public (Keccak, UInt256) Get(Span<byte> key)
        {
            var val = base.Get(key);
            return _decoder.Decode(val);
        }

        private static byte[] GetKey(BlockHeader header)
        {
            return GetKey(header.Number);
        }

        private static byte[] GetKey(long key)
        {
            return key.ToBigEndianByteArrayWithoutLeadingZeros().PadLeft(8);
        }

        private static byte[] GetRootHashKey(long key)
        {
            return Bytes.Concat(Encoding.ASCII.GetBytes("RootHash"), GetKey(key));
        }

        private Rlp GetValue(BlockHeader header)
        {
            if (!header.TotalDifficulty.HasValue)
            {
                throw new ArgumentException("Trying to use a header with a null total difficulty in LES Canonical Hash Trie") ;
            }

            return _decoder.Encode((header.Hash, header.TotalDifficulty.Value));
        }
    }
}
