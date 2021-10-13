// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Graphics;

namespace osu.Game.Audio.Effects
{
    public class AudioFilter : Component, ITransformableFilter
    {
        /// <summary>
        /// The maximum cutoff frequency that can be used with a low-pass filter.
        /// This is equal to nyquist - 1hz.
        /// </summary>
        public const int MAX_LOWPASS_CUTOFF = 22049; // nyquist - 1hz

        private readonly AudioMixer mixer;
        private readonly BQFParameters filter;
        private readonly BQFType type;

        private int cutoff;

        /// <summary>
        /// The cutoff frequency of this filter.
        /// </summary>
        public int Cutoff
        {
            get => cutoff;
            set
            {
                if (value == cutoff)
                    return;

                int oldValue = cutoff;
                cutoff = value;

                updateFilter(oldValue, cutoff);
            }
        }

        /// <summary>
        /// A Component that implements a BASS FX BiQuad Filter Effect.
        /// </summary>
        /// <param name="mixer">The mixer this effect should be applied to.</param>
        /// <param name="type">The type of filter (e.g. LowPass, HighPass, etc)</param>
        public AudioFilter(AudioMixer mixer, BQFType type = BQFType.LowPass)
        {
            this.mixer = mixer;
            this.type = type;

            switch (type)
            {
                case BQFType.HighPass:
                    cutoff = 1;
                    break;

                case BQFType.LowPass:
                    cutoff = MAX_LOWPASS_CUTOFF;
                    break;

                default:
                    cutoff = 500; // A default that should ensure audio remains audible for other filters.
                    break;
            }

            filter = new BQFParameters
            {
                lFilter = type,
                fCenter = cutoff,
                fBandwidth = 0,
                fQ = 0.7f // This allows fCenter to go up to 22049hz (nyquist - 1hz) without overflowing and causing weird filter behaviour (see: https://www.un4seen.com/forum/?topic=19542.0)
            };

            // Don't start attached if this is low-pass or high-pass filter (as they have special auto-attach/detach logic)
            if (type != BQFType.LowPass && type != BQFType.HighPass)
                attachFilter();
        }

        private void attachFilter()
        {
            Debug.Assert(!mixer.Effects.Contains(filter));
            mixer.Effects.Add(filter);
        }

        private void detachFilter()
        {
            Debug.Assert(mixer.Effects.Contains(filter));
            mixer.Effects.Remove(filter);
        }

        private void updateFilter(int oldValue, int newValue)
        {
            // Workaround for weird behaviour when rapidly setting fCenter of a low-pass filter to nyquist - 1hz.
            if (type == BQFType.LowPass)
            {
                if (newValue >= MAX_LOWPASS_CUTOFF)
                {
                    detachFilter();
                    return;
                }

                if (oldValue >= MAX_LOWPASS_CUTOFF)
                    attachFilter();
            }

            // Workaround for weird behaviour when rapidly setting fCenter of a high-pass filter to 1hz.
            if (type == BQFType.HighPass)
            {
                if (newValue <= 1)
                {
                    detachFilter();
                    return;
                }

                if (oldValue <= 1)
                    attachFilter();
            }

            var filterIndex = mixer.Effects.IndexOf(filter);

            if (filterIndex < 0) return;

            if (mixer.Effects[filterIndex] is BQFParameters existingFilter)
            {
                existingFilter.fCenter = newValue;

                // required to update effect with new parameters.
                mixer.Effects[filterIndex] = existingFilter;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (mixer.Effects.Contains(filter))
                detachFilter();
        }
    }
}
