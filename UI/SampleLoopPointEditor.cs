using System;
using System.Collections.Generic;
using System.Linq;
using audiotest.Core.Instruments;
using Godot;
using Range = Godot.Range;

namespace audiotest.UI;

public partial class SampleLoopPointEditor : Control
{
    public SamplerInstrument Instrument;

    [Export] public SpinBox StartPointSpinBox;
    [Export] public SpinBox EndPointSpinBox;

    [Export] public TextureRect WaveformContainer;
    [Export] public Control WaveformLoading;

    [Export] public Range StartCuePoint;
    [Export] public Range EndCuePoint;

    private const int WaveformSizeX = 2048;
    private const int WaveformSizeY = 512;

    private SubViewport? _waveformViewport;

    public override void _Ready()
    {
        Instrument.SampleLoaded += () => Reload();
        Reload();
    }

    private Texture2D CreateWaveform()
    {
        if (_waveformViewport is not null)
        {
            RemoveChild(_waveformViewport);
            _waveformViewport = null;
        }

        _waveformViewport = new SubViewport();
        _waveformViewport.Size = new Vector2I(WaveformSizeX, WaveformSizeY);
        _waveformViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        _waveformViewport.RenderTargetClearMode = SubViewport.ClearMode.Once;
        _waveformViewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest;
        AddChild(_waveformViewport);

        Line2D waveform = new Line2D();
        waveform.Width = 2.0f;
        waveform.Name = "Waveform";
        
        if (Instrument.Ready)
        {
            List<Vector2> points = new();
            float previousX = -9999;
            float previousY = -9999;
            for (uint i = 0; i < Instrument.SampleLength; i++)
            {
                Vector2 sample = Instrument.ProcessingFunction(i);
                float x = (i / (float)Instrument.SampleLength) * WaveformSizeX;
                float y = (WaveformSizeY / 2.0f) + (sample.X * WaveformSizeY / 2.0f);
                if (previousX < x + 2 && previousY < y + 2 && previousY > y - 2)
                {
                    points.Add(new Vector2(x, y));
                    
                }
                previousX = x;
                previousY = y;
            }

            waveform.Points = points.ToArray();
            points = null;
        }
        else
        {
            waveform.Points = [
                new Vector2(0, WaveformSizeY / 2.0f),
                new Vector2(WaveformSizeX, WaveformSizeY / 2.0f)
            ];
        }
        
        _waveformViewport.AddChild(waveform);

        ViewportTexture tex = _waveformViewport.GetTexture();
        return tex;
    }

    public void Reload()
    {
        WaveformLoading.Visible = true;
        WaveformContainer.Texture = CreateWaveform();
        WaveformLoading.Visible = false;

        if (Instrument.Ready)
        {
            StartPointSpinBox.MinValue = 0;
            StartPointSpinBox.MaxValue = Instrument.SampleLength;
            StartPointSpinBox.Value = Instrument.Params["lpst"].DoubleValue;
            
            EndPointSpinBox.MinValue = 0;
            EndPointSpinBox.MaxValue = Instrument.SampleLength;
            EndPointSpinBox.Value = Instrument.Params["lpen"].DoubleValue;
            
            StartCuePoint.MinValue = 0;
            StartCuePoint.MaxValue = Instrument.SampleLength;
            StartCuePoint.Value = Instrument.Params["lpst"].DoubleValue;
            
            EndCuePoint.MinValue = 0;
            EndCuePoint.MaxValue = Instrument.SampleLength;
            EndCuePoint.Value = Instrument.Params["lpen"].DoubleValue;

            StartPointSpinBox.ValueChanged += val =>
            {
                Instrument.Params["lpst"].DoubleValue = val;
                StartCuePoint.SetValueNoSignal(val);
            };
            
            EndPointSpinBox.ValueChanged += val =>
            {
                Instrument.Params["lpen"].DoubleValue = val;
                EndCuePoint.SetValueNoSignal(val);
            };

            StartCuePoint.ValueChanged += val => StartPointSpinBox.Value = val;
            EndCuePoint.ValueChanged += val => EndPointSpinBox.Value = val;
        }
    }

    public override void _ExitTree()
    {
        if (WaveformContainer.Texture is not null)
        {
            WaveformContainer.Texture = null;
        }
        if (_waveformViewport is not null)
        {
            Line2D waveform = _waveformViewport.GetNode<Line2D>("Waveform");
            waveform.ClearPoints();
            _waveformViewport.RemoveChild(waveform);
            waveform.QueueFree();
            RemoveChild(_waveformViewport);
            _waveformViewport.QueueFree();
            _waveformViewport = null;
        }
    }
}