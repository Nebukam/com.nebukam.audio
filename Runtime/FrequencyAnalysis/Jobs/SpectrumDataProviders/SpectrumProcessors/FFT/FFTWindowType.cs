using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public enum FFTWindowType
    {
        None,
        Rectangular,
        Welch,
        Bartlett,
        Hanning,
        Hann,
        Hamming,
        Nutall3,
        Nutall4,
        Nutall3A,
        Nutall3B,
        Nutall4A,
        BlackmanHarris,
        BH92,
        Nutall4B,

        SFT3F,
        SFT3M,
        FTNI,
        SFT4F,
        SFT5F,
        SFT4M,
        FTHP,
        HFT70,
        FTSRS,
        SFT5M,
        HFT90D,
        HFT95,
        HFT116D,
        HFT144D,
        HFT169D,
        HFT196D,
        HFT223D,
        HFT248D
    }
}
