using System.Collections.Generic;
using _Memoriam.Script.Serlalization.Serialization_Types;
using NUnit.Framework;
using TMPro;

namespace _Memoriam.Script.Localization
{
    public interface ILocalization
    {
        public TMP_Text TextToTranslateTMP { get; set; }
        
        public void Translate(Languages language);
    }
}