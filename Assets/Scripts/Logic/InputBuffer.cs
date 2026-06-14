using System.Collections.Generic;

public class BufferedInput
{
    public CombatInputType Input;
    public float Timestamp;        // 记录时的 TotalTime

    public BufferedInput(CombatInputType input, float timestamp)
    {
        Input = input;
        Timestamp = timestamp;
    }
}

public enum CombatInputType
{
    LightAttack,
    HeavyAttack,
    Dodge,
    Special
}

public class InputBuffer
{
    private readonly List<BufferedInput> _buffer = new(4);
    private float _bufferWindow;   // 缓冲有效时长（如 0.15s）

    public InputBuffer(float bufferWindow = 0.15f)
    {
        _bufferWindow = bufferWindow;
    }

    /// <summary>记录一次输入</summary>
    public void Record(CombatInputType input, float currentTime)
    {
        _buffer.Add(new BufferedInput(input, currentTime));
    }

    /// <summary>查看最早的未过期输入（不消耗）</summary>
    public BufferedInput Peek(float currentTime)
    {
        ClearExpired(currentTime);
        return _buffer.Count > 0 ? _buffer[0] : null;
    }

    /// <summary>消耗最早的输入</summary>
    public void Consume()
    {
        if (_buffer.Count > 0)
            _buffer.RemoveAt(0);
    }

    /// <summary>清空所有输入</summary>
    public void Clear()
    {
        _buffer.Clear();
    }

    private void ClearExpired(float currentTime)
    {
        for (int i = _buffer.Count - 1; i >= 0; i--)
        {
            if (currentTime - _buffer[i].Timestamp > _bufferWindow)
                _buffer.RemoveAt(i);
        }
    }
}