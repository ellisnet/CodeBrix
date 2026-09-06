<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › DSP building blocks</sub>

# DSP building blocks

**`CodeBrix.Audio.Dsp` holds the analysis primitives [CodeBrix.Audio](../CodeBrix.Audio.md) offers a
signal-processing job: a fast Fourier transform, biquad filters, an envelope follower and
energy-based voice-activity detection, plus a windowed FFT over a stream of samples and a handful of
utilities.** These are primitives, not finished detectors: there is no turnkey onset, pitch or beat
detector here and no audio-to-MIDI transcriber. You build those on top.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |

## What it does

- `FastFourierTransform` and `Complex` - forward and inverse FFT, transformed in place.
- `FftProcessor` and `FftWindowType` - a windowed FFT over a stream of samples, when you want a
  spectrum rather than a single transform.
- `BiQuadFilter` - low-pass, high-pass, band-pass, peaking and shelving.
- `EnvelopeFollower` - amplitude envelope tracking, a good basis for drum-hit and onset detection.
- `VoiceActivityDetector` - energy-based activity detection.
- `Decibels` - linear amplitude to and from decibels.
- `CircularBuffer` - the ring buffer `BufferedWaveProvider` is built on.

## When to use it

Use these when you are analyzing audio rather than playing it: driving a visualizer, isolating a
frequency band before onset detection, gating on speech, measuring a level. They take float samples,
so pair them with [`AudioFileReader`](reading-and-writing-files.md) or with `.ToSampleProvider()` on
any reader.

What is deliberately absent: no turnkey onset, pitch or beat detector and no audio-to-MIDI
transcriber. `MidiAudioAlignment` is not one of those either - it measures how far an **existing**
transcription sits from its audio, and produces no notes. See
[Multi-track songs and stems](multi-track-and-suno.md) for it.

There are also no visualization widgets. There is an FFT and there are filters; drawing a spectrum or
a waveform is your UI framework's job. The bundled Engine has visualization components of its own -
see [The bundled audio engine](audio-engine.md) - and its `VoiceActivityDetector` is a different type
from this one, in a different namespace.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Dsp;        // FFT, biquad filters, analysis primitives
```

## Key concepts

### An FFT transformed in place

`FastFourierTransform.FFT` transforms in place over a `Complex[]` you own, and sizes are powers of two
with `m = log2(size)`:

```csharp
using CodeBrix.Audio.Dsp;

// FFT magnitude spectrum (size must be a power of two; m = log2(size))
const int m = 10, size = 1 << m;
var bins = new Complex[size];
for (int i = 0; i < size; i++) bins[i].X = samples[i];   // .Y left 0 for real input
FastFourierTransform.FFT(forward: true, m, bins);
double mag0 = Math.Sqrt(bins[8].X * bins[8].X + bins[8].Y * bins[8].Y);
```

Allocate the array once and reuse it rather than per frame; the same goes for the float buffers you
pass to `Read`.

### Filters, envelopes and activity

```csharp
// Biquad filter (e.g. isolate a frequency band before onset detection)
var lowPass = BiQuadFilter.LowPassFilter(sampleRate: 44100, cutoffFrequency: 1000f, q: 0.707f);
float filtered = lowPass.Transform(inputSample);

// Envelope follower (good basis for drum-hit / onset detection)
var env = new EnvelopeFollower(attackMilliseconds: 5f, releaseMilliseconds: 50f, sampleRate: 44100);
float amplitude = env.ProcessSample(inputSample);

// Voice/activity detection (energy-based; needs a quiet stretch first to learn the floor)
var vad = new VoiceActivityDetector(sampleRate: 44100);
bool active = vad.Process(inputSample);
```

`VoiceActivityDetector` is energy-based, so it needs a quiet stretch first to learn the floor.

### A windowed spectrum over a stream

`FftProcessor` with a `FftWindowType` gives a windowed FFT over a stream of samples, which is what a
running spectrum wants rather than a single transform of one block.

## Pitfalls

- **FFT sizes are powers of two,** and the transform runs in place over an array you own. Allocate it
  once.
- **`VoiceActivityDetector` collides by name across the two assemblies.** This one is
  `CodeBrix.Audio.Dsp.VoiceActivityDetector`; the Engine has its own in
  `CodeBrix.Audio.Engine.Components`. They are not the same type and there is no conversion - see
  [The bundled audio engine](audio-engine.md).
- **These are primitives.** A detector that decides where a beat is, or what pitch is sounding, is
  yours to write on top of them.
- **Keep the audio thread clean.** If you analyze inside a source's `Read` or a message hook, that
  code runs on the real-time audio callback: no allocation you can avoid, no locks another thread
  holds for long, no UI marshalling.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| DSP tests | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). For the provenance and licensing of open source code included in
this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Reading and writing files](reading-and-writing-files.md) - where the samples you analyze come from
- [The bundled audio engine](audio-engine.md) - the other FFT, and the visualization components
- [Multi-track songs and stems](multi-track-and-suno.md) - the alignment estimator, which is analysis with a job
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
