﻿using SixLabors.ImageSharp;

namespace Pmad.HugeImages.Storage
{
    internal sealed class PersistentUniqueImageStorageSlot : UniqueImageStorageSlotBase
    {
        public PersistentUniqueImageStorageSlot(string path, Image preloaded, HugeImageSettingsBase settings)
            : base(path, preloaded, settings)
        {
        }
    }
}
