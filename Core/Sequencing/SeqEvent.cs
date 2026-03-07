using System;
using System.Diagnostics;
using audiotest.Core.Sequencing.EventData;

namespace audiotest.Core.Sequencing;

public enum SeqEventType
{
    NoteOn = 0x01,
    NoteOff = 0x02,
    NoteBend = 0x03,
    
    InstVol = 0x10,
    InstPan = 0x11,
    InstBend = 0x12,
    InstControl = 0x13,
    
    ProjectBpm = 0xE0
}

public class SeqEvent
{
    public SeqEventType Type;
    
    /*
     * this probably isn't a good idea... dynamic fields are boxed whenever a
     * new value is assigned to them and that might hinder performance when
     * users are going to make patterns with potentially hundreds of events
     *
     * eh it's worth a shot anyway
     */
    private dynamic _payload;

    public SeqEvent(SeqEventType type)
    {
        Type = type;
    }
    
    #region Note events
    
    #region Constructors
    
    public static SeqEvent NoteOn(NoteData data)
    {
        return new SeqEvent(SeqEventType.NoteOn)
        {
            _payload = data
        };
    }

    public static SeqEvent NoteOn(byte instRef, byte velocity, byte note)
    {
        return NoteOn(new NoteData
        {
            InstrumentRef = instRef,
            Velocity = velocity,
            Note = note
        });
    }

    public static SeqEvent NoteOn(byte instRef, byte velocity, Note note)
    {
        return NoteOn(instRef, velocity, note.Number);
    }

    public static SeqEvent NoteOff(NoteData data)
    {
        return new SeqEvent(SeqEventType.NoteOff)
        {
            _payload = data
        };
    }

    public static SeqEvent NoteOff(byte instRef, byte velocity, byte note)
    {
        return NoteOff(new NoteData
        {
            InstrumentRef = instRef,
            Velocity = velocity,
            Note = note
        });
    }

    public static SeqEvent NoteOff(byte instRef, byte velocity, Note note)
    {
        return NoteOff(instRef, velocity, note.Number);
    }

    public static SeqEvent NoteBend(NoteBendData data)
    {
        return new SeqEvent(SeqEventType.NoteBend)
        {
            _payload = data
        };
    }

    public static SeqEvent NoteBend(byte instRef, byte note, short relativePitch)
    {
        return NoteBend(new NoteBendData
        {
            InstrumentRef = instRef,
            Note = note,
            RelativePitch = relativePitch
        });
    }

    public static SeqEvent NoteBend(byte instRef, Note note, short relativePitch)
    {
        return NoteBend(instRef, note.Number, relativePitch);
    }
    
    #endregion
    
    #region Getters
    public NoteData GetNoteData()
    {
        if (_payload is not NoteData data)
            throw new InvalidOperationException($"expected NoteData payload, got {_payload.GetType().Name}");
        return data;
    }

    public NoteBendData GetNoteBendData()
    {
        if (_payload is not NoteBendData data)
            throw new InvalidOperationException($"expected NoteBendData payload, got {_payload.GetType().Name}");
        return data;
    }
    #endregion
    
    #endregion

    #region Instrument events

    #region Constructors

    public SeqEvent InstVol(byte instRef, ushort vol)
    {
        return new SeqEvent(SeqEventType.InstVol)
        {
            _payload = new InstVolumeData
            {
                InstrumentRef = instRef, Volume = vol
            }
        };
    }

    public SeqEvent InstPan(byte instRef, sbyte pan)
    {
        return new SeqEvent(SeqEventType.InstPan)
        {
            _payload = new InstPanData
            {
                InstrumentRef = instRef, Panning = pan
            }
        };
    }

    public SeqEvent InstBend(byte instRef, short relativePitch)
    {
        return new SeqEvent(SeqEventType.InstBend)
        {
            _payload = new InstBendData
            {
                InstrumentRef = instRef, RelativePitch = relativePitch
            }
        };
    }

    public SeqEvent InstControl(InstControlData data)
    {
        data.AssertValid();
        return new SeqEvent(SeqEventType.InstControl)
        {
            _payload = data
        };
    }

    public SeqEvent InstControl(byte instRef, string param, double value)
    {
        return InstControl(new InstControlData
        {
            InstrumentRef = instRef,
            Param = param,
            NewDoubleValue = value
        });
    }

    public SeqEvent InstControl(byte instRef, string param, bool value)
    {
        return InstControl(new InstControlData
        {
            InstrumentRef = instRef,
            Param = param,
            NewBoolValue = value
        });
    }

    public SeqEvent InstControl(byte instRef, string param, string value)
    {
        return InstControl(new InstControlData
        {
            InstrumentRef = instRef,
            Param = param,
            NewStringValue = value
        });
    }

    public SeqEvent InstControlActionTrigger(byte instRef, string param)
    {
        return InstControl(new InstControlData
        {
            InstrumentRef = instRef,
            Param = param,
            TriggerAction = true
        });
    }

    #endregion

    #region Getters

    public InstVolumeData GetInstVolumeData()
    {
        if (_payload is not InstVolumeData data)
            throw new InvalidOperationException(
                $"expected InstVolumeData payload, got {_payload.GetType().Name}");
        return data;
    }

    public InstPanData GetInstPanData()
    {
        if (_payload is not InstPanData data)
            throw new InvalidOperationException(
                $"expected InstPanData payload, got {_payload.GetType().Name}");
        return data;
    }

    public InstBendData GetInstBendData()
    {
        if (_payload is not InstBendData data)
            throw new InvalidOperationException(
                $"expected InstBendData payload, got {_payload.GetType().Name}");
        return data;
    }

    public InstControlData GetInstControlData()
    {
        if (_payload is not InstControlData data)
            throw new InvalidOperationException(
                $"expected InstControlData payload, got {_payload.GetType().Name}");
        return data;
    }

    #endregion

    #endregion

    public SeqEvent ProjectBpm(double bpm)
    {
        return new SeqEvent(SeqEventType.ProjectBpm)
        {
            _payload = new ProjectBpmData { BPM = bpm }
        };
    }

    public ProjectBpmData GetProjectBpmData()
    {
        if (_payload is not ProjectBpmData data)
            throw new InvalidOperationException($"expected ProjectBpmData payload, got {_payload.GetType().Name}");
        return data;
    }
}