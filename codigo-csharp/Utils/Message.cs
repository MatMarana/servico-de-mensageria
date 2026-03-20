using MessagePack;
using System;

namespace Utils;

[MessagePackObject]
public class Message
{
    [Key(0)]
    public string message { get; set; } = "";

}